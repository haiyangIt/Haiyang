using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arcserve.Office365.Exchange.Data.Account;
using Arcserve.Office365.Exchange.Data.Increment;
using Arcserve.Office365.Exchange.Thread;
using Microsoft.Exchange.WebServices.Data;
using System.Threading;
using Arcserve.Office365.Exchange.Util;
using Arcserve.Office365.Exchange.Data.Mail;
using Arcserve.Office365.Exchange.EwsApi.Interface;
using System.IO;

namespace Arcserve.Office365.Exchange.EwsApi.Impl.Increment
{
    internal class EwsServiceAdapter : IEwsServiceAdapter<IJobProgress>
    {
        static EwsServiceAdapter()
        {
            FolderPropertySet = new PropertySet(FolderSchema.DisplayName,
                        FolderSchema.ParentFolderId,
                        FolderSchema.ChildFolderCount,
                        FolderSchema.FolderClass,
                        FolderSchema.TotalCount,
                        FolderSchema.WellKnownFolderName);

            ItemPropertySet = new PropertySet(ItemSchema.Subject,
                        ItemSchema.DateTimeCreated,
                        ItemSchema.ParentFolderId,
                        ItemSchema.ItemClass,
                        ItemSchema.Size,
                        ItemSchema.DateTimeSent,
                        ItemSchema.DateTimeReceived,
                        ItemSchema.HasAttachments);

            EmailPropertySet = new PropertySet(ItemSchema.Subject,
                        ItemSchema.DateTimeCreated,
                        ItemSchema.ParentFolderId,
                        ItemSchema.ItemClass,
                        ItemSchema.Size,
                        ItemSchema.DateTimeSent,
                        ItemSchema.DateTimeReceived,
                        ItemSchema.HasAttachments,
                        EmailMessageSchema.Sender,
                        EmailMessageSchema.ReceivedRepresenting,
                        EmailMessageSchema.IsRead);

            CalendarPropertySet = new PropertySet(
                        ItemSchema.Subject,
                        ItemSchema.DateTimeCreated,
                        ItemSchema.ParentFolderId,
                        ItemSchema.ItemClass,
                        ItemSchema.Size,
                        ItemSchema.DateTimeSent,
                        ItemSchema.DateTimeReceived,
                        ItemSchema.HasAttachments,
                        AppointmentSchema.Organizer,
                        AppointmentSchema.RequiredAttendees,
                        AppointmentSchema.OptionalAttendees
                );
        }

        private static readonly PropertySet FolderPropertySet;

        private static readonly PropertySet ItemPropertySet;

        private static readonly PropertySet EmailPropertySet;

        private static readonly PropertySet CalendarPropertySet;


        public CancellationToken CancelToken
        {
            get; set;
        }

        public IJobProgress Progress
        {
            get; set;
        }

        public TaskScheduler Scheduler
        {
            get; set;
        }

        /// <summary>
        /// todo need to hide private;
        /// </summary>
        protected EwsBaseOperator _ewsOperator = new EwsLimitOperator();

        public ICollection<IMailboxDataSync> GetAllMailboxes(string adminUserName, string adminPassword, IEnumerable<string> mailboxes)
        {
            var result = _ewsOperator.GetAllMailbox(adminUserName, adminPassword, mailboxes);
            Progress.Report("Getting all mailboxes in exchange completed, total {0} mailboxes.", result.Count);
            return result;
        }

        public async Task<ICollection<IMailboxDataSync>> GetAllMailboxesAsync(string adminUserName, string adminPassword, IEnumerable<string> mailboxes)
        {
            throw new NotImplementedException();
        }


        public string GetExchangeService(string mailbox, OrganizationAdminInfo adminInfo)
        {
            var result = _ewsOperator.NewExchangeService(mailbox, new EwsServiceArgument(mailbox, adminInfo.UserName, adminInfo.UserPassword));
            return result;
        }

        public async Task<string> GetExchangeServiceAsync(string mailbox, OrganizationAdminInfo adminInfo)
        {
            throw new NotImplementedException();
        }

        public void InitTaskSyncContext(ITaskSyncContext<IJobProgress> mainContext)
        {
            this.CloneSyncContext(mainContext);
        }

        public ChangeCollection<FolderChange> SyncFolderHierarchy(string lastSyncStatus)
        {
            var result = _ewsOperator.SyncFolderHierarchy(lastSyncStatus);
            Progress.Report("Get folder hierarchy changes completed, this time contains {0} changes.", result.Count);
            return result;
        }

        public async Task<ChangeCollection<FolderChange>> SyncFolderHierarchyAsync(string lastSyncStatus)
        {
            throw new NotImplementedException();
        }


        public void LoadFolderProperties(Folder folder)
        {
            _ewsOperator.LoadFolderProperties(folder, FolderPropertySet);
        }

        public System.Threading.Tasks.Task LoadFolderPropertiesAsync(Folder folder)
        {
            throw new NotImplementedException();
        }

        public ChangeCollection<ItemChange> SyncItems(FolderId folderId, string lastSyncStatus)
        {
            return _ewsOperator.SyncFolderItems(folderId, lastSyncStatus);
        }

        public Task<ChangeCollection<ItemChange>> SyncItemsAsync(FolderId folderId, string lastSyncStatus)
        {
            throw new NotImplementedException();
        }

