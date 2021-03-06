﻿using EwsFrame;
using EwsFrame.Cache;
using SqlDbImpl.Model;
using SqlDbImpl.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace SqlDbImpl.Cache
{
    //[Obsolete("For blob size limitation.")]
    //public class FolderContainerMapCache : ICache
    //{
    //    public string MailboxAddress { get; set; }
    //    public FolderContainerMapCache(string mailboxAddress)
    //    {
    //        MailboxAddress = mailboxAddress;
    //    }
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

    //    private static string GetFolderContainerName(string currentMailbox)
    //    {
    //        string domain = ServiceContext.GetOrganizationPrefix(currentMailbox);
    //        return string.Format("{0}{2}{1}", domain, FolderContainerMappingContainerName, BlobDataAccess.DashChar);
    //    }
    //    private const string FolderContainerMappingContainerName = "foldercontainermapping";
    //    public void DeSerialize()
    //    {
    //        string folderContainerMappingContainerName = string.Empty;
    //        string blobName = string.Empty;
    //        GetContainerAndBlobName(MailboxAddress, out folderContainerMappingContainerName, out blobName);
    //        BlobDataAccess dataAccess = new BlobDataAccess(CatalogDataAccess.BlobClient);
            
    //        using (var memorySteam = new MemoryStream())
    //        {
    //            if (dataAccess.IsBlobExist(folderContainerMappingContainerName, blobName))
    //            {
    //                dataAccess.GetBlob(folderContainerMappingContainerName, blobName, memorySteam);
    //            }
                
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

    //    private void GetContainerAndBlobName(string mailboxAddress, out string containerName, out string blobName)
    //    {
    //        containerName = BlobDataAccess.ValidateContainerName(GetFolderContainerName(mailboxAddress));
    //        blobName = ItemLocationModel.GetFolderContainerMappingBlobName(mailboxAddress);
    //        blobName = BlobDataAccess.ValidateBlobName(blobName, false);
    //    }

    //    public void Serialize()
    //    {
    //        string folderContainerMappingContainerName = string.Empty;
    //        string blobName = string.Empty;
    //        GetContainerAndBlobName(MailboxAddress, out folderContainerMappingContainerName, out blobName);

    //        List<FolderContainerMapping> result = new List<FolderContainerMapping>(_dic.Count);
    //        foreach(var keyValue in _dic)
    //        {
    //            result.Add(keyValue.Value as FolderContainerMapping);
    //        }
    //        BlobDataAccess dataAccess = new BlobDataAccess(CatalogDataAccess.BlobClient);
    //        using (MemoryStream memoryStream = new MemoryStream())
    //        {
    //            FolderContainerMapping.SerializeList(result, memoryStream);
    //            memoryStream.Seek(0, SeekOrigin.Begin);
                
    //            dataAccess.SaveBlob(folderContainerMappingContainerName, blobName, memoryStream, false);
    //        }
    //    }

    //    private Dictionary<ICacheKey, object> _dic = new Dictionary<ICacheKey, object>();
    //}


}