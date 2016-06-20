using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arcserve.Office365.Exchange.DataProtect.Interface.Backup;
using Arcserve.Office365.Exchange.Thread;
using Arcserve.Office365.Exchange.Data;
using Arcserve.Office365.Exchange.Data.Increment;
using System.Threading;
using Microsoft.Exchange.WebServices.Data;
using Arcserve.Office365.Exchange.EwsApi.Interface;

namespace Arcserve.Office365.Exchange.DataProtect.Impl.AsyncBackup
{
    public class AsyncBackupFolder : BackupFolderAsyncFlowTemplate, ITaskSyncContext<IJobProgress>, IExchangeAccess<IJobProgress>
    {
        public Data.Increment.FolderTree FolderTree { get; set; }
        public ICatalogAccess<IJobProgress> CatalogAccess { get; set; }
        public IEwsServiceAdapter<IJobProgress> EwsServiceAdapter { get; set; }
        public IDataFromClient<IJobProgress> DataFromClient { get; set; }
        public IDataConvert DataConvert
        {
            get; set;
        }

        public async System.Threading.Tasks.Task BackupAsync(IFolderDataSync folder, ItemUADStatus itemStatus)
        {
            try
            {
                var lastSyncStatus = await BackupItemsAsync(folder, folder.SyncStatus);
                folder.SyncStatus = lastSyncStatus;

                switch (itemStatus)
                {
                    case ItemUADStatus.None:
                        await UpdateFolderStatusToCatalog(folder);
                        break;
                    case ItemUADStatus.Update:
                        await UpdateFolderToCatalog(folder);
                        break;
                    case ItemUADStatus.Delete:
                        throw new ArgumentException("Can't deal delete items.");
                    case ItemUADStatus.Add:
                        await AddFolderToCatalog(folder);
                        break;
                }
            }
            finally
            {
            }
        }

        public async Task<string> BackupItemsAsync(IFolderDataSync folder, string folderLastSyncStatus)
        {
            ChangeCollection<ItemChange> itemChanges = null;

            var lastSyncStatus = folderLastSyncStatus;
            var backupItemFlow = NewBackupItem();
            backupItemFlow.FolderTree = FolderTree;
            backupItemFlow.ParentFolder = folder;
            HashSet<string> dealItemIds = new HashSet<string>();

            Progress.Report("  Folder {0} items changed  Start.", folder.Location);
            do
            {
                itemChanges = await GetChangedItems(folder.FolderId, lastSyncStatus);
                lastSyncStatus = itemChanges.SyncState;

                if (itemChanges.Count == 0)
                    break;

                foreach (var itemChange in itemChanges)
                {
                    backupItemFlow.DealItem(itemChange);
                    dealItemIds.Add(itemChange.ItemId.UniqueId);
                }

            } while (itemChanges.MoreChangesAvailable);

            backupItemFlow.DealFinish(dealItemIds);
            
            return lastSyncStatus;
        }

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

        public async System.Threading.Tasks.Task AddFolderToCatalog(IFolderDataSync folder)
        {
            await CatalogAccess.AddFolderToCatalogAsync(folder);
        }

        public async System.Threading.Tasks.Task UpdateFolderToCatalog(IFolderDataSync folder)
        {
            await CatalogAccess.UpdateFolderToCatalogAsync(folder);
        }

        public async System.Threading.Tasks.Task UpdateFolderStatusToCatalog(IFolderDataSync folder)
        {
            await CatalogAccess.UpdateFolderSyncStatusToCatalogAsync(folder);
        }

        public async Task<ChangeCollection<ItemChange>> GetChangedItems(string folderId, string lastSyncStatus)
        {
            return await EwsServiceAdapter.SyncItemsAsync(folderId, lastSyncStatus);
        }

        public BackupItemAsyncFlow NewBackupItem()
        {
            var result = new AsyncBackupItem();
            result.CloneSyncContext(this);
            result.CloneExchangeAccess(this);
            return result;
        }
        
    }
}
