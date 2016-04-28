using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Exchange.WebServices.Data;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using Arcserve.Office365.Exchange.StorageAccess.Interface;
using Arcserve.Office365.Exchange.EwsApi.Impl.Common;

namespace Arcserve.Office365.Exchange.EwsApi.Impl.Impl
{
    public class ItemOperatorImpl : IItem
    {
        public ItemOperatorImpl(ExchangeService service, IDataAccess dataAccess)
        {
            CurrentExchangeService = service;
            _dataAccess = dataAccess;
        }

        private readonly IDataAccess _dataAccess;

        public ExchangeService CurrentExchangeService
        {
            get; set;
        }

        public void ExportEmlItem(Item itemInEws, MemoryStream emlStream, EwsServiceArgument argument)
        {
            PropertySet props = new PropertySet(EmailMessageSchema.MimeContent);

            // This results in a GetItem call to EWS.
            itemInEws.Load(props);
            //var email = EmailMessage.Bind(CurrentExchangeService, itemInEws.Id, props);
            emlStream.Write(itemInEws.MimeContent.Content, 0, itemInEws.MimeContent.Content.Length);
        }

        public void ExportItem(Item item, Stream stream, EwsServiceArgument argument)
        {
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

        public void ImportItem(FolderId parentFolderId, Stream stream, EwsServiceArgument argument)
        {
            ExportUploadHelper.UploadItemPost(Enum.GetName(typeof(ExchangeVersion),
                CurrentExchangeService.RequestedServerVersion), 
                parentFolderId, 
                CreateActionType.CreateNew, 
                string.Empty, 
                stream, 
                argument);
        }

        public void ImportItem(FolderId parentFolderId, byte[] itemData, EwsServiceArgument argument)
        {
            ExportUploadHelper.UploadItemPost(Enum.GetName(typeof(ExchangeVersion), CurrentExchangeService.RequestedServerVersion), parentFolderId, CreateActionType.CreateNew, string.Empty, itemData, argument);
        }

        public bool IsItemNew(Item item, DateTime lastTime, DateTime thisTime)
        {
            var dataAccess = (ICatalogDataAccess)_dataAccess;
            return !dataAccess.IsItemContentExist(item.Id.UniqueId);

            //return (item.DateTimeCreated > lastTime && item.DateTimeCreated <= thisTime) || (item.LastModifiedTime > lastTime && item.LastModifiedTime <= thisTime);
        }
    }

    //public class ContainerCount
    //{
    //    public string ContainerName { get; set; }
    //    public int UsedCount { get; set; }

    //    public static void WriteToStream(ContainerCount obj, Stream streamWriter)
    //    {
    //        BinaryWriter writer = new BinaryWriter(streamWriter);

    //        writer.Write(obj.ContainerName);
    //        writer.Write(obj.UsedCount);


    //    }

    //    public static ContainerCount ReadFromStream(Stream streamReader)
    //    {
    //        BinaryReader reader = new BinaryReader(streamReader);

    //        ContainerCount result = new ContainerCount();
    //        result.ContainerName = reader.ReadString();
    //        result.UsedCount = reader.ReadInt32();
    //        return result;

    //    }
    //}

    //[Serializable]
    //public class FolderContainerMapping
    //{
    //    public string FolderId { get; set; }
    //    public ContainerCount ContainerInfo { get; set; }

    //    public static FolderContainerMapping NewInstance(string folderId)
    //    {
    //        var result = new FolderContainerMapping();
    //        result.FolderId = folderId;
    //        result.ContainerInfo = new ContainerCount();
    //        result.ContainerInfo.ContainerName = NameHelper.GetItemContainerName(folderId, 0);
    //        result.ContainerInfo.UsedCount = 0;
    //        return result;
    //    }

    //    public static FolderContainerMapping NewNextContainer(FolderContainerMapping currentMapping)
    //    {
    //        currentMapping.ContainerInfo.ContainerName = NameHelper.GetItemNextContainerName(currentMapping.ContainerInfo.ContainerName);
    //        currentMapping.ContainerInfo.UsedCount = 0;
    //        return currentMapping;
    //    }

    //    public static void WriteToStream(FolderContainerMapping obj, Stream streamWriter)
    //    {
    //        BinaryWriter writer = new BinaryWriter(streamWriter);

    //        writer.Write(obj.FolderId);
    //        ContainerCount.WriteToStream(obj.ContainerInfo, streamWriter);
    //    }

    //    public static FolderContainerMapping ReadFromStream(Stream streamReader)
    //    {
    //        BinaryReader reader = new BinaryReader(streamReader);

    //        FolderContainerMapping result = new FolderContainerMapping();
    //        result.FolderId = reader.ReadString();
    //        result.ContainerInfo = ContainerCount.ReadFromStream(streamReader);
    //        return result;

    //    }

    //    public static void SerializeList(List<FolderContainerMapping> lists, Stream data)
    //    {
    //        foreach (var obj in lists)
    //        {
    //            FolderContainerMapping.WriteToStream(obj, data);
    //        }
    //    }

    //    public static List<FolderContainerMapping> DeSerializeList(Stream data)
    //    {
    //        List<FolderContainerMapping> result = new List<FolderContainerMapping>();
    //        while (data.Position < data.Length)
    //        {
    //            FolderContainerMapping obj = FolderContainerMapping.ReadFromStream(data);
    //            result.Add(obj);
    //        }
    //        return result;
    //    }
    //}
}