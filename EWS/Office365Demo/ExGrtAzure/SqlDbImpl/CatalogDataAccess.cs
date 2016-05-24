using DataProtectInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EwsDataInterface;
using EwsFrame;
using System.Data.Entity;
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
using EwsServiceInterface;
using System.Threading;
using System.Data.Entity.Infrastructure;
using EwsFrame.Util;
using LogInterface;

namespace SqlDbImpl
{
    public class CatalogDataAccess : DataAccessBase, ICatalogDataAccess, IDisposable
    {
        public CatalogDataAccess(EwsServiceArgument argument, string organization)
        {
            _argument = argument;
            _organization = organization;
            _dbContext = new CatalogDbContext(new OrganizationModel() { Name = _organization });
        }


        private DbContext _dbContext;
        private object _lockObj = new object();
        public override DbContext DbContext
        {
            get
            {
                if (_dbContext == null)
                {
                    using (_lockObj.LockWhile(() =>
                    {
                        if (_dbContext == null)
                            _dbContext = new CatalogDbContext(new OrganizationModel() { Name = _organization });
                    }))
                    { }
                }
                return _dbContext;
            }
        }

        public CatalogDbContext CatalogContext
        {
            get
            {
                return DbContext as CatalogDbContext;
            }
        }

        private readonly EwsServiceArgument _argument;
        private readonly string _organization;

        private delegate void AddToDbSet<TEntity>(CatalogDbContext context, List<TEntity> lists) where TEntity : class;

        public ICatalogJob GetLastCatalogJob(DateTime thisJobStartTime)
        {
            //using (var context = new CatalogDbContext(new OrganizationModel() { Name = _organization }))
            //{
            var lastCatalogInfoQuery = CatalogContext.Catalogs.Where(c => c.StartTime < thisJobStartTime).OrderByDescending(c => c.StartTime).Take(1);
            var lastCatalogInfo = lastCatalogInfoQuery.FirstOrDefault();
            if (lastCatalogInfo == default(CatalogInfoModel))
                return null;
            return lastCatalogInfo;
            //}
        }

        public void SaveCatalogJob(ICatalogJob catalogJob)
        {
            using (_lockObj.LockWhile(() =>
            {
                CatalogInfoModel information = catalogJob as CatalogInfoModel;
                if (information == null)
                    throw new ArgumentException("argument type is not right or argument is null", "catalogJob");
                //using (var context = new CatalogDbContext(new OrganizationModel() { Name = _organization }, SqlConn, false))
                //using (var context = new CatalogDbContext(new OrganizationModel() { Name = _organization }))
                //{
                CatalogContext.Catalogs.Add(information);
                //CatalogContext.SaveChanges();
                //}

                SaveModelCache<MailboxModel>(null, true, CacheKeyNameDic[typeof(MailboxModel)], CachPageCountDic[typeof(MailboxModel)], (context, list) => context.Mailboxes.AddRange(list));
                SaveModelCache<FolderModel>(null, true, CacheKeyNameDic[typeof(FolderModel)], CachPageCountDic[typeof(FolderModel)], (context, list) => context.Folders.AddRange(list));
                SaveModelCache<ItemModel>(null, true, CacheKeyNameDic[typeof(ItemModel)], CachPageCountDic[typeof(ItemModel)], (context, list) => context.Items.AddRange(list));
                SaveModelCache<ItemLocationModel>(null, true, CacheKeyNameDic[typeof(ItemLocationModel)], CachPageCountDic[typeof(ItemLocationModel)], (context, list) => context.ItemLocations.AddRange(list));
            }))
            { }
        }

