using DataProtectInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EwsDataInterface;
using EwsFrame;
using System.Data.Entity;
using Microsoft.Exchange.WebServices.Data;
using SqlDbImpl.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;
using System.Configuration;
using System.IO;
using SqlDbImpl.Model;
using System.Data.SqlClient;
using System.Data.Entity.Core.EntityClient;
using System.Transactions;

namespace SqlDbImpl
{
    public class CatalogDataAccess : DataAccessBase, ICatalogDataAccess
    {
        private delegate void AddToDbSet<TEntity>(CatalogDbContext context, List<TEntity> lists) where TEntity : class;

        private IServiceContext ServiceContext
        {
            get
            {
                return CatalogFactory.Instance.GetServiceContext();
            }
        }

        public ICatalogJob GetLastCatalogJob(DateTime thisJobStartTime)
        {
            using (var context = new CatalogDbContext(new OrganizationModel() { Name = ServiceContext.AdminInfo.OrganizationName }))
            {
                var lastCatalogInfoQuery = context.Catalogs.Where(c => c.StartTime < thisJobStartTime).OrderByDescending(c => c.StartTime).Take(1);
                var lastCatalogInfo = lastCatalogInfoQuery.FirstOrDefault();
                if (lastCatalogInfo == default(CatalogInfoModel))
                    return null;
                return lastCatalogInfo;
            }
        }

        public void SaveCatalogJob(ICatalogJob catalogJob)
        {
            CatalogInfoModel information = catalogJob as CatalogInfoModel;
            if (information == null)
                throw new ArgumentException("argument type is not right or argument is null", "catalogJob");
            using (var context = new CatalogDbContext(new OrganizationModel() { Name = ServiceContext.AdminInfo.OrganizationName }, SqlConn, false))
            {
                context.Catalogs.Add(information);
                context.SaveChanges();
            }

            SaveModelCache<MailboxModel>(null, true, CacheKeyNameDic[typeof(MailboxModel)], CachPageCountDic[typeof(MailboxModel)], (context, list) => context.Mailboxes.AddRange(list));
            SaveModelCache<FolderModel>(null, true, CacheKeyNameDic[typeof(FolderModel)], CachPageCountDic[typeof(FolderModel)], (context, list) => context.Folders.AddRange(list));
            SaveModelCache<ItemModel>(null, true, CacheKeyNameDic[typeof(ItemModel)], CachPageCountDic[typeof(ItemModel)], (context, list) => context.Items.AddRange(list));
            SaveModelCache<ItemLocationModel>(null, true, CacheKeyNameDic[typeof(ItemLocationModel)], CachPageCountDic[typeof(ItemLocationModel)], (context, list) => context.ItemLocations.AddRange(list));
        }

        private static Dictionary<Type, string> _cacheKeyNameDic;
        private static Dictionary<Type, string> CacheKeyNameDic
        {
            get
            {
                if(_cacheKeyNameDic == null)
                {
                    _cacheKeyNameDic = new Dictionary<Type, string>();
                    _cacheKeyNameDic.Add(typeof(MailboxModel), "SaveMailboxList");
                    _cacheKeyNameDic.Add(typeof(FolderModel), "SaveFolderList");
                    _cacheKeyNameDic.Add(typeof(ItemModel), "SaveItemList");
                    _cacheKeyNameDic.Add(typeof(ItemLocationModel), "SaveItemLocationList");
                }
                return _cacheKeyNameDic;
            }
        }

        private static Dictionary<Type, int> _cachePageCountDic;
        private static Dictionary<Type, int> CachPageCountDic
        {
            get
            {
                if (_cachePageCountDic == null)
                {
                    _cachePageCountDic = new Dictionary<Type, int>();
                    _cachePageCountDic.Add(typeof(MailboxModel), 20);
                    _cachePageCountDic.Add(typeof(FolderModel), 20);
                    _cachePageCountDic.Add(typeof(ItemModel), 100);
                    _cachePageCountDic.Add(typeof(ItemLocationModel), 1);
                }
                return _cachePageCountDic;
            }
        }

