/*
Note: for the catalog file, wo only write a new file/create a new db file to save the new catalog. the latest catalog (db)file only used to query.
So we don't need delete any thing.
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EwsWSData = Microsoft.Exchange.WebServices.Data;
using Arcserve.Office365.Exchange.Data.Increment;
using Arcserve.Office365.Exchange.Data;
using Arcserve.Office365.Exchange.Util;
using Arcserve.Office365.Exchange.Data.Mail;
using Arcserve.Office365.Exchange.Util.Setting;
using Arcserve.Office365.Exchange.Thread;

namespace Arcserve.Office365.Exchange.DataProtect.Interface.Backup.Increment
{
    public abstract class BackupFlowTemplate : ITaskSyncContext<IJobProgress>, IDisposable
    {
        /// <summary>
        /// get all mailbox information from plan/client
        /// </summary>
        public abstract Func<ICollection<IMailboxDataSync>> FuncGetAllMailboxFromPlan { get; }
        /// <summary>
        /// get all mailbox from exchange
        /// </summary>
        public abstract Func<ICollection<IMailboxDataSync>> FuncGetAllMailboxFromExchange { get; }
        /// <summary>
        /// get last catalog job information
        /// </summary>
        public abstract Func<ICatalogJob> FuncGetLatestCatalogJob { get; }
        /// <summary>
        /// get all mailbox information from last catalog .param:catalogjob.
        /// </summary>
        public abstract Func<ICatalogJob, IEnumerable<IMailboxDataSync>> FuncGetAllMailboxFromLastCatalog { get; }

        /// <summary>
        /// get intersection mailbox collection from param1 mailbox from exchange and param2 mailbox from plan
        /// </summary>
        public abstract Func<ICollection<IMailboxDataSync>, ICollection<IMailboxDataSync>, ICollection<IMailboxDataSync>> FuncGetIntersectionMailboxCollection { get; }

        /// <summary>
        /// add mailbox to catalog.
        /// </summary>
        public abstract Action<ICollection<IMailboxDataSync>> AddMailboxToCurrentCatalog { get; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        /// <param name="DoEachMailbox"></param>
        public abstract void ForEachLoop(ICollection<IMailboxDataSync> items, Action<IMailboxDataSync> DoEachMailbox);

        public abstract Func<BackupMailboxFlowTemplate> FuncNewMailboxTemplate { get; }

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
            this.InitTaskSyncContext(mainContext);
        }

        public int TotalDealedItemCount = 0;
        public long TotalDealedItemSize = 0;
        public int TotalDealedFolderCount = 0;
        public int TotalDealedMailboxCount = 0;

        public void BackupSync()
        {
            try
            {
                var pCounter = PerformanceCounter.Start();
                Progress.Report("Generate catalog {0} Start.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));

                var mailboxesInExchange = FuncGetAllMailboxFromExchange();
                var mailboxesInPlan = FuncGetAllMailboxFromPlan();
                var catalogJob = FuncGetLatestCatalogJob();
                var mailboxesInLastCatalog = FuncGetAllMailboxFromLastCatalog(catalogJob);

                var mailboxesValid = FuncGetIntersectionMailboxCollection(mailboxesInExchange, mailboxesInPlan);

                AddMailboxToCurrentCatalog(mailboxesValid);
                // todo test case: if a mailbox delete and a new mailbox whose name is same as deleted mailbox create. how to generate catalog.

                ForEachLoop(mailboxesValid, (mailbox) =>
                {
                    var mailboxFlowTemplate = FuncNewMailboxTemplate();
                    mailboxFlowTemplate.MailboxInfo = mailbox;

                    var pCounter1 = PerformanceCounter.Start();
                    Progress.Report("Mailbox {0} Start.", mailbox.DisplayName);
                    mailboxFlowTemplate.BackupSync();
                    Progress.Report("Mailbox {0} End, Total folder count:{2},  total item count:{3}, total item size:{4}, TotalTime {1}h.",
                        mailbox.DisplayName, pCounter1.EndByHour(), mailboxFlowTemplate.TotalDealedFolderCount, mailboxFlowTemplate.TotalDealedItemCount, mailboxFlowTemplate.TotalDealedItemSize);

                    Interlocked.Add(ref TotalDealedItemCount, mailboxFlowTemplate.TotalDealedItemCount);
                    Interlocked.Add(ref TotalDealedItemSize, mailboxFlowTemplate.TotalDealedItemSize);
                    Interlocked.Add(ref TotalDealedFolderCount, mailboxFlowTemplate.TotalDealedFolderCount);
                    Interlocked.Increment(ref TotalDealedMailboxCount);

                });

                var endByHour = pCounter.EndByHour();
                Progress.Report("Generate catalog {0} End, Total mailbox count:{5}, Total folder count:{2},  total item count:{3}, total item size:{4}, TotalTime {1}h. Speed:{6}G/H",
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), endByHour, TotalDealedFolderCount, TotalDealedItemCount, TotalDealedItemSize, TotalDealedMailboxCount, ((TotalDealedItemSize) / (endByHour * 1024 * 1024 * 1024)).ToString("0.00"));
            }
            finally
            {
            }
        }

        public void Dispose()
        {
        }


    }

    public abstract class BackupMailboxFlowTemplate : ITaskSyncContext<IJobProgress>
    {
        public IMailboxDataSync MailboxInfo { get; set; }
        public abstract Action ActionConnectExchangeService { get; }

        public abstract Func<string, EwsWSData.ChangeCollection<EwsWSData.FolderChange>> FuncGetChangedFolders { get; }

        public abstract Func<BackupFolderFlowTemplate> FuncNewFolderTemplate { get; }
        public abstract Func<IMailboxDataSync, IEnumerable<IFolderDataSync>> FuncGetFoldersInLastCatalog { get; }

        public abstract Func<IEnumerable<IFolderDataSync>, IEnumerable<IFolderDataSync>, FolderTree> FuncGetFolderTrees { get; }

        public abstract Func<string, bool> FuncIsFolderInPlan { get; }

        public abstract Func<string, bool> FuncIsFolderClassValid { get; }

        public abstract Action<IMailboxDataSync> ActionUpdateMailbox { get; }
        public abstract Action<EwsWSData.Folder> ActionLoadFolderProperties { get; }

        public abstract void ForEachLoop(ICollection<IFolderDataSync> folders, Action<IFolderDataSync> DoEachFolderChange);

        public abstract IDataConvert DataConvert { get; set; }

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
            this.InitTaskSyncContext(mainContext);
        }

        public int TotalDealedItemCount = 0;
        public long TotalDealedItemSize = 0;
        public int TotalDealedFolderCount = 0;

        public void BackupSync()
        {
            try
            {
                ActionConnectExchangeService();

                var foldersInLastCatalog = FuncGetFoldersInLastCatalog(MailboxInfo);

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

                EwsWSData.ChangeCollection<EwsWSData.FolderChange> folderChanges = null;

                HashSet<string> folderDealed = new HashSet<string>();
                var lastSyncStatus = MailboxInfo.SyncStatus;

                List<EwsWSData.FolderChange> validFolders = new List<Microsoft.Exchange.WebServices.Data.FolderChange>();
                HashSet<string> deleteFolders = new HashSet<string>();
                List<IFolderDataSync> addOrUpdateFolders = new List<IFolderDataSync>();

                var pCounter = PerformanceCounter.Start();
                Progress.Report("Get Folder Change {0} Start.", MailboxInfo.MailAddress);
                do
                {
                    folderChanges = FuncGetChangedFolders(lastSyncStatus);
                    lastSyncStatus = folderChanges.SyncState;

                    if (folderChanges.Count == 0)
                        break;


                    foreach (var folderChange in folderChanges) // todo create folder hierechy. convert to IFolderDataSync.
                    {
                        if (FuncIsFolderInPlan(folderChange.Folder.Id.UniqueId))
                        {
                            ActionLoadFolderProperties(folderChange.Folder);

                            if (!FuncIsFolderClassValid(folderChange.Folder.FolderClass))
                            {
                                continue;
                            }

                            IFolderDataSync folderData = null;
                            switch (folderChange.ChangeType)
                            {
                                case EwsWSData.ChangeType.Create:
                                    folderData = DataConvert.Convert(folderChange.Folder, MailboxInfo);
                                    addOrUpdateFolders.Add(folderData);
                                    folderDealed.Add(folderChange.FolderId.UniqueId);
                                    break;
                                case Microsoft.Exchange.WebServices.Data.ChangeType.ReadFlagChange:
                                case Microsoft.Exchange.WebServices.Data.ChangeType.Update:
                                    folderData = DataConvert.Convert(folderChange.Folder, MailboxInfo);
                                    folderData.SyncStatus = foldersInDic[folderChange.Folder.Id.UniqueId].SyncStatus;
                                    addOrUpdateFolders.Add(folderData);
                                    folderDealed.Add(folderChange.FolderId.UniqueId);
                                    break;
                                case Microsoft.Exchange.WebServices.Data.ChangeType.Delete:
                                    if (foldersInDic.ContainsKey(folderChange.Folder.Id.UniqueId))
                                    {
                                        foldersInDic.Remove(folderChange.Folder.Id.UniqueId);
                                        deleteFolders.Add(folderChange.Folder.Id.UniqueId);
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }

                } while (folderChanges.MoreChangesAvailable);
                Progress.Report("Get Folder Change {0} End, total {1} changed folders, total time {2}m.", MailboxInfo.MailAddress, addOrUpdateFolders.Count, pCounter.EndByMinute());

                var folderTree = FuncGetFolderTrees(foldersInLastCatalog, addOrUpdateFolders);

                MailboxInfo.SyncStatus = lastSyncStatus;
                ActionUpdateMailbox(MailboxInfo);

                ForEachLoop(addOrUpdateFolders, (folder) =>
                {
                    var folderTemplate = FuncNewFolderTemplate();
                    folderTemplate.FolderTree = folderTree;

                    var pCounter1 = PerformanceCounter.Start();
                    Progress.Report("  Folder {0} Start.", folder.Location);
                    folderTemplate.BackupSync(folder);
                    Progress.Report("  Folder {0} End, TotalTime {1}s.", folder.Location, pCounter1.EndBySecond());
                    Interlocked.Add(ref TotalDealedItemCount, folderTemplate.TotalDealedItemCount);
                    Interlocked.Add(ref TotalDealedItemSize, folderTemplate.TotalDealedItemSize);
                    Interlocked.Increment(ref TotalDealedFolderCount);
                });

                List<IFolderDataSync> otherNeedDealedFolder = new List<IFolderDataSync>();
                foreach (var folder in foldersInLastCatalog)
                {
                    if (FuncIsFolderInPlan(folder.Id) && !folderDealed.Contains(folder.Id))
                    {
                        otherNeedDealedFolder.Add(folder);
                    }
                }

                ForEachLoop(otherNeedDealedFolder, (folder) =>
                {
                    var folderTemplate = FuncNewFolderTemplate();
                    folderTemplate.FolderTree = folderTree;
                    var pCounter1 = PerformanceCounter.Start();
                    Progress.Report("  Folder {0} Start.", folder.Location);
                    folderTemplate.BackupSync(folder);
                    Progress.Report("  Folder {0} End, TotalTime {1}s.", folder.Location, pCounter1.EndBySecond());
                    Interlocked.Add(ref TotalDealedItemCount, folderTemplate.TotalDealedItemCount);
                    Interlocked.Add(ref TotalDealedItemSize, folderTemplate.TotalDealedItemSize);
                    Interlocked.Increment(ref TotalDealedFolderCount);
                });
            }
            finally
            {

            }
        }


    }

    public abstract class BackupFolderFlowTemplate : ITaskSyncContext<IJobProgress>
    {
        public FolderTree FolderTree { get; set; }


        /// <summary>
        /// in: folderChange, out: folderLastSyncStatus from last catalog.
        /// </summary>
        /// 
        public abstract Action<IFolderDataSync> ActionAddFolderToCatalog { get; }


        public abstract Func<EwsWSData.FolderId, string, EwsWSData.ChangeCollection<EwsWSData.ItemChange>> FuncGetChangedItems { get; }

        public abstract Func<BackupItemFlow> FuncNewBackupItem { get; }

        public long TotalDealedItemSize = 0;
        public int TotalDealedItemCount = 0;
        public void BackupSync(IFolderDataSync folder)
        {
            try
            {
                var lastSyncStatus = BackupItemsSync(folder, folder.SyncStatus);
                folder.SyncStatus = lastSyncStatus;
                ActionAddFolderToCatalog(folder);
            }
            finally
            {

            }
        }

        protected string BackupItemsSync(IFolderDataSync folder, string folderLastSyncStatus)
        {
            EwsWSData.ChangeCollection<EwsWSData.ItemChange> itemChanges = null;

            var lastSyncStatus = folderLastSyncStatus;
            BackupItemFlow backupItemFlow = FuncNewBackupItem();
            backupItemFlow.FolderTree = FolderTree;
            backupItemFlow.ParentFolder = folder;
            HashSet<string> dealItemIds = new HashSet<string>();

            var pCounter = PerformanceCounter.Start();
            Progress.Report("  Folder {0} items changed  Start.", folder.Location);
            int itemCount = 0;
            do
            {
                itemCount++;
                itemChanges = FuncGetChangedItems(folder.FolderId, lastSyncStatus);
                lastSyncStatus = itemChanges.SyncState;

                if (itemChanges.Count == 0)
                    break;

                foreach (var itemChange in itemChanges)
                {
                    switch (itemChange.ChangeType)
                    {
                        case EwsWSData.ChangeType.Create:
                            backupItemFlow.DealAdded(itemChange);
                            break;
                        case Microsoft.Exchange.WebServices.Data.ChangeType.Update:
                            backupItemFlow.DealUpdate(itemChange);
                            dealItemIds.Add(itemChange.ItemId.UniqueId);
                            break;
                        case Microsoft.Exchange.WebServices.Data.ChangeType.ReadFlagChange:
                            backupItemFlow.DealReadFlagChanged(itemChange);
                            dealItemIds.Add(itemChange.ItemId.UniqueId);
                            break;
                        case Microsoft.Exchange.WebServices.Data.ChangeType.Delete:
                            backupItemFlow.DealDelete(itemChange);
                            dealItemIds.Add(itemChange.ItemId.UniqueId);
                            break;
                    }
                }

            } while (itemChanges.MoreChangesAvailable);

            backupItemFlow.DealFinish(dealItemIds);
            Progress.Report("  Folder {0} items changed  end, total items {1}, total size:{3}, total time : {2}m.", folder.Location, itemCount, pCounter.EndByMinute(), backupItemFlow.TotalDealedItemSize);
            Interlocked.Add(ref TotalDealedItemSize, backupItemFlow.TotalDealedItemSize);
            Interlocked.Add(ref TotalDealedItemCount, itemCount);
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
            this.InitTaskSyncContext(mainContext);
        }

    }

    public abstract class BackupItemFlow : ITaskSyncContext<IJobProgress>
    {
        public abstract IDataConvert DataConvert { get; set; }
        public void DealAdded(EwsWSData.ItemChange itemChange)
        {
            ICollection<EwsWSData.ItemChange> items = null;
            ItemClass itemClass = (ItemClass)(int)this.ParentFolder.FolderType.GetFolderClass();
            if (CheckCanBatchAdded(itemChange, itemClass, out items))
            {
                BatchAddedItems(items, itemClass);
            }
        }

        public void DealReadFlagChanged(EwsWSData.ItemChange itemChange)
        {
            ICollection<EwsWSData.ItemChange> items = null;
            ItemClass itemClass = (ItemClass)(int)ParentFolder.FolderType.GetFolderClass();
            if (CheckCanBatchReadChange(itemChange, itemClass, out items))
            {
                BatchChangeRead(items);

            }
        }

        public void DealUpdate(EwsWSData.ItemChange itemChange)
        {
            ICollection<EwsWSData.ItemChange> items = null;
            ItemClass itemClass = (ItemClass)(int)ParentFolder.FolderType.GetFolderClass();
            if (CheckCanBatchUpdate(itemChange, itemClass, out items))
            {
                BatchUpdate(items, itemClass);
            }
        }

        public void DealDelete(EwsWSData.ItemChange itemChange)
        {
            //ICollection<EwsWSData.ItemChange> items = null;
            //if (CheckCanBatchDelete(itemChange, out items))
            //{
            //    BatchDeleteItems(items);
            //}
        }

        public virtual void DealFinish(HashSet<string> dealedItemIds)
        {
            DealBatchLeft();
            IEnumerable<IItemDataSync> allItems = FuncGetItemsByParentFolderIdFromCatalog(ParentFolder.FolderId);
            if (allItems != null)
            {
                foreach (var item in allItems)
                {
                    if (!dealedItemIds.Contains(item.ItemId))
                    {
                        ActionAddItemToCatalog(item);
                    }
                }
            }
        }

        public abstract Action<IEnumerable<EwsWSData.Item>, ItemClass> ActionLoadPropertyForItems { get; }
        public abstract Func<IEnumerable<IItemDataSync>, int> FuncWriteItemsToStorage { get; }
        public abstract Action<IEnumerable<IItemDataSync>> ActionAddItemsToCatalog { get; }
        public abstract Action<IItemDataSync> ActionAddItemToCatalog { get; }
        public abstract Func<IEnumerable<string>, IEnumerable<IItemDataSync>> FuncGetItemsFromCatalog { get; }
        public abstract Func<string, IEnumerable<IItemDataSync>> FuncGetItemsByParentFolderIdFromCatalog { get; }

        protected abstract bool CheckCanBatchAdded(EwsWSData.ItemChange itemChange, ItemClass itemClass, out ICollection<EwsWSData.ItemChange> batchItems);
        protected abstract bool CheckCanBatchUpdate(EwsWSData.ItemChange itemChange, ItemClass itemClass, out ICollection<EwsWSData.ItemChange> batchItems);
        protected abstract bool CheckCanBatchDelete(EwsWSData.ItemChange itemChange, ItemClass itemClass, out ICollection<EwsWSData.ItemChange> batchItems);
        protected abstract bool CheckCanBatchReadChange(EwsWSData.ItemChange itemChange, ItemClass itemClass, out ICollection<EwsWSData.ItemChange> batchItems);

        protected abstract Dictionary<ItemClass, List<EwsWSData.ItemChange>> GetLeftBatchAdded();
        protected abstract Dictionary<ItemClass, List<EwsWSData.ItemChange>> GetLeftBatchUpdated();
        protected abstract List<EwsWSData.ItemChange> GetLeftBatchReadChanged();
        protected abstract Dictionary<ItemClass, List<EwsWSData.ItemChange>> GetLeftBatchDeleted();

        protected abstract IEnumerable<IEnumerable<IItemDataSync>> GetLeftWriteToStorageItems();

        protected abstract bool CheckCanWriteToStorage(IItemDataSync item, out IEnumerable<IEnumerable<IItemDataSync>> items);

        public long TotalDealedItemSize = 0;
        protected virtual void DealBatchLeft()
        {
            var addItems = GetLeftBatchAdded();
            foreach (var itemKeyValue in addItems)
            {
                BatchAddedItems(itemKeyValue.Value, itemKeyValue.Key);
            }
            var udpateItems = GetLeftBatchUpdated();
            foreach (var itemKeyValue in udpateItems)
            {
                BatchUpdate(itemKeyValue.Value, itemKeyValue.Key);
            }

            var readChangeItems = GetLeftBatchReadChanged();
            BatchChangeRead(readChangeItems);

            var deleteItems = GetLeftBatchDeleted();
            foreach (var itemKeyValue in deleteItems)
            {
                BatchDeleteItems(itemKeyValue.Value);
            }

            var writeToStorageItems = GetLeftWriteToStorageItems();
            foreach (var itemPartition in writeToStorageItems)
            {
                FuncWriteItemsToStorage(itemPartition);
            }
        }

        protected virtual void BatchAddedItems(ICollection<EwsWSData.ItemChange> batchItems, ItemClass itemClass)
        {
            var items = from item in batchItems select item.Item;

            var pCounter = PerformanceCounter.Start();
            Progress.Report("      Items add {0} Start.", batchItems.Count());
            ActionLoadPropertyForItems(items, itemClass);
            IEnumerable<IItemDataSync> itemDatas = DataConvert.Convert(items, ParentFolder);
            ActionAddItemsToCatalog(itemDatas);
            BatchWriteItemToStorage(itemDatas);
            Progress.Report("      Items add {0} End, total count {0}, total time {1}.", batchItems.Count(), pCounter.EndByMinute());
        }
        protected virtual void BatchDeleteItems(ICollection<EwsWSData.ItemChange> batchItems)
        {
            //var items = from item in batchItems select item.Item;
            //ActionDeleteItemsToCatalog(items);
        }
        protected virtual void BatchUpdate(ICollection<EwsWSData.ItemChange> batchItems, ItemClass itemClass)
        {
            var items = from item in batchItems select item.Item;
            var pCounter = PerformanceCounter.Start();
            Progress.Report("      Items update {0} Start.", batchItems.Count());
            ActionLoadPropertyForItems(items, itemClass);
            IEnumerable<IItemDataSync> itemDatas = DataConvert.Convert(items, ParentFolder);
            ActionAddItemsToCatalog(itemDatas);
            BatchWriteItemToStorage(itemDatas);
            Progress.Report("      Items update {0} End, total count {0}, total time {1}.", batchItems.Count(), pCounter.EndByMinute());
        }
        protected virtual void BatchChangeRead(ICollection<EwsWSData.ItemChange> batchItems)
        {
            if (batchItems == null || batchItems.Count == 0)
                return;
            var pCounter = PerformanceCounter.Start();
            Progress.Report("      Items readChange {0} Start.", batchItems.Count());

            var items = from item in batchItems select item.Item;
            var ids = from item in batchItems select item.ItemId.UniqueId;
            var itemDatas = FuncGetItemsFromCatalog(ids);
            foreach (var item in itemDatas)
            {
                item.IsRead = !item.IsRead;
            }
            ActionAddItemsToCatalog(itemDatas);
            if (IsRewriteDataIfReadFlagChanged)
            {
                BatchWriteItemToStorage(itemDatas);
            }
            Progress.Report("      Items readChange {0} End, total count {0}, total time {1}.", batchItems.Count(), pCounter.EndByMinute());
        }

        protected virtual void BatchWriteItemToStorage(IEnumerable<IItemDataSync> items)
        {
            var pCounter = PerformanceCounter.Start();
            foreach (var item in items)
            {
                IEnumerable<IEnumerable<IItemDataSync>> result;
                if (CheckCanWriteToStorage(item, out result))
                {
                    foreach (var itemPartition in result)
                    {
                        Progress.Report("      Items export and write {0} Start.", itemPartition.Count());
                        pCounter.Restart();
                        long size = FuncWriteItemsToStorage(itemPartition);
                        Interlocked.Add(ref TotalDealedItemSize, size);
                        Progress.Report("      Items export and write {0} End, total count {0}, total size {2}, total time {1}.", itemPartition.Count(), pCounter.EndByMinute(), size);
                    }
                }

            }
        }

        protected abstract bool IsRewriteDataIfReadFlagChanged { get; }
        public FolderTree FolderTree { get; internal set; }
        public IFolderDataSync ParentFolder { get; internal set; }

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
            this.InitTaskSyncContext(mainContext);
        }
    }
}
