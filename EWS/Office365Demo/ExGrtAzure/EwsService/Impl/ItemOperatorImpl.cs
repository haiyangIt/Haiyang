using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Exchange.WebServices.Data;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using EwsService.Common;
using EwsServiceInterface;
using EwsFrame;
using DataProtectInterface;
using System.Threading;
using LogInterface;

namespace EwsService.Impl
{
    public class ItemOperatorImpl : IItem
    {
        public ItemOperatorImpl(ExchangeService service, IDataAccess dataAccess)
        {
            CurrentExchangeService = service;
            _dataAccess = dataAccess;
        }

        private PropertyDefinition[] _itemProperties;
        private PropertyDefinition[] ItemProperties
        {
            get
            {

                if (_itemProperties == null)
                {
                    _itemProperties = new PropertyDefinition[]
                    {
                        ItemSchema.Subject,
                        ItemSchema.DateTimeCreated,
                        ItemSchema.ParentFolderId,
                        ItemSchema.ItemClass,
                        ItemSchema.Size
                    };
                }
                return _itemProperties;

            }
        }

        private PropertySet _itemPropertySet;
        private PropertySet ItemPropertySet
        {
            get
            {
                if (_itemPropertySet == null)
                {
                    _itemPropertySet = new PropertySet(ItemProperties);
                }
                return _itemPropertySet;
            }
        }


        private readonly IDataAccess _dataAccess;

        public ExchangeService CurrentExchangeService
        {
            get; set;
        }

        private void DoExportEmlItem(Item itemInEws, MemoryStream emlStream, EwsServiceArgument argument)
        {
            PropertySet props = new PropertySet(EmailMessageSchema.MimeContent);
            //itemInEws.Load(props);
            //This results in a GetItem call to EWS.
            AutoResetEvent ev = new AutoResetEvent(false);
            Exception ex = null;
            Timer t = null;
            try
            {
                bool hasLoad = false;
                t = new Timer((arg) =>
                {
                    try
                    {
                        if (hasLoad)
                        {
                            ex = new TimeoutException("Export eml message time out.");
                            ev.Set();
                        }
                        else
                        {
                            hasLoad = true;
                            itemInEws.Load(props);
                            ev.Set();
                        }
                    }
                    catch (Exception e)
                    {
                        ex = e;
                        LogFactory.LogInstance.WriteException(LogLevel.ERR, "Export eml failed", e, e.Message);
                        ev.Set();
                    }
                }, null, 0, ExportUploadHelper.TimeOut);
                while (!ev.WaitOne(1000))
                {

                }
            }
            finally
            {
                if (t != null)
                    t.Dispose();
                if (ev != null)
                    ev.Dispose();
                t = null;
                ev = null;
            }

            if (ex != null)
            {
                throw new ApplicationException("Export eml error", ex);
            }

            //var email = EmailMessage.Bind(CurrentExchangeService, itemInEws.Id, props);
            emlStream.Write(itemInEws.MimeContent.Content, 0, itemInEws.MimeContent.Content.Length);
            itemInEws.MimeContent.Content = null;
        }
        public void ExportEmlItem(Item itemInEws, MemoryStream emlStream, EwsServiceArgument argument)
        {
            int retryCount = 0;
            Exception lastException = null;
            while (retryCount < 3)
            {
                if (retryCount > 0)
                {
                    const int sleepCount = 10 * 1000;
                    LogFactory.LogInstance.WriteLog(LogLevel.WARN, "retry export eml", "after sleeping  {0} seconde ,will try the [{1}]th export.", sleepCount, retryCount);
                    Thread.Sleep(sleepCount);
                }
                try
                {
                    DoExportEmlItem(itemInEws, emlStream, argument);
                    break;
                }
                catch (Exception e)
                {
                    System.Diagnostics.Trace.TraceError(e.GetExceptionDetail());
                    LogFactory.LogInstance.WriteException(LogLevel.ERR, "Export eml error", e, e.Message);
                    lastException = e;
                    retryCount++;
                }
            }
            if (lastException != null)
                throw new ApplicationException("Export eml failure", lastException);
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
            ItemView oView = new ItemView(pageSize, offset, OffsetBasePoint.Beginning);
            oView.PropertySet = ItemPropertySet;
            while (moreItems)
            {
                oView.Offset = offset;
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
            var result = !dataAccess.IsItemContentExist(item.Id.UniqueId);
            return result;

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