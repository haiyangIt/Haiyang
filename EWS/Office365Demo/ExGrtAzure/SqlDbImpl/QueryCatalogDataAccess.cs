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
using System.Data.Entity;

namespace SqlDbImpl
{
    public class QueryCatalogDataAccess : CatalogDataAccessBase, IQueryCatalogDataAccess
    {
        public ICatalogJob CatalogJob { get; set; }

        private string _organization;
        public override string Organization
        {
            get
            {
                if(CatalogJob != null)
                {
                    return CatalogJob.Organization;
                }
                return _organization;
            }
            set
            {
                _organization = value;
            }
        }

        private IDataConvertFromDb DataConvert
        {
            get
            {
                return RestoreFactory.Instance.NewDataConvert();
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

        private static CloudStorageAccount StorageAccount = FactoryBase.GetStorageAccount();

        internal static CloudTableClient TableClient = StorageAccount.CreateCloudTableClient();

        internal static CloudBlobClient BlobClient = StorageAccount.CreateCloudBlobClient();

        public static readonly BlobDataAccess BlobDataAccessObj = new BlobDataAccess(BlobClient);

        [Obsolete("Please use GetItemContent(IItemData, ExportType)")]
        public IItemData GetItemContent(IItemData itemData)
        {
            return GetItemContent(itemData.ItemId, itemData.DisplayName);
        }

        [Obsolete("Please use GetItemContent(string, ExportType)")]
        public IItemData GetItemContent(string itemId, string displayName)
        {
            return GetItemContent(itemId, displayName, ExportType.TransferBin);
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

        public List<IItemData> GetChildItems(string folderId, int pageIndex, int pageCount)
        {
            return QueryData<IItemData>((context) =>
               {
                   return from i in context.Items
                          where i.StartTime == CatalogJob.StartTime && i.ParentFolderId == folderId
                          orderby i.CreateTime
                          select i;

               }, pageIndex, pageCount);
        }

        public int GetChildItemsCount(string folderId)
        {
            return QueryData<IItemData>((context) =>
            {
                return (from i in context.Items
                        where i.StartTime == CatalogJob.StartTime && i.ParentFolderId == folderId
                        select i).Count();
            });
        }

        public List<ICatalogJob> GetCatalogsInOneDay(DateTime day)
        {
            var startTime = new DateTime(day.Year, day.Month, day.Day, 0, 0, 0);
            day = day.AddDays(1);
            var endTime = new DateTime(day.Year, day.Month, day.Day, 0, 0, 0);
            return QueryDatas<ICatalogJob>((context) =>
            {
                return from c in context.Catalogs
                       where c.StartTime >= startTime && c.StartTime < endTime
                       select c;
            });
        }

        public ICatalogJob GetLatestCatalogJob()
        {
            return QueryData<ICatalogJob>((context) =>
            {
                return (from c in context.Catalogs
                        orderby c.StartTime descending
                        select c).Take(1);
            });
        }

        public List<DateTime> GetCatalogDaysInMonth(DateTime startTime)
        {
            var startDay = new DateTime(startTime.Year, startTime.Month, 1, 0, 0, 0);
            var endDay = startDay.AddMonths(1);
            return QueryDatas<DateTime>((context) =>
            {
               
                return (from c in context.Catalogs
                        where c.StartTime >= startDay && c.StartTime < endDay
                        orderby c.StartTime
                        select DbFunctions.TruncateTime(c.StartTime).Value).Distinct();
            });
        }

        public List<IMailboxData> GetAllMailbox(List<string> excludeIds)
        {
            return QueryDatas<IMailboxData>((context) =>
            {
                return from m in context.Mailboxes
                       where !excludeIds.Contains(m.RootFolderId) && m.StartTime == CatalogJob.StartTime
                       select m;
            });
        }

        public List<IFolderData> GetAllChildFolder(string parentFolderId, List<string> excludefolderIds)
        {
            return QueryDatas<IFolderData>((context) =>
            {
                return from f in context.Folders
                       where f.StartTime == CatalogJob.StartTime && f.ParentFolderId == parentFolderId && !excludefolderIds.Contains(f.FolderId)
                       select f;
            });
        }

        public List<IItemData> GetAllChildItems(string parentFolderId, List<string> excludeItemIds)
        {
            return QueryDatas<IItemData>((context) =>
            {
                return from f in context.Items
                       where f.StartTime == CatalogJob.StartTime && f.ParentFolderId == parentFolderId && !excludeItemIds.Contains(f.ItemId)
                       select f;
            });
        }

        public List<IFolderData> GetAllChildFolder(string rootFolderId)
        {
            return QueryDatas<IFolderData>((context) =>
            {
                return from f in context.Folders
                       where f.StartTime == CatalogJob.StartTime && f.ParentFolderId == rootFolderId
                       select f;
            });
        }

        public IItemData GetItemContent(IItemData itemData, ExportType type)
        {
            return GetItemContent(itemData.ItemId, itemData.DisplayName, type);
        }

        public IItemData GetItemContent(string itemId, string displayName, ExportType type)
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
                string blobNamePrefix = MailLocation.GetBlobNamePrefix(itemId);
                string blobName = MailLocation.GetBlobName(type, blobNamePrefix);
                BlobDataAccessObj.GetBlob(location, blobName, stream);
                stream.Capacity = (int)stream.Length;
                stream.Seek(0, SeekOrigin.Begin);
                data = stream.ToArray();
            }

            IItemData model = DataConvert.Convert(result, data);
            return model;
        }
    }
}
