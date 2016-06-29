using Arcserve.Office365.Exchange.Data.Account;
using Arcserve.Office365.Exchange.Data.Increment;
using Arcserve.Office365.Exchange.DataProtect.Interface.Backup;
using Arcserve.Office365.Exchange.EwsApi.Interface;
using Arcserve.Office365.Exchange.Thread;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Exchange.WebServices.Data;
using Arcserve.Office365.Exchange.Util.Setting;

namespace Arcserve.Office365.Exchange.DataProtect.Impl.AsyncBackup
{
    public class AsyncBackupMailbox : BackupMailboxAsyncFlowTemplate, ITaskSyncContext<IJobProgress>, IExchangeAccess<IJobProgress>
    {
        public ICatalogAccess<IJobProgress> CatalogAccess { get; set; }
        public IEwsServiceAdapter<IJobProgress> EwsServiceAdapter { get; set; }
        public IDataFromBackup<IJobProgress> DataFromClient { get; set; }
        public OrganizationAdminInfo AdminInfo { get; internal set; }
        public string Organization { get; internal set; }
        public IDataConvert DataConvert
        {
            get; set;
        }
        public IMailboxDataSync MailboxInfo { get; set; }
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

        public async System.Threading.Tasks.Task ConnectExchangeService()
        {
            await EwsServiceAdapter.GetExchangeServiceAsync(MailboxInfo.MailAddress, AdminInfo);
        }

        public void InitTaskSyncContext(ITaskSyncContext<IJobProgress> mainContext)
        {
            this.CloneSyncContext(mainContext);
        }

        public async Task<ChangeCollection<FolderChange>> GetChangedFolders(string lastSyncStatus)
        {
            return await EwsServiceAdapter.SyncFolderHierarchyAsync(lastSyncStatus);
        }

        public BackupFolderAsyncFlowTemplate NewFolderTemplate()
        {
            var result = new AsyncBackupFolder();
            result.CloneSyncContext(this);
            result.CloneExchangeAccess(this);
            return result;
        }

        public async Task<IEnumerable<IFolderDataSync>> GetFoldersInLastCatalog(IMailboxDataSync mailboxData)
        {
            return await CatalogAccess.GetFoldersFromLatestCatalogAsync(mailboxData);
        }

        public FolderTree GetFolderTrees(IEnumerable<IFolderDataSync> array1, IEnumerable<IFolderDataSync> array2)
        {
            FolderTree result = new FolderTree();
            foreach (var f in array1)
            {
                result.AddNode(f);
            }
            foreach (var f in array2)
            {
                result.AddNode(f);
            }
            result.AddComplete();
            return result;
        }

        public bool IsFolderInPlan(string folderId)
        {
            return DataFromClient.IsFolderInPlan(folderId);
        }

        public bool IsFolderValid(IFolderDataSync folder)
        {
            return DataFromClient.IsFolderInPlan(folder);
        }

        public bool IsFolderClassValid(string folderClass)
        {
            return DataFromClient.IsFolderClassValid(folderClass);
        }

        public async System.Threading.Tasks.Task UpdateMailboxSyncAndTreeToCatalog(IMailboxDataSync mailbox)
        {
            await CatalogAccess.UpdateMailboxSyncAndTreeToCatalogAsync(mailbox);
        }

        public async System.Threading.Tasks.Task UpdateMailboxToCatalog(IMailboxDataSync mailbox)
        {
            await CatalogAccess.UpdateMailboxToCatalogAsync(mailbox);
        }

        public async System.Threading.Tasks.Task AddMailboxToCatalog(IMailboxDataSync mailbox)
        {
            await CatalogAccess.AddMailboxesToCatalogAsync(mailbox);
        }

        public async System.Threading.Tasks.Task LoadFolderProperties(Folder folders)
        {
            await EwsServiceAdapter.LoadFolderPropertiesAsync(folders);
        }

        public async System.Threading.Tasks.Task DeleteFolderToCatalog(string folderId)
        {
            await CatalogAccess.DeleteFolderToCatalogAsync(folderId);
        }