        private static object _diclock = new object();
        private static Dictionary<Type, string> _cacheKeyNameDic;
        private static Dictionary<Type, string> CacheKeyNameDic
        {
            get
            {
                if (_cacheKeyNameDic == null)
                {
                    using (_diclock.LockWhile(() =>
                    {
                        if (_cacheKeyNameDic == null)
                        {

                            _cacheKeyNameDic = new Dictionary<Type, string>();
                            _cacheKeyNameDic.Add(typeof(MailboxModel), "SaveMailboxList");
                            _cacheKeyNameDic.Add(typeof(FolderModel), "SaveFolderList");
                            _cacheKeyNameDic.Add(typeof(ItemModel), "SaveItemList");
                            _cacheKeyNameDic.Add(typeof(ItemLocationModel), "SaveItemLocationList");
                        }
                    }))
                    { }
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
                    using (_diclock.LockWhile(() =>
                    {
                        if (_cachePageCountDic == null)
                        {

                            _cachePageCountDic = new Dictionary<Type, int>();
                            _cachePageCountDic.Add(typeof(MailboxModel), 1);
                            _cachePageCountDic.Add(typeof(FolderModel), 1);
                            _cachePageCountDic.Add(typeof(ItemModel), 100);
                            _cachePageCountDic.Add(typeof(ItemLocationModel), 1);
                        }
                    }))
                    { }
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

        private static CloudStorageAccount StorageAccount = FactoryBase.GetStorageAccount();

        internal static CloudBlobClient BlobClient = StorageAccount.CreateCloudBlobClient();

        public readonly BlobDataAccess BlobDataAccessObj = new BlobDataAccess(BlobClient);

        public IEwsAdapter EwsAdapter { get; set; }
        public void SaveItemContent(IItemData item, string mailboxAddress, DateTime startTime, bool isCheckExist = false, bool isExist = false)
        {

            if (isCheckExist)
            {
                if (isExist)
                    return;
            }
            else if (IsItemContentExist(item.ItemId))
                return;

            string containerName = string.Empty;

            //var itemOper = CatalogFactory.Instance.NewItemOperatorImpl(itemInEws.Service, this);
            var itemLocationModel = new ItemLocationModel();
            List<MailLocation> locationInfos = new List<MailLocation>(3);

            MailLocation mailLocation = new MailLocation();
            int binStreamLength = 0;
            int actualSize = 0;
            byte[] buffer = null;

            buffer = EwsAdapter.ExportItem(item, _argument);

            if (buffer == null || buffer.Length == 0)
                return;

            var location = ItemLocationModel.GetLocation(item);
            string blobNamePrefix = MailLocation.GetBlobNamePrefix(item.ItemId);

            var binLocation = new ExportItemSizeInfo() { Type = ExportType.TransferBin, Size = (int)buffer.Length };
            mailLocation.AddLocation(binLocation);
            binStreamLength = (int)buffer.Length;
            string binBlobName = MailLocation.GetBlobName(ExportType.TransferBin, blobNamePrefix);
            using (MemoryStream binStream = new MemoryStream(buffer))
            {
                BlobDataAccessObj.SaveBlob(location, binBlobName, binStream, true);
            }
            var binLength = buffer.Length;
            buffer = null;

            //buffer = EwsAdapter.ExportEmlItem(item, _argument);
            //var emlLocation = new ExportItemSizeInfo() { Type = ExportType.Eml, Size = (int)buffer.Length };
            //mailLocation.AddLocation(emlLocation);
            //string emlBlobName = MailLocation.GetBlobName(ExportType.Eml, blobNamePrefix);
            //using (MemoryStream emlStream = new MemoryStream(buffer))
            //{
            //    BlobDataAccessObj.SaveBlob(location, emlBlobName, emlStream, true);
            //}

            mailLocation.Path = location;
            actualSize = (int)binLength;// + (int)buffer.Length;
            buffer = null;


            itemLocationModel.ItemId = item.ItemId;
            itemLocationModel.ParentFolderId = item.ParentFolderId;
            itemLocationModel.Location = mailLocation.Path;
            itemLocationModel.ActualSize = binStreamLength;
            ((ItemModel)item).ActualSize = actualSize;

            SaveModel<IItemData, ItemLocationModel>(itemLocationModel,
                CacheKeyNameDic[typeof(ItemLocationModel)],
                CachPageCountDic[typeof(ItemLocationModel)],
                (dbContext, lists) =>
                {
                    dbContext.ItemLocations.AddRange(lists);

                    foreach (var i in lists)
                    {
                        if (!ItemCache.Contains(i.ItemId))
                            ItemCache.Add(i.ItemId);
                    }
                },
                false);
        }

        private HashSet<string> _ItemsCache = null;
        private HashSet<string> ItemCache
        {
            get
            {
                if (_ItemsCache == null)
                {
                    using (_lockObj.LockWhile(() =>
                    {
                        if (_ItemsCache == null)
                        {
                            var list = (from m in CatalogContext.ItemLocations select m.ItemId).ToList();
                            _ItemsCache = new HashSet<string>(list);
                        }
                    }))
                    { }
                }
                return _ItemsCache;
            }
        }

        public bool IsItemContentExist(string itemId)
        {
            // using (var context = new CatalogDbContext(new OrganizationModel() { Name = _organization }))
            // {
            //var boolResult = false;

            return ItemCache.Contains(itemId);
            //using (_lockObj.LockWhile(() =>
            //{
            //    var result = from m in CatalogContext.ItemLocations
            //                 where m.ItemId == itemId
            //                 select m;
            //    var itemContent = result.FirstOrDefault();
            //    if (itemContent == default(ItemLocationModel))
            //    {
            //        boolResult = false;
            //    }
            //    else
            //        boolResult = true;
            //}))
            //{ }
            //return boolResult;
            //}
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

        private Dictionary<string, object> _otherInformation;
        private Dictionary<string, object> OtherInformation
        {
            get
            {
                if (_otherInformation == null)
                {
                    _otherInformation = new Dictionary<string, object>();
                }
                return _otherInformation;
            }
        }

        public object OtherObj
        {
            set
            {
                EwsAdapter = value as IEwsAdapter;
            }
        }

        class ConcurrentSave<T> : IDisposable
        {
            public ReaderWriterLockSlim LockSlim = new ReaderWriterLockSlim();
            public List<T> Datas;

            public ConcurrentSave(int capacity)
            {
                Datas = new List<T>(capacity);
            }

            public void Dispose()
            {
                LockSlim.Dispose();
            }
        }

        public override void SaveChanges()
        {
            bool saveFailed;
            do
            {
                saveFailed = false;

                try
                {
                    CatalogContext.SaveChanges();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    System.Diagnostics.Trace.TraceError(ex.GetExceptionDetail());
                    LogFactory.LogInstance.WriteException(LogInterface.LogLevel.WARN, "Save change error", ex, "Save change error. will retry.");
                    saveFailed = true;

                    //var entry = ex.Entries.Single();
                    //entry.OriginalValues.SetValues(entry.GetDatabaseValues());
                    ex.Entries.Single().Reload();
                }

            } while (saveFailed);
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
            using (_lockObj.LockWhile(() =>
            {
                object modelListObject;
                if (!OtherInformation.TryGetValue(keyName, out modelListObject))
                {
                    modelListObject = new ConcurrentSave<T>(pageCount);
                    OtherInformation.Add(keyName, modelListObject);
                }
                var readWriteLock = ((ConcurrentSave<T>)modelListObject).LockSlim;
                List<T> modelList = ((ConcurrentSave<T>)modelListObject).Datas as List<T>;


                //readWriteLock.EnterReadLock();
                //try
                //{
                if (modelData != null)
                    modelList.Add(modelData);
                //}
                //finally
                //{
                //    readWriteLock.ExitReadLock();
                //}

                //readWriteLock.EnterWriteLock();
                //try
                //{
                if (modelList.Count >= pageCount || isEnd)
                {
                    if (isInTransaction)
                    {
                        //using (var context = new CatalogDbContext(new OrganizationModel() { Name = _organization }))
                        //using (var context = new CatalogDbContext(new OrganizationModel() { Name = _organization }, SqlConn, false))
                        //{
                        delegateFunc(CatalogContext, modelList);
                        // CatalogContext.SaveChanges();
                        //}
                    }
                    else
                    {
                        //using (var context = new CatalogDbContext(new OrganizationModel() { Name = _organization }))
                        //{
                        delegateFunc(CatalogContext, modelList);
                        //  CatalogContext.SaveChanges();
                        //}
                    }
                    modelList.Clear();
                }
                //}
                //finally
                //{
                //    readWriteLock.ExitWriteLock();
                //}
            }))
            { }
        }

        //private SqlConnection _sqlConn;
        ///// <summary>
        ///// Get sql connect and start transaction.
        ///// </summary>
        //SqlConnection SqlConn
        //{
        //    get
        //    {
        //        if(_sqlConn == null)
        //        {
        //            _sqlConn = new SqlConnection(CatalogDbContext.GetConnectString(_organization));
        //            _sqlConn.Open();
        //            //var scope = TransactionScope;
        //        }
        //        return _sqlConn;
        //    }
        //}

        //private TransactionScope _transactionScope;
        //private TransactionScope TransactionScope
        //{
        //    get
        //    {
        //        if(_transactionScope == null)
        //        {
        //            _transactionScope = new TransactionScope();
        //        }
        //        return _transactionScope;
        //    }
        //}

        public override void BeginTransaction()
        {

        }

        public override void EndTransaction(bool isCommit)
        {
            SaveChanges();
            //if (_transactionScope != null) {
            //    if (isCommit)
            //        TransactionScope.Complete();

            //    TransactionScope.Dispose();
            //    SqlConn.Dispose();
            //}

        }

        public override void Dispose()
        {
            foreach (var keyValue in OtherInformation)
            {
                IDisposable obj = keyValue.Value as IDisposable;
                if (obj != null)
                {
                    obj.Dispose();
                    obj = null;
                }
            }
            if (_dbContext != null)
            {
                _dbContext.Dispose();
                _dbContext = null;
            }

        }

        public void UpdateFolderChildFolderItemCount(IFolderData folderData, DateTime startTime)
        {
            using (_lockObj.LockWhile(() =>
            {
                CatalogContext.Folders.Attach((FolderModel)folderData);
                //using (var context = new CatalogDbContext(new OrganizationModel() { Name = _organization }))
                //{
                CatalogContext.Entry((FolderModel)folderData).State = EntityState.Modified;
                //CatalogContext.Folders.Attach((FolderModel) folderData);
                //    var folderDataOld = (from f in CatalogContext.Folders where f.StartTime == startTime && f.FolderId == folderData.FolderId select f).FirstOrDefault();
                //    if (folderDataOld == null)
                //        throw new NullReferenceException("can't find the folder, please add first.");
                // folderDataOld.ChildFolderCount = folderData.ChildFolderCount;
                //folderDataOld.ChildItemCount = folderData.ChildItemCount;
                //context.SaveChanges();
                //}
            }))
            { }
        }

        public void UpdateMailboxChildFolderCount(IMailboxData mailboxData, DateTime startTime)
        {
            using (_lockObj.LockWhile(() =>
            {
                //using (var context = new CatalogDbContext(new OrganizationModel() { Name = _organization }))
                //{
                var mailboxDataOld = (from f in CatalogContext.Mailboxes where f.StartTime == startTime && f.RootFolderId == mailboxData.RootFolderId select f).FirstOrDefault();
                if (mailboxDataOld == null)
                    throw new NullReferenceException("can't find the mailbox, please add first.");
                mailboxDataOld.ChildFolderCount = mailboxData.ChildFolderCount;
                //context.SaveChanges();
                //}
            }))
            { }
        }
    }
}
