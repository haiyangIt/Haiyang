using ExGrtAzure.EWS.ItemOperator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Exchange.WebServices.Data;
using System.IO;
using ExGrtAzure.EWS.Catalog.Storage;
using ExGrtAzure.EWS.Catalog.Cache;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace ExGrtAzure.EWS.Catalog.Operator
{
    public class ItemOperatorImpl : IItem
    {
        public ItemOperatorImpl(ExchangeService service, string organization)
        {
            CurrentExchangeService = service;
            _organization = organization;
        }

        private readonly string _organization;

        public ExchangeService CurrentExchangeService
        {
            get; set;
        }

        public void ExportItem(Item item, Stream stream)
        {
            var argument = ServiceContext.CurrentContext.GetExchangeServiceArgument();
            ExportUploadHelper.ExportItemPost(Enum.GetName(typeof(ExchangeVersion), item.Service.RequestedServerVersion), item.Id.UniqueId, stream, argument);
        }

        public List<Item> GetFolderItems(Folder folder)
        {
            const int pageSize = 100;
            int offset = 0;
            bool moreItems = true;
            List<Item> result = new List<Item>(folder.TotalCount);
            while (moreItems)
            {
                ItemView oView = new ItemView(pageSize, offset, OffsetBasePoint.Beginning);
                FindItemsResults<Item> findResult = folder.FindItems(oView);
                result.AddRange(findResult.Items);
                if (!findResult.MoreAvailable)
                    moreItems = false;

                if (moreItems)
                    offset += pageSize;
            }
            return result;
        }

        public string GetLocation(Item item, DateTime thisCatalogTime, bool isNew)
        {
            BlobDataAccess dataAccess = ((ExGrtAzure.EWS.Catalog.Storage.DataAccess)ServiceContext.CurrentContext.DataAccessObj).BlobDataAccessObj;
            if (dataAccess == null)
                throw new ArgumentNullException("dependencyInfo");

            ICache cache = MailboxCacheManager.CacheManager.GetCache(item.ParentFolderId.Mailbox.Address, FolderContainerMapCache.CacheName);
            if (cache == null)
            {
                cache = MailboxCacheManager.CacheManager.NewCache(item.ParentFolderId.Mailbox.Address, FolderContainerMapCache.CacheName, typeof(FolderContainerMapCache));
                
            }

            StringCacheKey itemKey = new StringCacheKey(item.ParentFolderId.UniqueId);
            object outObj = null;
            FolderContainerMapping folderCountInfo = null;
            if (!cache.TryGetValue(itemKey, out outObj))
            {
                outObj = FolderContainerMapping.NewInstance(item.ParentFolderId.UniqueId);
                cache.AddKeyValue(itemKey, folderCountInfo);
            }

            folderCountInfo = outObj as FolderContainerMapping;

            if(BlobDataAccess.IsOutOfBlobCountRange(folderCountInfo.ContainerInfo.UsedCount, item.Size))
            {
                folderCountInfo.ContainerInfo.UsedCount += BlobDataAccess.GetBlobCount(item.Size);
            }
            else
            {
                folderCountInfo = FolderContainerMapping.NewNextContainer(folderCountInfo);
                folderCountInfo.ContainerInfo.UsedCount += BlobDataAccess.GetBlobCount(item.Size);
                cache.SetKeyValue(itemKey, folderCountInfo);
            }

            return folderCountInfo.ContainerInfo.ContainerName;
        }

        public void ImportItem(Folder parentFolder, string fileName)
        {
            throw new NotImplementedException();
        }

        public bool IsItemNew(Item item, DateTime lastTime, DateTime thisTime)
        {
            return item.DateTimeCreated > lastTime && item.DateTimeCreated <= thisTime;
        }
    }

}