using DataProtectInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EwsDataInterface;
using SqlDbImpl.Model;
using EwsServiceInterface;
using SqlDbImpl.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;
using System.Configuration;
using System.IO;
using EwsFrame;

namespace SqlDbImpl
{
    public class QueryCatalogDataAccess : DataAccessBase, IQueryCatalogDataAccess
    {
        private delegate IQueryable<T> QueryFunc<T>(CatalogDbContext context);
        
        public ICatalogJob CatalogJob { get; set; }

        private IServiceContext ServiceContext
        {
            get
            {
                return RestoreFactory.Instance.GetServiceContext();
            }
        }

        private IDataConvertFromDb DataConvert
        {
            get
            {
                return RestoreFactory.Instance.NewDataConvert();
            }
        }

        private List<T> QueryDatas<T>(QueryFunc<T> funcObj)
        {
            using (var context = new CatalogDbContext(ServiceContext.AdminInfo.OrganizationName))
            {
                IQueryable<T> query = funcObj(context);
                return query.ToList();
            }
        }

        private T QueryData<T>(QueryFunc<T> funcObj)
        {
            using (var context = new CatalogDbContext(ServiceContext.AdminInfo.OrganizationName))
            {
                IQueryable<T> query = funcObj(context);
                return query.FirstOrDefault();
            }
        }

        public List<IFolderData> GetAllChildFolder(IFolderData parentFolder)
        {
            return QueryDatas<IFolderData>(
                (context) =>
                    {
                        return from f in context.Folders
                               where f.StartTime == CatalogJob.StartTime && f.ParentFolderId == parentFolder.FolderId
                               select f;
                    }
                );
        }

        public List<IItemData> GetAllChildItems(IFolderData parentFolder)
        {
            return GetAllChildItems(parentFolder.FolderId);
        }


        public List<IItemData> GetAllChildItems(string folderId)
        {
            return QueryDatas<IItemData>(
               (context) =>
               {
                   return from f in context.Items
                          where f.StartTime == CatalogJob.StartTime && f.ParentFolderId == folderId
                          select f;
               }
               );
        }

        

        public IItemData GetItem(string itemId)
        {
            return QueryDatas<IItemData>(
               (context) =>
               {
                   return from f in context.Items
                          where f.StartTime == CatalogJob.StartTime && f.ItemId == itemId
                          select f;
               }
               ).FirstOrDefault();
        }

        public List<IMailboxData> GetAllMailbox()
        {
            return QueryDatas<IMailboxData>(
               (context) =>
               {
                   return from f in context.Mailboxes
                          where f.StartTime == CatalogJob.StartTime 
                          select f;
               }
               );
        }

        public List<ICatalogJob> GetAllCatalogJob()
        {
            return QueryDatas<ICatalogJob>(
                (context) =>
                {
                    return from c in context.Catalogs
                           orderby c.StartTime descending
                           select c;
                }
                );
        }

        private static CloudStorageAccount StorageAccount = CloudStorageAccount.Parse(
    ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);

        internal static CloudTableClient TableClient = StorageAccount.CreateCloudTableClient();

        internal static CloudBlobClient BlobClient = StorageAccount.CreateCloudBlobClient();

        public readonly BlobDataAccess BlobDataAccessObj = new BlobDataAccess(BlobClient);
        public IItemData GetItemContent(IItemData itemData)
        {
            return GetItemContent(itemData.ItemId);
        }

        public IItemData GetItemContent(string itemId)
        {
            var result = QueryData<ItemLocationModel>(
               (context) =>
               {
                   return from f in context.ItemLocations
                          where f.ItemId == itemId
                          select f;
               }
               );
            if (result == default(ItemLocationModel))
                return null;

            var location = result.Location;
            byte[] data = null;
            using (MemoryStream stream = new MemoryStream())
            {
                string blobName = MD5Utility.ConvertToMd5(itemId);
                BlobDataAccessObj.GetBlob(location, blobName, stream, result.ActualSize);
                stream.Capacity = (int)stream.Length;
                stream.Seek(0, SeekOrigin.Begin);
                data = stream.ToArray();
            }

            IItemData model = DataConvert.Convert(result, data);
            return model;
        }

        public override void BeginTransaction()
        {
            
        }

        public override void EndTransaction(bool isCommit)
        {
            
        }

        public List<IFolderData> GetAllFoldersInMailboxes(string mailboxAddress)
        {
            return QueryDatas<IFolderData>(
               (context) =>
               {
                   return from f in context.Folders
                          where f.StartTime == CatalogJob.StartTime && f.MailboxAddress == mailboxAddress
                          select f;
               }
               );
        }

    }
}