        //public void ExportItems(IEnumerable<Item> items, Action<IEnumerable<ItemDatas>> writeItemsToStorage)
        //{
        //    IEnumerable<ItemDatas> itemDatas = _ewsOperator.ExportItems(items);
        //    writeItemsToStorage(itemDatas);
        //}

        //public System.Threading.Tasks.Task ExportItemsAsync(IEnumerable<Item> items, Action<IEnumerable<Item>> writeItemsToStorage)
        //{
        //    throw new NotImplementedException();
        //}

        public void LoadItemsProperties(IEnumerable<Item> items, ItemClass itemClass)
        {
            switch (itemClass)
            {
                case ItemClass.Message:
                    _ewsOperator.LoadPropertiesForItems(items, EmailPropertySet);
                    break;
                default:
                    _ewsOperator.LoadPropertiesForItems(items, ItemPropertySet);
                    break;
            }
        }

        public System.Threading.Tasks.Task LoadItemsPropertiesAsync(IEnumerable<Item> items, ItemClass itemClass)
        {
            throw new NotImplementedException();
        }

        public int ExportItems(IEnumerable<IItemDataSync> items, IExportItemsOper exportItemOper)
        {
            return _ewsOperator.ExportItems(items, exportItemOper);
        }

        public System.Threading.Tasks.Task<int> ExportItemsAsync(IEnumerable<IItemDataSync> items, IExportItemsOper exportItemOper)
        {
            throw new NotImplementedException();
        }

        public void ImportItem(Stream dataStream, FolderId folder)
        {
            _ewsOperator.ImportItem(folder.UniqueId, dataStream);
        }

        public Folder GetAndCreateIfFolderNotExist(IFolderDataSync folder, Folder parentFolder)
        {
            throw new NotImplementedException();
        }

        public FindFoldersResults FindFolders(FolderId parentFolderId, FolderView view)
        {
            return _ewsOperator.FindFolders(parentFolderId, view);
        }

        public FindFoldersResults FindFolders(WellKnownFolderName parentFolderName, FolderView view)
        {
            return _ewsOperator.FindFolders(parentFolderName, view);
        }

        public FindFoldersResults FindFolders(FolderId parentFolderId, SearchFilter searchFilter, FolderView view)
        {
            return _ewsOperator.FindFolders(parentFolderId, searchFilter, view);
        }

        public FindFoldersResults FindFolders(WellKnownFolderName parentFolderName, SearchFilter searchFilter, FolderView view)
        {
            return _ewsOperator.FindFolders(parentFolderName, searchFilter, view);
        }

        public Task<FindFoldersResults> FindFoldersAsync(FolderId parentFolderId, FolderView view)
        {
            throw new NotImplementedException();
        }

        public Task<FindFoldersResults> FindFoldersAsync(WellKnownFolderName parentFolderName, FolderView view)
        {
            throw new NotImplementedException();
        }

        public Task<FindFoldersResults> FindFoldersAsync(FolderId parentFolderId, SearchFilter searchFilter, FolderView view)
        {
            throw new NotImplementedException();
        }

        public Task<FindFoldersResults> FindFoldersAsync(WellKnownFolderName parentFolderName, SearchFilter searchFilter, FolderView view)
        {
            throw new NotImplementedException();
        }

        public Folder FolderBind(WellKnownFolderName wellKnowFolderName)
        {
            return _ewsOperator.FolderBind(wellKnowFolderName);
        }

        public Folder FolderBind(WellKnownFolderName wellKnowFolderName, PropertySet propertySet)
        {
            return _ewsOperator.FolderBind(wellKnowFolderName, propertySet);
        }

        public Task<Folder> FolderBindAsync(WellKnownFolderName wellKnowFolderName)
        {
            throw new NotImplementedException();
        }

        public Task<Folder> FolderBindAsync(WellKnownFolderName wellKnowFolderName, PropertySet propertySet)
        {
            throw new NotImplementedException();
        }
        public void FolderCreate(string folderName, string folderType, Folder parentFolder)
        {
            _ewsOperator.FolderCreate(folderName, folderType, parentFolder);
        }

        public Folder FindFolderInRoot(string folderDisplayName)
        {
            throw new NotImplementedException();
        }

        public Folder FindFolder(string folderDisplayName, FolderId parentFolderId)
        {
            throw new NotImplementedException();
        }

        public Folder CreateFolder(string folderName, string folderType, FolderId parentFolderId)
        {
            throw new NotImplementedException();
        }

        public Folder CreateFolder(string folderName, string folderType)
        {
            throw new NotImplementedException();
        }

        public void ImportItems(IEnumerable<ImportItemStatus> partition, Folder folder)
        {
            throw new NotImplementedException();
        }

        public int GetAllItemsCount()
        {
            var rootFolder = _ewsOperator.FolderBind(WellKnownFolderName.Root, BasePropertySet.IdOnly);
            var folderView = new FolderView(1);
            folderView.PropertySet = new PropertySet(FolderSchema.TotalCount);
            folderView.Traversal = FolderTraversal.Shallow;
            var searchFilter = new SearchFilter.IsEqualTo(FolderSchema.DisplayName, "AllItems");
            var result = _ewsOperator.FindFolders(rootFolder.Id, searchFilter, folderView);
            if (result.TotalCount != 1)
                return -1;
            return result.Folders[0].TotalCount;
        }
    }
}
