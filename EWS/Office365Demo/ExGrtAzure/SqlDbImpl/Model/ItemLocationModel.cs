using EwsDataInterface;
using EwsFrame;
using EwsFrame.Cache;
using Microsoft.Exchange.WebServices.Data;
using SqlDbImpl;
using SqlDbImpl.Cache;
using SqlDbImpl.Storage;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Web;
using System.Security.Cryptography;

namespace SqlDbImpl.Model
{
    [Table("ItemLocation")]
    public class ItemLocationModel : IItemData
    {
        [NotMapped]
        public DateTime? CreateTime
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        [NotMapped]
        public object Data
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        [NotMapped]
        public string DisplayName
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        [NotMapped]
        public string ItemClass
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        
        [Required]
        [MaxLength(512)]
        public string ParentFolderId
        {
            get; set;
        }

        [Key]
        [MaxLength(512)]
        public string ItemId { get; set; }

        [Required]
        [MaxLength(256)]
        public string Location { get; set; }

        [NotMapped]
        public int Size { get; set; }

        [Required]
        public int ActualSize { get; set; }

        [NotMapped]
        public string Id
        {
            get
            {
                return ItemId;
            }
            set { }
        }

        [NotMapped]
        public ItemKind ItemKind
        {
            get
            {
                return ItemKind.Item;
            }
            set { }
        }

        internal static string GetItemContainerName(string folderIdMd5Str, int index)
        {
            var context = CatalogFactory.Instance.GetServiceContext();
            return string.Format("{0}{3}{1}{3}{2}",context.GetOrganizationPrefix(), folderIdMd5Str, index, BlobDataAccess.DashChar);
        }

        internal static string GetItemContainerName(IItemData itemData)
        {
            return GetItemContainerName(MD5Utility.ConvertToMd5(itemData.ParentFolderId), 0);
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


        public static string GetLocation(IItemData item)
        {
            return GetItemContainerName(item);
        }

        /// <summary>
        /// This method is based on a hypothesis that all mails write into a block blob who contains many blobs, the blob max size is 4MB and count can't more than 50000.
        /// So if a mail is a block blob. we don't need the limitation.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="actualSize"></param>
        /// <param name="thisCatalogTime"></param>
        /// <returns></returns>
        [Obsolete("This method is for size limitation.")]
        public static string GetLocation(IItemData item, int actualSize, DateTime thisCatalogTime)
        {
            BlobDataAccess dataAccess = new BlobDataAccess(CatalogDataAccess.BlobClient);
            var context = CatalogFactory.Instance.GetServiceContext();
            ICache cache = MailboxCacheManager.CacheManager.GetCache(context.CurrentMailbox, FolderContainerMapCache.CacheName);
            if (cache == null)
            {
                cache = MailboxCacheManager.CacheManager.NewCache(context.CurrentMailbox, FolderContainerMapCache.CacheName, typeof(FolderContainerMapCache));
            }

            StringCacheKey itemKey = new StringCacheKey(item.ParentFolderId);
            object outObj = null;
            FolderContainerMapping folderCountInfo = null;
            if (!cache.TryGetValue(itemKey, out outObj))
            {
                outObj = FolderContainerMapping.NewInstance(item.ParentFolderId);
                cache.AddKeyValue(itemKey, outObj);
            }

            folderCountInfo = outObj as FolderContainerMapping;

            if (BlobDataAccess.IsNotOutOfBlobCountRange(folderCountInfo.ContainerInfo.UsedCount, actualSize))
            {
                folderCountInfo.ContainerInfo.AddUsedCount(actualSize);
            }
            else
            {
                folderCountInfo = FolderContainerMapping.NewNextContainer(folderCountInfo);
                folderCountInfo.ContainerInfo.AddUsedCount(actualSize);
                cache.SetKeyValue(itemKey, folderCountInfo);
            }

            return folderCountInfo.ContainerInfo.ContainerName;
        }

        public IItemData Clone()
        {
            return new ItemLocationModel()
            {
                ParentFolderId = ParentFolderId,
                ItemId = ItemId,
                Location = Location,
                Size = Size,
                ActualSize = ActualSize
            };
        }
    }

    [Obsolete("For blob size limitation.")]
    [Serializable]
    public class FolderContainerMapping
    {
        public string FolderId { get; set; }
        public ContainerCount ContainerInfo { get; set; }

        public static FolderContainerMapping NewInstance(string folderId)
        {
            var result = new FolderContainerMapping();
            result.FolderId = folderId;
            result.ContainerInfo = ContainerCount.NewInstance(folderId, 0);
            return result;
        }

        public static FolderContainerMapping NewNextContainer(FolderContainerMapping currentMapping)
        {
            currentMapping.ContainerInfo = ContainerCount.NewInstanceByPrevContainerName(currentMapping.ContainerInfo.ContainerName);
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

    [Obsolete("For blob size limitation.")]
    public class ContainerCount
    {
        public string ContainerName { get; private set; }
        public int UsedCount { get; private set; }

        private static MD5 _md5Hash = MD5.Create();

        public static ContainerCount NewInstance(string parentFolderId, int index = 0)
        {
            return new ContainerCount(parentFolderId, index, 0);
        }

        public static ContainerCount NewInstanceByPrevContainerName(string prevContainerName, int usedCount = 0)
        {
            var result = new ContainerCount();
            result.ContainerName = ItemLocationModel.GetItemNextContainerName(prevContainerName);
            result.UsedCount = usedCount;
            return result;
        }

        public static ContainerCount NewInstanceFromStream(string containerName, int usedCount = 0)
        {
            var result = new ContainerCount();
            result.ContainerName = containerName;
            result.UsedCount = usedCount;
            return result;
        }

        private ContainerCount()
        {

        }

        private ContainerCount(string parentFolderId, int index, int usedCount)
        {
            ContainerName = GetFolderIdContainerName(parentFolderId);
            ContainerName = ItemLocationModel.GetItemContainerName(ContainerName, index);
            UsedCount = usedCount;
        }

        private const string FolderIdContainerCacheName = "FolderIdToMd5ContainerName";
        private string GetFolderIdContainerName(string parentFolderId)
        {
            return MD5Utility.ConvertToMd5(parentFolderId);
        }

        public void AddUsedCount(int blobSize)
        {
            UsedCount += BlobDataAccess.GetBlobCount(blobSize);
        }

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
}