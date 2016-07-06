using Arcserve.Office365.Exchange.Data.Increment;
using Arcserve.Office365.Exchange.Thread;
using Arcserve.Office365.Exchange.Util.Setting;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.DataProtect.Interface.Restore
{
    public abstract class RestoreFlowTemplate : ITaskSyncContext<IJobProgress>, IDisposable
    {
        public IJobProgress Progress
        {
            get; set;
        }

        public CancellationToken CancelToken
        {
            get; set;
        }

        public TaskScheduler Scheduler
        {
            get; set;
        }
        public IRestoreToPosition<IJobProgress> RestoreToPosition { get; set; }

        public void InitTaskSyncContext(ITaskSyncContext<IJobProgress> mainContext)
        {
            this.CloneSyncContext(mainContext);
        }

        public abstract void Dispose();

        public void RestoreSync()
        {
            try
            {
                var mailboxesInPlan = GetAllMailboxFromPlan(GetAllMailboxFromCatalog);
                ForEachMailbox(mailboxesInPlan, RestoreMailbox);
            }
            catch (Exception e)
            {

            }
        }

        protected abstract IEnumerable<IMailboxDataSync> GetAllMailboxFromCatalog();
        protected abstract IEnumerable<IMailboxDataSync> GetAllMailboxFromPlan(Func<IEnumerable<IMailboxDataSync>> funcGetAllMailboxFromCatalog);
        protected abstract void ForEachMailbox(IEnumerable<IMailboxDataSync> mailboxes, Action<IMailboxDataSync> ActionDealMailbox);
        protected virtual void RestoreMailbox(IMailboxDataSync mailbox)
        {
            var restoreMailbox = NewRestoreMailboxFlowTemplate();
            restoreMailbox.MailboxInfo = mailbox;
            restoreMailbox.RestoreMailbox();
        }

        protected abstract RestoreMailboxFlowTemplate NewRestoreMailboxFlowTemplate();
    }

    public abstract class RestoreMailboxFlowTemplate : ITaskSyncContext<IJobProgress>
    {
        public IJobProgress Progress
        {
            get; set;
        }

        public CancellationToken CancelToken
        {
            get; set;
        }

        public TaskScheduler Scheduler
        {
            get; set;
        }
        public IRestoreToPosition<IJobProgress> RestoreToPosition { get; set; }

        public void InitTaskSyncContext(ITaskSyncContext<IJobProgress> mainContext)
        {
            this.CloneSyncContext(mainContext);
        }

        internal protected IMailboxDataSync MailboxInfo { get; set; }
        public void RestoreMailbox()
        {
            RestoreToPosition = RestoreToPosition.NewRestoreToPosition(MailboxInfo);
            ConnectExchangeService();
            var folderTree = new FolderTree();
            folderTree.Deserialize(MailboxInfo.FolderTree);
            var folders = GetFoldersFromPlan(MailboxInfo, GetFoldersFromCatalog);
            ForEachFolder(folders, folderTree, DealFolder);
        }

        protected abstract void ConnectExchangeService();
        protected abstract IEnumerable<IFolderDataSync> GetFoldersFromPlan(IMailboxDataSync mailboxInfo, Func<IMailboxDataSync, IEnumerable<IFolderDataSync>> funcGetFolderFromCatalog);
        protected abstract IEnumerable<IFolderDataSync> GetFoldersFromCatalog(IMailboxDataSync mailboxInfo);
        protected abstract void ForEachFolder(IEnumerable<IFolderDataSync> folders, FolderTree folderTree, Action<IFolderDataSync, FolderTree> actionDoFolder);
        protected abstract RestoreFolderFlowTemplate NewRestoreFolderTemplate();
        protected virtual void DealFolder(IFolderDataSync folder, FolderTree folderTree)
        {
            var folderFlow = NewRestoreFolderTemplate();
            folderFlow.RestoreToPosition = RestoreToPosition;
            folderFlow.Folder = folder;
            folderFlow.FolderTree = folderTree;
            folderFlow.RestoreFolder();
        }
    }

    public abstract class RestoreFolderFlowTemplate : ITaskSyncContext<IJobProgress>
    {
        public IJobProgress Progress
        {
            get; set;
        }

        public CancellationToken CancelToken
        {
            get; set;
        }

        public TaskScheduler Scheduler
        {
            get; set;
        }
        internal protected IRestoreToPosition<IJobProgress> RestoreToPosition { get; set; }

        public void InitTaskSyncContext(ITaskSyncContext<IJobProgress> mainContext)
        {
            this.CloneSyncContext(mainContext);
        }

        internal IFolderDataSync Folder { get; set; }
        internal FolderTree FolderTree { get; set; }

        public void RestoreFolder()
        {
            var folderHierarchy = FolderTree.GetParentsData(Folder);
            var folder = GetAndCreateIfFolderNotExistToExchange(folderHierarchy);
            ItemList itemList = null;
            int offset = 0;
            int pageCount = CloudConfig.Instance.BatchLoadItemsCountForRestore;
            var restoreItem = NewRestoreItemFlow();
            restoreItem.RestoreToPosition = RestoreToPosition;
            restoreItem.Folder = Folder;
            restoreItem.FolderForRestore = folder;
            restoreItem.Init();
            do
            {
                itemList = GetFolderItemsFromPlanAndCatalog(Folder, offset, pageCount);
                offset = itemList.NextOffset;
                restoreItem.AddItems(itemList.Items);
            } while (itemList.MoreAvailable);
            restoreItem.AddComplete();
        }

        protected abstract IRestoreDestinationFolder GetAndCreateIfFolderNotExistToExchange(IEnumerable<IFolderDataSync> folderHierarchy);
        protected abstract ItemList GetFolderItemsFromPlanAndCatalog(IFolderDataSync folder, int offset, int pageCount);
        protected abstract RestoreItemsFlowTemplate NewRestoreItemFlow();
    }

    public class ItemList
    {
        public IEnumerable<IItemDataSync> Items { get; set; }
        public bool MoreAvailable { get; set; }
        public int NextOffset { get; set; }
    }

    public abstract class RestoreItemsFlowTemplate : ITaskSyncContext<IJobProgress>
    {
        public IJobProgress Progress
        {
            get; set;
        }

        public CancellationToken CancelToken
        {
            get; set;
        }

        public TaskScheduler Scheduler
        {
            get; set;
        }
        public IRestoreToPosition<IJobProgress> RestoreToPosition { get; set; }
        public void InitTaskSyncContext(ITaskSyncContext<IJobProgress> mainContext)
        {
            this.CloneSyncContext(mainContext);
        }
        internal protected IRestoreDestinationFolder FolderForRestore { get; set; }
        internal protected IFolderDataSync Folder { get; set; }
        
        /// <summary>
        /// Before retore items in folder, this method will initialize some values.
        /// </summary>
        public abstract void Init();

        /// <summary>
        /// used to analysis items, which items is need to restore, and which items not.
        /// </summary>
        /// <param name="items"></param>
        protected abstract void AnalysisItems(IEnumerable<IItemDataSync> items);

        /// <summary>
        /// batch restore the items who need restore.
        /// </summary>
        /// <param name="isForceRestore"></param>
        protected abstract void RestoreItems(bool isForceRestore);

        public void AddItems(IEnumerable<IItemDataSync> items)
        {
            AnalysisItems(items);
            RestoreItems(false);
        }

        public void AddComplete()
        {
            RestoreItems(true);
        }
    }
}