        public void SaveFolder(IFolderData folder, IMailboxData mailboxData, IFolderData parentFolderData)
        {
            SaveModel<IFolderData, FolderModel>(folder, CacheKeyNameDic[typeof(FolderModel)], CachPageCountDic[typeof(FolderModel)], (context, lists) => context.Folders.AddRange(lists));
        }

        public void SaveItem(IItemData item, IMailboxData mailboxData, IFolderData parentFolderData)
        {
            SaveModel<IItemData, ItemModel>(item, CacheKeyNameDic[typeof(ItemModel)], CachPageCountDic[typeof(ItemModel)], (context, lists) => context.Items.AddRange(lists));
        }

        private static CloudStorageAccount StorageAccount = CloudStorageAccount.Parse(
    ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);

        internal static CloudBlobClient BlobClient = StorageAccount.CreateCloudBlobClient();

        public readonly BlobDataAccess BlobDataAccessObj = new BlobDataAccess(BlobClient);
        public void SaveItemContent(IItemData item, DateTime startTime, bool isCheckExist = false, bool isExist = false)
        {
            Item itemInEws = item.Data as Item;

            if(isCheckExist)
            {
                if (isExist)
                    return;
            }
            else if(IsItemContentExist(item.ItemId))
                return;

            string containerName = string.Empty;

            var itemOper = CatalogFactory.Instance.NewItemOperatorImpl(itemInEws.Service);
            IServiceContext context = CatalogFactory.Instance.GetServiceContext();
            var itemLocationModel = new ItemLocationModel();
            List<MailLocation> locationInfos = new List<MailLocation>(3);

            MemoryStream binStream = null;
            MemoryStream emlStream = null;
            MailLocation mailLocation = new MailLocation();
            int binStreamLength = 0;
            try
            {
                binStream = new MemoryStream();
                itemOper.ExportItem(itemInEws, binStream, ServiceContext.Argument);
                binStream.Capacity = (int)binStream.Length;
                binStream.Seek(0, SeekOrigin.Begin);
                var binLocation = new ExportItemSizeInfo() { Type = ExportType.TransferBin, Size = (int)binStream.Length };
                mailLocation.AddLocation(binLocation);
                binStreamLength = (int)binStream.Length;

                emlStream = new MemoryStream();
                itemOper.ExportEmlItem(itemInEws, emlStream, ServiceContext.Argument);
                emlStream.Capacity = (int)emlStream.Length;
                emlStream.Seek(0, SeekOrigin.Begin);
                var emlLocation = new ExportItemSizeInfo() { Type = ExportType.Eml, Size = (int)emlStream.Length };
                mailLocation.AddLocation(emlLocation);

                var location = ItemLocationModel.GetLocation(item);
                mailLocation.Path = location;

                string blobNamePrefix = MailLocation.GetBlobNamePrefix(item.ItemId);
                string binBlobName = MailLocation.GetBlobName(ExportType.TransferBin, blobNamePrefix);
                string emlBlobName = MailLocation.GetBlobName(ExportType.Eml, blobNamePrefix);

                BlobDataAccessObj.SaveBlob(location, binBlobName, binStream, true);
                BlobDataAccessObj.SaveBlob(location, emlBlobName, emlStream, true);
            }
            finally
            {
                if(binStream != null)
                {
                    binStream.Close();
                    binStream.Dispose();
                }
                if (emlStream != null)
                {
                    emlStream.Close();
                    emlStream.Dispose();
                }
            }

            itemLocationModel.ItemId = item.ItemId;
            itemLocationModel.ParentFolderId = item.ParentFolderId;
            itemLocationModel.Location = mailLocation.Path;
            itemLocationModel.ActualSize = binStreamLength;

            SaveModel<IItemData, ItemLocationModel>(itemLocationModel, CacheKeyNameDic[typeof(ItemLocationModel)], CachPageCountDic[typeof(ItemLocationModel)], (dbContext, lists) => dbContext.ItemLocations.AddRange(lists), false);
        }

