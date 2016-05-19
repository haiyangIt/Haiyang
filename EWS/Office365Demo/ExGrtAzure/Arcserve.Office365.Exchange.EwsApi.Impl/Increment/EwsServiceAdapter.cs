using Arcserve.Office365.Exchange.EwsApi.Increment;
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

namespace Arcserve.Office365.Exchange.EwsApi.Impl.Increment
{
    public class EwsServiceAdapter : IEwsServiceAdapter<IJobProgress>
    {
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

        private EwsBaseOperator _ewsOperator = new EwsLimitOperator();

        public ICollection<IMailboxDataSync> GetAllMailboxes(string adminUserName, string adminPassword)
        {
            var result = _ewsOperator.GetAllMailbox(adminUserName, adminPassword);
            Progress.Report("Getting all mailboxes in exchange completed, total {0} mailboxes.", result.Count);
            return result;
        }

        public async Task<ICollection<IMailboxDataSync>> GetAllMailboxesAsync(string adminUserName, string adminPassword)
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

        public async Task<ChangeCollection<FolderChange>> SyncFoldersAsync(string lastSyncStatus)
        {
            throw new NotImplementedException();
        }

        private static object _lockObj = new object();
        private static PropertySet _folderPropertySet = null;
        private static PropertySet FolderPropertySet
        {
            get
            {
                if (_folderPropertySet == null)
                {
                    using (_lockObj.LockWhile(() =>
                    {
                        if (_folderPropertySet == null)
                        {
                            _folderPropertySet = new PropertySet(FolderSchema.DisplayName,
                        FolderSchema.ParentFolderId,
                        FolderSchema.ChildFolderCount,
                        FolderSchema.FolderClass,
                        FolderSchema.TotalCount);
                        }
                    }))
                    { }

                }
                return _folderPropertySet;
            }
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

        public void ExportItems(IEnumerable<Item> items, Action<IEnumerable<ItemDatas>> writeItemsToStorage)
        {
            IEnumerable<ItemDatas> itemDatas = _ewsOperator.ExportItems(items);
            writeItemsToStorage(itemDatas);
        }

        public System.Threading.Tasks.Task ExportItemsAsync(IEnumerable<Item> items, Action<IEnumerable<Item>> writeItemsToStorage)
        {
            throw new NotImplementedException();
        }

        private static PropertySet _itemPropertySet = null;
        private static PropertySet ItemPropertySet
        {
            get
            {
                if (_itemPropertySet == null)
                {
                    using (_lockObj.LockWhile(() =>
                    {
                        if (_itemPropertySet == null)
                        {
                            _itemPropertySet = new PropertySet(ItemSchema.Subject,
                        ItemSchema.DateTimeCreated,
                        ItemSchema.ParentFolderId,
                        ItemSchema.ItemClass,
                        ItemSchema.Size);
                        }
                    }))
                    { }

                }
                return _itemPropertySet;
            }
        }
        public void LoadItemsProperties(IEnumerable<Item> items)
        {
            _ewsOperator.LoadPropertiesForItems(items, ItemPropertySet);
        }

        public System.Threading.Tasks.Task LoadItemsPropertiesAsync(IEnumerable<Item> items)
        {
            throw new NotImplementedException();
        }
    }
}
