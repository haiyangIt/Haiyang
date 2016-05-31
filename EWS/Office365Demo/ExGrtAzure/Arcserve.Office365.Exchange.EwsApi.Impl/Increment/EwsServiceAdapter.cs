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
using Arcserve.Office365.Exchange.Data.Mail;

namespace Arcserve.Office365.Exchange.EwsApi.Impl.Increment
{
    public class EwsServiceAdapter : IEwsServiceAdapter<IJobProgress>
    {
        static EwsServiceAdapter()
        {
            FolderPropertySet = new PropertySet(FolderSchema.DisplayName,
                        FolderSchema.ParentFolderId,
                        FolderSchema.ChildFolderCount,
                        FolderSchema.FolderClass,
                        FolderSchema.TotalCount);

            ItemPropertySet = new PropertySet(ItemSchema.Subject,
                        ItemSchema.DateTimeCreated,
                        ItemSchema.ParentFolderId,
                        ItemSchema.ItemClass,
                        ItemSchema.Size,
                        ItemSchema.HasAttachments);
            EmailPropertySet = new PropertySet(ItemSchema.Subject,
                        ItemSchema.DateTimeCreated,
                        ItemSchema.ParentFolderId,
                        ItemSchema.ItemClass,
                        ItemSchema.Size,
                        ItemSchema.HasAttachments,
                        EmailMessageSchema.IsRead);

        }

        private static readonly PropertySet FolderPropertySet;

        private static readonly PropertySet ItemPropertySet;

        private static readonly PropertySet EmailPropertySet;


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
        private EwsBaseOperator _ewsOperator = new EwsLimitOperator();

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

        public async Task<ChangeCollection<FolderChange>> SyncFoldersAsync(string lastSyncStatus)
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
    }
}
