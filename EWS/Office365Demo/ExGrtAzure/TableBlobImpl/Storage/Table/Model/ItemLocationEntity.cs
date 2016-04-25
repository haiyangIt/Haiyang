using EwsFrame.Cache;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using TableBlobImpl.Access;
using TableBlobImpl.Access.Blob;
using TableBlobImpl.Access.Table;
using TableBlobImpl.Cache;

namespace TableBlobImpl.Storage.Table.Model
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Thhis tableName is 
    /// </remarks>
    public class ItemLocationEntity : TableEntity
    {
        [IgnoreProperty()]
        public string ItemId { get; set; }
        [IgnoreProperty()]
        public string ParentFolderId { get; set; }
        public string Location { get; set; }

        public static ItemLocationEntity NewItemLocationEntity(string itemId, string parentFolderId, string location)
        {
            ItemLocationEntity entity = new ItemLocationEntity();
            entity.ItemId = itemId;
            entity.ParentFolderId = parentFolderId;
            entity.Location = location;
            entity.PartitionKey = TableDataAccess.ValidateRowPartitionKey(parentFolderId);
            entity.RowKey = TableDataAccess.ValidateRowPartitionKey(itemId);
            return entity;
        }


        internal static string GetItemLocationTableName(string orignizeName, string parentFolderId)
        {
            return string.Format("{0}{1}", orignizeName, "itemlocation");
        }

        internal static string GetItemContainerName(string folderId, int index)
        {
            return string.Format("{0}{2}{1}", folderId, index, BlobDataAccess.DashChar);
        }

        internal static string GetItemNextContainerName(string containerName)
        {
            string[] array = containerName.Split(BlobDataAccess.DashCharArray, StringSplitOptions.RemoveEmptyEntries);
            int index = Convert.ToInt32(array[1]);
            return GetItemContainerName(array[0], index + 1);
        }

        internal static string GetFolderContainerMappingBlobName(string name)
        {
            return HttpUtility.UrlEncode(name);
        }

        public static string GetLocation(Item item, DateTime thisCatalogTime, bool isNew)
        {
            throw new NotSupportedException();
            //BlobDataAccess dataAccess = new BlobDataAccess(TableBlobDataAccess.BlobClient);
            
            //if (dataAccess == null)
            //    throw new ArgumentNullException("dependencyInfo");

            //ICache cache = MailboxCacheManager.CacheManager.GetCache(item.ParentFolderId.Mailbox.Address, FolderContainerMapCache.CacheName);
            //if (cache == null)
            //{
            //    cache = MailboxCacheManager.CacheManager.NewCache(item.ParentFolderId.Mailbox.Address, FolderContainerMapCache.CacheName, typeof(FolderContainerMapCache));

            //}

            //StringCacheKey itemKey = new StringCacheKey(item.ParentFolderId.UniqueId);
            //object outObj = null;
            //FolderContainerMapping folderCountInfo = null;
            //if (!cache.TryGetValue(itemKey, out outObj))
            //{
            //    outObj = FolderContainerMapping.NewInstance(item.ParentFolderId.UniqueId);
            //    cache.AddKeyValue(itemKey, folderCountInfo);
            //}

            //folderCountInfo = outObj as FolderContainerMapping;

            //if (BlobDataAccess.IsOutOfBlobCountRange(folderCountInfo.ContainerInfo.UsedCount, item.Size))
            //{
            //    folderCountInfo.ContainerInfo.UsedCount += BlobDataAccess.GetBlobCount(item.Size);
            //}
            //else
            //{
            //    folderCountInfo = FolderContainerMapping.NewNextContainer(folderCountInfo);
            //    folderCountInfo.ContainerInfo.UsedCount += BlobDataAccess.GetBlobCount(item.Size);
            //    cache.SetKeyValue(itemKey, folderCountInfo);
            //}

            //return folderCountInfo.ContainerInfo.ContainerName;
        }
    }


    public class ContainerCount
    {
        public string ContainerName { get; set; }
        public int UsedCount { get; set; }

        public static void WriteToStream(ContainerCount obj, Stream streamWriter)
        {
            BinaryWriter writer = new BinaryWriter(streamWriter);

            writer.Write(obj.ContainerName);
            writer.Write(obj.UsedCount);


        }

        public static ContainerCount ReadFromStream(Stream streamReader)
        {
            BinaryReader reader = new BinaryReader(streamReader);

            ContainerCount result = new ContainerCount();
            result.ContainerName = reader.ReadString();
            result.UsedCount = reader.ReadInt32();
            return result;
        }

        

    }

    [Serializable]
    public class FolderContainerMapping
    {
        public string FolderId { get; set; }
        public ContainerCount ContainerInfo { get; set; }

        public static FolderContainerMapping NewInstance(string folderId)
        {
            var result = new FolderContainerMapping();
            result.FolderId = folderId;
            result.ContainerInfo = new ContainerCount();
            result.ContainerInfo.ContainerName = ItemLocationEntity.GetItemContainerName(folderId, 0);
            result.ContainerInfo.UsedCount = 0;
            return result;
        }

        public static FolderContainerMapping NewNextContainer(FolderContainerMapping currentMapping)
        {
            currentMapping.ContainerInfo.ContainerName = ItemLocationEntity.GetItemNextContainerName(currentMapping.ContainerInfo.ContainerName);
            currentMapping.ContainerInfo.UsedCount = 0;
            return currentMapping;
        }

        public static void WriteToStream(FolderContainerMapping obj, Stream streamWriter)
        {
            BinaryWriter writer = new BinaryWriter(streamWriter);

            writer.Write(obj.FolderId);
            ContainerCount.WriteToStream(obj.ContainerInfo, streamWriter);
        }

        public static FolderContainerMapping ReadFromStream(Stream streamReader)
        {
            BinaryReader reader = new BinaryReader(streamReader);

            FolderContainerMapping result = new FolderContainerMapping();
            result.FolderId = reader.ReadString();
            result.ContainerInfo = ContainerCount.ReadFromStream(streamReader);
            return result;

        }

        public static void SerializeList(List<FolderContainerMapping> lists, Stream data)
        {
            foreach (var obj in lists)
            {
                FolderContainerMapping.WriteToStream(obj, data);
            }
        }

        public static List<FolderContainerMapping> DeSerializeList(Stream data)
        {
            List<FolderContainerMapping> result = new List<FolderContainerMapping>();
            while (data.Position < data.Length)
            {
                FolderContainerMapping obj = FolderContainerMapping.ReadFromStream(data);
                result.Add(obj);
            }
            return result;
        }
    }
}