        public bool IsItemContentExist(string itemId)
        {
            using (var context = new CatalogDbContext(new OrganizationModel() { Name = ServiceContext.AdminInfo.OrganizationName }))
            {
                var result = from m in context.ItemLocations
                             where m.ItemId == itemId
                             select m;
                var itemContent = result.FirstOrDefault();
                if(itemContent == default(ItemLocationModel))
                {
                    return false;
                }
                return true;
            }
        }

        public void SaveMailbox(IMailboxData mailboxAddress)
        {
            SaveModel<IMailboxData, MailboxModel>(mailboxAddress, CacheKeyNameDic[typeof(MailboxModel)], CachPageCountDic[typeof(MailboxModel)], (context, lists) => context.Mailboxes.AddRange(lists));
        }

        private void SaveModel<IT, TImpl>(IT data, string keyName, int pageCount, AddToDbSet<TImpl> delegateFunc, bool isInTransaction = true) where TImpl : class, IT
        {
            if (data == null)
            {
                throw new ArgumentException("argument type is not right or argument is null", "folder");
            } 

            TImpl model = (TImpl)data;
            SaveModelCache(model, false, keyName, pageCount, delegateFunc, isInTransaction);
        }

        /// <summary>
        /// batch save informatioin.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="modelData"></param>
        /// <param name="isEnd">is the last data.</param>
        /// <param name="keyName"></param>
        /// <param name="pageCount">the count in each batch save operation.</param>
        /// <param name="delegateFunc"></param>
        private void SaveModelCache<T>(T modelData, bool isEnd, string keyName, int pageCount, AddToDbSet<T> delegateFunc, bool isInTransaction = true) where T : class
        {
            object modelListObject;
            if (!ServiceContext.OtherInformation.TryGetValue(keyName, out modelListObject))
            {
                modelListObject = new List<T>(pageCount);
                ServiceContext.OtherInformation.Add(keyName, modelListObject);
            }
            List<T> modelList = modelListObject as List<T>;
            

            if (modelData != null)
                modelList.Add(modelData);

            if (modelList.Count >= pageCount || isEnd)
            {
                HashSet<string> ids = new HashSet<string>();
                bool isMultiItem = false;
                foreach (var item in modelList)
                {
                    IItemData temp = item as IItemData;
                    if(temp != null){
                        if (ids.Contains(temp.Id))
                        {
                            isMultiItem = true;
                        }
                        else
                            ids.Add(temp.Id);
                    }
                }


                if (isInTransaction)
                {
                    using (var context = new CatalogDbContext(new OrganizationModel() { Name = ServiceContext.AdminInfo.OrganizationName }, SqlConn, false))
                    {
                        delegateFunc(context, modelList);
                        context.SaveChanges();
                    }
                }
                else
                {
                    using (var context = new CatalogDbContext(new OrganizationModel() { Name = ServiceContext.AdminInfo.OrganizationName }))
                    {
                        delegateFunc(context, modelList);
                        context.SaveChanges();
                    }
                }
                modelList.Clear();
            }
        }

        private SqlConnection _sqlConn;
        /// <summary>
        /// Get sql connect and start transaction.
        /// </summary>
        SqlConnection SqlConn
        {
            get
            {
                if(_sqlConn == null)
                {
                    _sqlConn = new SqlConnection(CatalogDbContext.GetConnectString(ServiceContext.AdminInfo.OrganizationName));
                    _sqlConn.Open();
                    var scope = TransactionScope;
                }
                return _sqlConn;
            }
        }

        private TransactionScope _transactionScope;
        private TransactionScope TransactionScope
        {
            get
            {
                if(_transactionScope == null)
                {
                    _transactionScope = new TransactionScope();
                }
                return _transactionScope;
            }
        }

        public override void BeginTransaction()
        {
            
        }

        public override void EndTransaction(bool isCommit)
        {
            if (_transactionScope != null) {
                if (isCommit)
                    TransactionScope.Complete();

                TransactionScope.Dispose();
                SqlConn.Dispose();
            }
        }
        
    }
}
