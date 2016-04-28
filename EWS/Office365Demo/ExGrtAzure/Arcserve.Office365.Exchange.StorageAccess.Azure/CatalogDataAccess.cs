using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;
using System.Configuration;
using System.IO;
using System.Data.SqlClient;
using System.Data.Entity.Core.EntityClient;
using Arcserve.Office365.Exchange.StorageAccess.Interface;
using Arcserve.Office365.Exchange.EwsApi;
using Arcserve.Office365.Exchange.Data;
using Arcserve.Office365.Exchange.Data.Mail;
using Arcserve.Office365.Exchange.DataProtect.Interface;
using Arcserve.Office365.Exchange.Azure;
using Arcserve.Office365.Exchange.Data.Impl;
using Arcserve.Office365.Exchange.Data.Impl.Mail;

namespace Arcserve.Office365.Exchange.StorageAccess.Azure
{
    public class CatalogDataAccess : CatalogDataAccessBase, ICatalogDataAccess
    {
        public CatalogDataAccess(EwsServiceArgument argument, string organization) : base(organization)
        {
            _argument = argument;
        }

        private readonly EwsServiceArgument _argument;


        public ICatalogJob GetLastCatalogJob(DateTime thisJobStartTime)
        {
            using (var context = NewCatalogDbContext(false))
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
            using (var context = NewCatalogDbContext(true))
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
                if (_cacheKeyNameDic == null)
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
            BatchSaveModel<IFolderData, FolderModel>(folder, CacheKeyNameDic[typeof(FolderModel)], CachPageCountDic[typeof(FolderModel)], (context, lists) => context.Folders.AddRange(lists));
        }

        public void SaveItem(IItemData item, IMailboxData mailboxData, IFolderData parentFolderData)
        {
            BatchSaveModel<IItemData, ItemModel>(item, CacheKeyNameDic[typeof(ItemModel)], CachPageCountDic[typeof(ItemModel)], (context, lists) => context.Items.AddRange(lists));
        }

        private static CloudStorageAccount StorageAccount = FactoryBase.GetStorageAccount();

        internal static CloudBlobClient BlobClient = StorageAccount.CreateCloudBlobClient();

        public readonly BlobDataAccess BlobDataAccessObj = new BlobDataAccess(BlobClient);
        public void SaveItemContent(IItemData item, DateTime startTime, bool isCheckExist = false, bool isExist = false)
        {
            Item itemInEws = item.Data as Item;

            if (isCheckExist)
            {
                if (isExist)
                    return;
            }
            else if (IsItemContentExist(item.ItemId))
                return;

            string containerName = string.Empty;

            var itemOper = CatalogFactory.Instance.NewItemOperatorImpl(itemInEws.Service, this);
            var itemLocationModel = new ItemLocationModel();
            List<MailLocation> locationInfos = new List<MailLocation>(3);

            MemoryStream binStream = null;
            MemoryStream emlStream = null;
            MailLocation mailLocation = new MailLocation();
            int binStreamLength = 0;
            try
            {
                binStream = new MemoryStream();
                itemOper.ExportItem(itemInEws, binStream, _argument);
                binStream.Capacity = (int)binStream.Length;
                binStream.Seek(0, SeekOrigin.Begin);
                var binLocation = new ExportItemSizeInfo() { Type = ExportType.TransferBin, Size = (int)binStream.Length };
                mailLocation.AddLocation(binLocation);
                binStreamLength = (int)binStream.Length;

                emlStream = new MemoryStream();
                itemOper.ExportEmlItem(itemInEws, emlStream, _argument);
                emlStream.Capacity = (int)emlStream.Length;
                emlStream.Seek(0, SeekOrigin.Begin);
                var emlLocation = new ExportItemSizeInfo() { Type = ExportType.Eml, Size = (int)emlStream.Length };
                mailLocation.AddLocation(emlLocation);

                var location = DataHelper.GetLocation(item, _argument.CurrentMailbox);
                mailLocation.Path = location;

                string blobNamePrefix = MailLocation.GetBlobNamePrefix(item.ItemId);
                string binBlobName = MailLocation.GetBlobName(ExportType.TransferBin, blobNamePrefix);
                string emlBlobName = MailLocation.GetBlobName(ExportType.Eml, blobNamePrefix);

                BlobDataAccessObj.SaveBlob(location, binBlobName, binStream, true);
                BlobDataAccessObj.SaveBlob(location, emlBlobName, emlStream, true);
            }
            finally
            {
                if (binStream != null)
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

            BatchSaveModel<IItemData, ItemLocationModel>(itemLocationModel, CacheKeyNameDic[typeof(ItemLocationModel)], CachPageCountDic[typeof(ItemLocationModel)], (dbContext, lists) => dbContext.ItemLocations.AddRange(lists), false);
        }

        public bool IsItemContentExist(string itemId)
        {
            using (var context = new CatalogDbContext(new OrganizationModel() { Name = Organization }))
            {
                var result = from m in context.ItemLocations
                             where m.ItemId == itemId
                             select m;
                var itemContent = result.FirstOrDefault();
                if (itemContent == default(ItemLocationModel))
                {
                    return false;
                }
                return true;
            }
        }

        public void SaveMailbox(IMailboxData mailboxAddress)
        {
            BatchSaveModel<IMailboxData, MailboxModel>(mailboxAddress, CacheKeyNameDic[typeof(MailboxModel)], CachPageCountDic[typeof(MailboxModel)], (context, lists) => context.Mailboxes.AddRange(lists));
        }
    }
}