        public async System.Threading.Tasks.Task ForEachLoop(ICollection<IFolderDataSync> folders, ItemUADStatus itemStatus,
            Func<IFolderDataSync, ItemUADStatus, System.Threading.Tasks.Task> DoEachFolderChange)
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                Parallel.ForEach(folders,
                    new ParallelOptions()
                    {
                        TaskScheduler = Scheduler,
                        CancellationToken = CancelToken,
                        MaxDegreeOfParallelism = CloudConfig.Instance.MaxDegreeOfParallelismForFolder
                    }, (folder) =>
                     {
                         DoEachFolderChange(folder, itemStatus).Wait();
                     });
            });
        }

        public async System.Threading.Tasks.Task DoEachFolder(IFolderDataSync folder, ItemUADStatus itemStatus, FolderTree folderTree)
        {
            var folderTemplate = NewFolderTemplate();
            folderTemplate.FolderTree = folderTree;
            Progress.Report("  Folder {0} Start.", folder.Location);
            await folderTemplate.BackupAsync(folder, itemStatus);
        }

        public async System.Threading.Tasks.Task ForEachFolderChagnes(ChangeCollection<FolderChange> folderChanges,
            HashSet<string> folderDealed,
            List<IFolderDataSync> folderTreeItems,
            Dictionary<ItemUADStatus, List<IFolderDataSync>> folderDataChangeUAD,
            Dictionary<string, IFolderDataSync> foldersInDic)
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                Parallel.ForEach(folderChanges, new ParallelOptions()
                {
                    CancellationToken = CancelToken,
                    TaskScheduler = Scheduler,
                    MaxDegreeOfParallelism = CloudConfig.Instance.MaxDegreeOfParallelismForFolderChanges
                },
                (folderChange) =>
                {
                    DoEachFolderChanges(folderChange, folderDealed, folderTreeItems, folderDataChangeUAD, foldersInDic).Wait();
                });
            });
        }

        public async System.Threading.Tasks.Task DoEachFolderChanges(FolderChange folderChange,
            HashSet<string> folderDealed,
            List<IFolderDataSync> folderTreeItems,
            Dictionary<ItemUADStatus, List<IFolderDataSync>> folderDataChangeUAD,
            Dictionary<string, IFolderDataSync> foldersInDic)
        {
            if (folderChange.ChangeType == ChangeType.Delete)
            {
                await DeleteFolderToCatalog(folderChange.FolderId.UniqueId);
                folderDealed.Add(folderChange.FolderId.UniqueId);
            }
            else if (IsFolderInPlan(folderChange.Folder.Id.UniqueId))
            {
                await LoadFolderProperties(folderChange.Folder);

                if (IsFolderClassValid(folderChange.Folder.FolderClass))
                {
                    IFolderDataSync folderData = null;
                    switch (folderChange.ChangeType)
                    {
                        case ChangeType.Create:
                            folderData = DataConvert.Convert(folderChange.Folder, MailboxInfo);
                            folderTreeItems.Add(folderData);
                            if (IsFolderValid(folderData))
                            {
                                folderDataChangeUAD[ItemUADStatus.Add].Add(folderData);

                                folderDealed.Add(folderChange.FolderId.UniqueId);
                            }
                            break;
                        case Microsoft.Exchange.WebServices.Data.ChangeType.ReadFlagChange:
                        case Microsoft.Exchange.WebServices.Data.ChangeType.Update:
                            folderData = DataConvert.Convert(folderChange.Folder, MailboxInfo);
                            folderTreeItems.Add(folderData);
                            if (IsFolderValid(folderData))
                            {

                                folderData.SyncStatus = foldersInDic[folderChange.Folder.Id.UniqueId].SyncStatus;
                                folderDataChangeUAD[ItemUADStatus.Update].Add(folderData);

                                folderDealed.Add(folderChange.FolderId.UniqueId);
                            }
                            break;
                        case Microsoft.Exchange.WebServices.Data.ChangeType.Delete:
                            if (foldersInDic.ContainsKey(folderChange.Folder.Id.UniqueId))
                            {
                                foldersInDic.Remove(folderChange.Folder.Id.UniqueId);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public async System.Threading.Tasks.Task BackupAsync(ItemUADStatus uadStatus)
        {
            try
            {
                var connectTask = ConnectExchangeService();
                var foldersInLastCatalogTask = GetFoldersInLastCatalog(MailboxInfo);

                await System.Threading.Tasks.Task.WhenAll(connectTask, foldersInLastCatalogTask);
                var foldersInLastCatalog = foldersInLastCatalogTask.Result;

                Dictionary<string, IFolderDataSync> foldersInDic = null;
                if (foldersInLastCatalog == null)
                {
                    foldersInDic = new Dictionary<string, IFolderDataSync>();
                }
                else
                {
                    foldersInDic = new Dictionary<string, IFolderDataSync>(foldersInLastCatalog.Count());
                    foreach (var folder in foldersInLastCatalog)
                    {
                        foldersInDic.Add(folder.FolderId, folder);
                    }
                }

                ChangeCollection<FolderChange> folderChanges = null;

                HashSet<string> folderDealed = new HashSet<string>();
                var lastSyncStatus = MailboxInfo.SyncStatus;

                List<FolderChange> validFolders = new List<Microsoft.Exchange.WebServices.Data.FolderChange>();
                List<IFolderDataSync> folderTreeItems = new List<IFolderDataSync>();
                Dictionary<ItemUADStatus, List<IFolderDataSync>> folderDataChangeUAD = new Dictionary<ItemUADStatus, List<IFolderDataSync>>(4)
                {
                    {ItemUADStatus.Add, new List<IFolderDataSync>() },
                    {ItemUADStatus.Update, new List<IFolderDataSync>() },
                     {ItemUADStatus.None, new List<IFolderDataSync>() }
                };

                List<System.Threading.Tasks.Task> allTasks = new List<System.Threading.Tasks.Task>();
                do
                {
                    folderChanges = await GetChangedFolders(lastSyncStatus);
                    lastSyncStatus = folderChanges.SyncState;

                    if (folderChanges.Count == 0)
                        break;

                    var task = ForEachFolderChagnes(folderChanges, folderDealed, folderTreeItems, folderDataChangeUAD, foldersInDic);
                    allTasks.Add(task);

                } while (folderChanges.MoreChangesAvailable);

                await System.Threading.Tasks.Task.WhenAll(allTasks.ToArray());

                var folderTree = GetFolderTrees(folderTreeItems, new List<IFolderDataSync>(0));

                MailboxInfo.SyncStatus = lastSyncStatus;
                MailboxInfo.FolderTree = folderTree.Serialize();

                var tempTasks = new List<System.Threading.Tasks.Task>();
                System.Threading.Tasks.Task updateOrAddTask = null;
                switch (uadStatus)
                {
                    case ItemUADStatus.None:
                        updateOrAddTask = UpdateMailboxSyncAndTreeToCatalog(MailboxInfo);
                        break;
                    case ItemUADStatus.Update:
                        updateOrAddTask = UpdateMailboxToCatalog(MailboxInfo);
                        break;
                    case ItemUADStatus.Add:
                        updateOrAddTask = AddMailboxToCatalog(MailboxInfo);
                        break;
                    default:
                        throw new NotSupportedException();

                }
                if (updateOrAddTask != null)
                {
                    tempTasks.Add(updateOrAddTask);
                }

                List<IFolderDataSync> otherNeedDealedFolder = new List<IFolderDataSync>();
                foreach (var folder in foldersInLastCatalog)
                {
                    if (IsFolderInPlan(folder.Id) && !folderDealed.Contains(folder.Id))
                    {
                        otherNeedDealedFolder.Add(folder);
                    }
                }

                folderDataChangeUAD[ItemUADStatus.None].AddRange(otherNeedDealedFolder);

                foreach (var folderItems in folderDataChangeUAD)
                {
                    var task = ForEachLoop(folderItems.Value, folderItems.Key, (folder, itemStatus) =>
                   {
                       return DoEachFolder(folder, itemStatus, folderTree);
                   });
                    tempTasks.Add(task);
                }

                await System.Threading.Tasks.Task.WhenAll(tempTasks.ToArray());
            }
            finally
            {

            }
        }
    }
}
