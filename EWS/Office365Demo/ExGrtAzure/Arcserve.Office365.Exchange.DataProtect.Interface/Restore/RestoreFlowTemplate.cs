using Arcserve.Office365.Exchange.Data.Increment;
using Arcserve.Office365.Exchange.Thread;
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

        public void InitTaskSyncContext(ITaskSyncContext<IJobProgress> mainContext)
        {
            this.CloneSyncContext(mainContext);
        }

        public abstract void Dispose();

        public void RestoreSync()
        {
            try
            {
                var mailboxesInPlan = GetAllMailboxFromPlan();
                var mailboxesInExchange = GetAllMailboxFromExchange(mailboxesInPlan);
                var validMailboxes = GetValidMailboxes(mailboxesInPlan, mailboxesInExchange);
                
                foreach(var noexistMailbox in validMailboxes[ItemUADStatus.Delete])
                {
                    DealNotExistMailbox(noexistMailbox);
                }

                ForEachMailbox(validMailboxes[ItemUADStatus.None], RestoreMailbox);
            }
            catch(Exception e)
            {

            }
        }

        protected abstract IEnumerable<IMailboxDataSync> GetAllMailboxFromPlan();
        protected abstract IEnumerable<IMailboxDataSync> GetAllMailboxFromExchange(IEnumerable<IMailboxDataSync> selectedMailboxes);
        protected abstract IDictionary<ItemUADStatus, ICollection<IMailboxDataSync>> GetValidMailboxes(IEnumerable<IMailboxDataSync> selectedMailboxes, IEnumerable<IMailboxDataSync> mailboxesInExchange);
        protected abstract void DealNotExistMailbox(IMailboxDataSync notExistMailbox);
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

        public void InitTaskSyncContext(ITaskSyncContext<IJobProgress> mainContext)
        {
            this.CloneSyncContext(mainContext);
        }

        internal IMailboxDataSync MailboxInfo { get; set; }
        public void RestoreMailbox()
        {
            ConnectExchangeService();
            var folderTree = new FolderTree();
            folderTree.Deserialize(MailboxInfo.FolderTree);
            var foldersInPlan = GetFoldersFromPlan(MailboxInfo);
            foldersInPlan = LoadDetailInformationFromCatalog(foldersInPlan);
            ForEachFolder(foldersInPlan, DealFolder);
        }

        protected abstract void ConnectExchangeService();
        protected abstract IEnumerable<IFolderDataSync> GetFoldersFromPlan(IMailboxDataSync mailboxInfo);
        protected abstract IEnumerable<IFolderDataSync> LoadDetailInformationFromCatalog(IEnumerable<IFolderDataSync> foldersInPlan);
        protected abstract void ForEachFolder(IEnumerable<IFolderDataSync> folders, Action<IFolderDataSync, FolderTree> actionDoFolder);
        protected abstract RestoreFolderFlowTemplate NewRestoreFolderTemplate();
        protected virtual void DealFolder(IFolderDataSync folder, FolderTree folderTree)
        {
            var folderFlow = NewRestoreFolderTemplate();
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
            var items = GetFolderItemsFromPlan(Folder);
            items = LoadDetailsFromCatalog(items);
            ForEachItems(items, folder, DoItem);
        }

        protected abstract Folder GetAndCreateIfFolderNotExistToExchange(IEnumerable<IFolderDataSync> folderHierarchy);
        protected abstract IEnumerable<IItemDataSync> GetFolderItemsFromPlan(IFolderDataSync folder);
        protected abstract IEnumerable<IItemDataSync> LoadDetailsFromCatalog(IEnumerable<IItemDataSync> items);
        protected abstract void ForEachItems(IEnumerable<IItemDataSync> items, Folder folder, Action<IItemDataSync, Folder> actionDoItem);
        protected abstract RestoreItemFlowTemplate NewRestoreItemFlow();
        protected virtual void DoItem(IItemDataSync item, Folder folder)
        {
            var restoreItem = NewRestoreItemFlow();
            restoreItem.Folder = Folder;
            restoreItem.FolderInExchange = folder;
            restoreItem.RestoreItem(item);
        }
    }

    public class RestoreItemFlowTemplate
    {
        internal Folder FolderInExchange { get; set; }
        internal IFolderDataSync Folder { get; set; }

        public void RestoreItem(IItemDataSync item)
        {

        }
    }
}
