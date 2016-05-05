using EwsFrame;
using EwsFrame.Cache;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using TableBlobImpl.Access;
using TableBlobImpl.Access.Blob;
using TableBlobImpl.Storage.Table.Model;

namespace TableBlobImpl.Cache
{
    //public class FolderContainerMapCache : ICache
    //{
    //    public FolderContainerMapCache(string mailboxAddress) { }

    //    public string MailboxAddress { get; set; }

    //    public static string CacheName = "FolderContainer";

    //    public void AddKeyValue(ICacheKey cacheKey, object cacheValue)
    //    {
    //        _dic.Add(cacheKey, cacheValue);
    //    }

    //    public void SetKeyValue(ICacheKey cacheKey, object cacheValue)
    //    {
    //        _dic[cacheKey] = cacheValue;
    //    }

    //    public bool TryGetValue(ICacheKey cacheKey,out object value)
    //    {
    //        return _dic.TryGetValue(cacheKey, out value);
    //    }

    //    private const string FolderContainerMappingContainerName = "foldercontainermapping";
    //    public void DeSerialize()
    //    {
    //        string folderContainerMappingContainerName = BlobDataAccess.ValidateContainerName(FolderContainerMappingContainerName);
    //        string blobName = ItemLocationEntity.GetFolderContainerMappingBlobName(MailboxAddress);
    //        blobName = BlobDataAccess.ValidateBlobName(blobName, false);

    //        BlobDataAccess dataAccess = new BlobDataAccess(TableBlobDataAccess.BlobClient);
    //        using (var memorySteam = new MemoryStream())
    //        {
    //            dataAccess.GetBlob(folderContainerMappingContainerName, blobName, memorySteam);

    //            memorySteam.Seek(0, SeekOrigin.Begin);
    //            if (memorySteam.Length != 0)
    //            {
    //                var allFolderContainerMapping = FolderContainerMapping.DeSerializeList(memorySteam);
    //                foreach (var eachObj in allFolderContainerMapping)
    //                {
    //                    StringCacheKey key = new StringCacheKey(eachObj.FolderId);
    //                    AddKeyValue(key, eachObj);
    //                }
    //            }
    //        }
    //    }

    //    public void Serialize()
    //    {
    //        string folderContainerMappingContainerName = BlobDataAccess.ValidateContainerName(FolderContainerMappingContainerName);
    //        string blobName = ItemLocationEntity.GetFolderContainerMappingBlobName(MailboxAddress);
    //        blobName = BlobDataAccess.ValidateBlobName(blobName, false);

    //        List<FolderContainerMapping> result = new List<FolderContainerMapping>(_dic.Count);
    //        foreach(var keyValue in _dic)
    //        {
    //            result.Add(keyValue.Value as FolderContainerMapping);
    //        }
    //        BlobDataAccess dataAccess = new BlobDataAccess(TableBlobDataAccess.BlobClient);
    //        using (MemoryStream memoryStream = new MemoryStream())
    //        {
    //            FolderContainerMapping.SerializeList(result, memoryStream);
    //            memoryStream.Seek(0, SeekOrigin.Begin);
                
    //            dataAccess.SaveBlob(folderContainerMappingContainerName, blobName, memoryStream);
    //        }
    //    }

    //    private Dictionary<ICacheKey, object> _dic = new Dictionary<ICacheKey, object>();
    //}


}