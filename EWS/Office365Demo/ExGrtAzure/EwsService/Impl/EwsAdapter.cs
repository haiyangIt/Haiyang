using DataProtectInterface.Util;
using EwsDataInterface;
using EwsFrame;
using EwsFrame.Util;
using EwsService.Common;
using EwsServiceInterface;
using LogInterface;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EwsService.Impl
{
    public class EwsAdapter : IEwsAdapter
    {
        private EwsBaseOperator _ewsOperator;

        private ExchangeService CurrentExchangeService
        {
            get; set;
        }

        public string MailboxPrincipalAddress
        {
            get; private set;
        }

        public string ConnectMailbox(EwsServiceArgument argument, string connectMailAddress)
        {
            argument.SetConnectMailbox(connectMailAddress);
            MailboxPrincipalAddress = connectMailAddress;
            CurrentExchangeService = EwsProxyFactory.CreateExchangeService(argument, MailboxPrincipalAddress);
            CreatedFolders = new Dictionary<string, FolderId>();
            return CurrentExchangeService.Url.AbsoluteUri;
        }

        class ExchangeServiceObserver
        {
            public ExchangeService CurrentExchangeService { get; set; }
            public EwsServiceArgument Argument { get; set; }
            public string Mailbox { get; set; }


        }

        private PropertyDefinition[] _folderProperties;
        private PropertyDefinition[] FolderProperties
        {
            get
            {

                if (_folderProperties == null)
                {
                    _folderProperties = new PropertyDefinition[]
                    {
                        FolderSchema.DisplayName,
                        FolderSchema.ParentFolderId,
                        FolderSchema.ChildFolderCount,
                        FolderSchema.FolderClass,
                        FolderSchema.TotalCount
                    };
                }
                return _folderProperties;

            }
        }

        private PropertySet _folderPropertySet;
        private PropertySet FolderPropertySet
        {
            get
            {
                if (_folderPropertySet == null)
                {
                    _folderPropertySet = new PropertySet(FolderProperties);
                }
                return _folderPropertySet;
            }
        }
        

        public IFolderData GetRootFolder()
        {
            var folder = _ewsOperator.FolderBind(WellKnownFolderName.MsgFolderRoot, FolderPropertySet);
            return DataConvert.Convert(folder, MailboxPrincipalAddress);
        }


        public bool IsFolderNeedGenerateCatalog(IFolderData folder)
        {
            return FolderClassUtil.IsFolderValid(folder.FolderType);
        }

        private FolderId CreateChildFolder(IFolderDataBase folderData, string parentFolderId)
        {
            Folder folder = new Folder(CurrentExchangeService);
            folder.DisplayName = folderData.DisplayName;
            folder.FolderClass = folderData.FolderType;
            _ewsOperator.FolderSave(folder, parentFolderId);
            return FindFolder(folderData, parentFolderId);
        }

        private FolderId FindFolder(IFolderDataBase folderData, string parentFolderId, int findCount = 0)
        {
            FolderView view = new FolderView(1);
            view.PropertySet = new PropertySet(BasePropertySet.IdOnly);
            view.Traversal = FolderTraversal.Shallow;
            SearchFilter filter = new SearchFilter.IsEqualTo(FolderSchema.DisplayName, folderData.DisplayName);
            FindFoldersResults results = _ewsOperator.FindFolders(parentFolderId, filter, view);

            if (results.TotalCount > 1)
            {
                throw new InvalidOperationException("Find more than 1 folder.");
            }
            else if (results.TotalCount == 0)
            {
                if (findCount > 3)
                {
                    return null;
                }
                Thread.Sleep(500);
                return FindFolder(folderData, parentFolderId, ++findCount);
            }
            else
            {
                foreach (var result in results)
                {
                    return result.Id;
                }
                throw new InvalidOperationException("Thread sleep time is too short.");
            }
        }

        public void DeleteFolder(string findFolderId, DeleteMode deleteMode)
        {
            Folder folder = _ewsOperator. FolderBind(findFolderId);
            _ewsOperator.FolderDelete(folder, deleteMode);
        }

        public List<IFolderData> GetChildFolder(string parentFolderId)
        {
            const int pageSize = 100;
            int offset = 0;
            bool moreItems = true;
            List<IFolderData> result = new List<IFolderData>();


            var parentFolder = new FolderId(parentFolderId);
            //var parentFolderObj = Folder.Bind(CurrentExchangeService, parentFolder);
            // return GetChildFolder(parentFolderObj);
            while (moreItems)
            {
                FolderView oView = new FolderView(pageSize, offset, OffsetBasePoint.Beginning);
                oView.PropertySet = FolderPropertySet;
                FindFoldersResults findResult = _ewsOperator.FindFolders(parentFolder, oView);

                foreach(var folder in findResult.Folders)
                {
                    result.Add(DataConvert.Convert(folder, MailboxPrincipalAddress));
                }

                if (!findResult.MoreAvailable)
                    moreItems = false;

                if (moreItems)
                    offset += pageSize;
            }


            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folderPaths"></param>
        /// <returns>folderId</returns>
        public string CreateFoldersIfNotExist(List<IFolderDataBase> folderPaths)
        {
            FolderId parentFolderId = GetRootFolder().Id;
            StringBuilder sb = new StringBuilder();
            foreach (var folder in folderPaths)
            {
                parentFolderId = FindAndCreateFolder(folder, parentFolderId, sb);
            }
            return parentFolderId.UniqueId;
        }

        private FolderId FindAndCreateFolder(IFolderDataBase folder, FolderId parentFolderId, StringBuilder keyBuilder)
        {
            FolderId folderId = null;
            keyBuilder.Append(folder.DisplayName).Append("\\");
            var key = keyBuilder.ToString(); //string.Format("{0}-{1}", folder.DisplayName, parentFolderId.UniqueId);
            if (CreatedFolders.TryGetValue(key, out folderId))
                return folderId;

            folderId = FindFolder(folder, parentFolderId.UniqueId, 3);
            if (folderId == null)
                folderId = CreateChildFolder(folder, parentFolderId.UniqueId);
            CreatedFolders.Add(key, folderId);
            return folderId;
        }
        
        private Dictionary<string, FolderId> CreatedFolders;

        private Folder _contactFolder;
        private Folder _appointmentFolder;
        private Folder _taskFolder;
        private object _lockObj = new object();

        public string GetContactFolderId()
        {
            if(_contactFolder == null)
            {
                using (_lockObj.LockWhile(() => {
                    if(_contactFolder == null)
                    {
                        _contactFolder = _ewsOperator.FolderBind(WellKnownFolderName.Contacts);
                    }
                })) { }
            }
            return _contactFolder.Id.UniqueId;
        }

        public string GetAppointmentFolderId()
        {
            if (_appointmentFolder == null)
            {
                using (_lockObj.LockWhile(() => {
                    if (_appointmentFolder == null)
                    {
                        _appointmentFolder = _ewsOperator.FolderBind(WellKnownFolderName.Calendar);
                    }
                }))
                { }
            }
            return _appointmentFolder.Id.UniqueId;
        }

        public string GetTaskFolderId()
        {
            if (_taskFolder == null)
            {
                using (_lockObj.LockWhile(() => {
                    if (_taskFolder == null)
                    {
                        _taskFolder = _ewsOperator.FolderBind( WellKnownFolderName.Tasks);
                    }
                }))
                { }
            }
            return _taskFolder.Id.UniqueId;
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

        public IDataConvert DataConvert
        {
            get; set;
        }
        
        public void ExportEmlItem(IItemData item, MemoryStream emlStream, EwsServiceArgument argument)
        {
            var itemInEws = item.Data as Item;

            PropertySet props = new PropertySet(EmailMessageSchema.MimeContent);
            _ewsOperator.Load(itemInEws, props);
            //var email = EmailMessage.Bind(CurrentExchangeService, itemInEws.Id, props);
            emlStream.Write(itemInEws.MimeContent.Content, 0, itemInEws.MimeContent.Content.Length);
            itemInEws.MimeContent.Content = null;
        }

        public void ExportItem(IItemData item, Stream stream, EwsServiceArgument argument)
        {
            ExportUploadHelper.ExportItemPost(Enum.GetName(typeof(ExchangeVersion), CurrentExchangeService.RequestedServerVersion), item.Id, stream, argument);
        }

        public List<IItemData> GetFolderItems(IFolderData folder)
        {
            const int pageSize = 100;
            int offset = 0;
            bool moreItems = true;
            List<IItemData> result = new List<IItemData>(folder.ChildItemCountInEx);
            ItemView oView = new ItemView(pageSize, offset, OffsetBasePoint.Beginning);
            oView.PropertySet = ItemPropertySet;
            var folderId = new FolderId(folder.FolderId);
            while (moreItems)
            {
                oView.Offset = offset;
                FindItemsResults<Item> findResult = _ewsOperator.FindItems(folderId, oView);

                foreach(var item in findResult.Items)
                {
                    result.Add(DataConvert.Convert(item));
                }

                if (!findResult.MoreAvailable)
                    moreItems = false;

                if (moreItems)
                    offset += pageSize;
            }
            return result;
        }

        public void ImportItem(string parentFolderId, Stream stream, EwsServiceArgument argument)
        {
            ExportUploadHelper.UploadItemPost(Enum.GetName(typeof(ExchangeVersion),
                CurrentExchangeService.RequestedServerVersion),
                parentFolderId,
                CreateActionType.CreateNew,
                string.Empty,
                stream,
                argument);
        }

        public void ImportItem(string parentFolderId, byte[] itemData, EwsServiceArgument argument)
        {
            ExportUploadHelper.UploadItemPost(Enum.GetName(typeof(ExchangeVersion), CurrentExchangeService.RequestedServerVersion), parentFolderId, CreateActionType.CreateNew, string.Empty, itemData, argument);
        }

        public bool IsItemNew(IItemData item, DateTime lastTime, DateTime thisTime)
        {
            return true;
            //var dataAccess = (ICatalogDataAccess)_dataAccess;
            //var result = !dataAccess.IsItemContentExist(item.Id.UniqueId);
            //return result;

            //return (item.DateTimeCreated > lastTime && item.DateTimeCreated <= thisTime) || (item.LastModifiedTime > lastTime && item.LastModifiedTime <= thisTime);
        }
    }

    public class EwsOperationBase
    {

    }
}
