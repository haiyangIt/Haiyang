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
using Arcserve.Office365.Exchange.Log;

namespace Arcserve.Office365.Exchange.DataProtect.Interface.Backup
{
    public abstract class BackupFlowTemplate : ITaskSyncContext<IJobProgress>, IDisposable
    {
        /// <summary>
        /// get all mailbox information from plan/client
        /// </summary>
        protected abstract Func<ICollection<IMailboxDataSync>> FuncGetAllMailboxFromPlan { get; }
        /// <summary>
        /// get all mailbox from exchange
        /// </summary>
        protected abstract Func<IEnumerable<string>, ICollection<IMailboxDataSync>> FuncGetAllMailboxFromExchange { get; }
        /// <summary>
        /// get last catalog job information
        /// </summary>
        protected abstract Func<ICatalogJob> FuncGetLatestCatalogJob { get; }
        /// <summary>
        /// get all mailbox information from last catalog .param:catalogjob.
        /// </summary>
        protected abstract Func<ICatalogJob, IEnumerable<IMailboxDataSync>> FuncGetAllMailboxFromLastCatalog { get; }

        /// <summary>
        /// get intersection mailbox collection from param1 mailbox from exchange and param2 mailbox from plan
        /// </summary>
        protected abstract Func<ICollection<IMailboxDataSync>, ICollection<IMailboxDataSync>, ICollection<IMailboxDataSync>> FuncGetIntersectionMailboxCollection { get; }

        protected abstract Func<ICollection<IMailboxDataSync>, IEnumerable<IMailboxDataSync>, IDictionary<ItemUADStatus, ICollection<IMailboxDataSync>>> FuncGetMailboxUAD { get; }
        /// <summary>
        /// add mailbox to catalog.
        /// </summary>
        protected abstract Action<ICollection<IMailboxDataSync>> AddMailboxToCurrentCatalog { get; }

        protected abstract Action<ICollection<IMailboxDataSync>> UpdateMailboxToCurrentCatalog { get; }
        protected abstract Action<ICollection<IMailboxDataSync>> DeleteMailboxToCurrentCatalog { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        /// <param name="DoEachMailbox"></param>
        protected abstract void ForEachLoop(ICollection<IMailboxDataSync> items, ItemUADStatus uadStatus, Action<IMailboxDataSync, ItemUADStatus> DoEachMailbox);

        protected abstract Func<BackupMailboxFlowTemplate> FuncNewMailboxTemplate { get; }

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

        public int CompletedItemCount = 0;
        public long CompletedItemSize = 0;
        public int CompletedFolderCount = 0;
        public int CompletedMailboxCount = 0;

        public int TotalMailboxCount = 0;
        public int DealedMailboxCount = 0;

        public void BackupSync()
        {
            var pCounter = PerformanceCounter.Start();
            try
            {

                Progress.Report("Generate catalog {0} Start.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));

                var mailboxesInPlan = FuncGetAllMailboxFromPlan();
                var mailboxesInExchange = FuncGetAllMailboxFromExchange(from m in mailboxesInPlan select m.MailAddress);

                var catalogJob = FuncGetLatestCatalogJob();
                var mailboxesInLastCatalog = FuncGetAllMailboxFromLastCatalog(catalogJob);

                var mailboxesValid = FuncGetIntersectionMailboxCollection(mailboxesInExchange, mailboxesInPlan);
                var mailboxesUAD = FuncGetMailboxUAD(mailboxesValid, mailboxesInLastCatalog);

                DeleteMailboxToCurrentCatalog(mailboxesUAD[ItemUADStatus.Delete]);

                // AddMailboxToCurrentCatalog(mailboxesValid);
                TotalMailboxCount = mailboxesValid.Count;
                // todo test case: if a mailbox delete and a new mailbox whose name is same as deleted mailbox create. how to generate catalog.

                foreach (var mailboxesItem in mailboxesUAD)
                {
                    ForEachLoop(mailboxesItem.Value, mailboxesItem.Key, (mailbox, uadStatus) =>
                    {
                        Interlocked.Increment(ref DealedMailboxCount);
                        Progress.Report(0.01 * DealedMailboxCount / TotalMailboxCount);

                        var mailboxFlowTemplate = FuncNewMailboxTemplate();
                        mailboxFlowTemplate.MailboxInfo = mailbox;

                        var pCounter1 = PerformanceCounter.Start();
                        Progress.Report("Mailbox {0} Start.", mailbox.DisplayName);
                        mailboxFlowTemplate.BackupSync(uadStatus);
                        Progress.Report("Mailbox {0} End, Total folder count:{2},  total item count:{3}, total item size:{4}, TotalTime {1}h.",
                            mailbox.DisplayName, pCounter1.EndByHour(), mailboxFlowTemplate.TotalDealedFolderCount, mailboxFlowTemplate.TotalDealedItemCount, mailboxFlowTemplate.TotalDealedItemSize);

                        Interlocked.Add(ref CompletedItemCount, mailboxFlowTemplate.TotalDealedItemCount);
                        Interlocked.Add(ref CompletedItemSize, mailboxFlowTemplate.TotalDealedItemSize);
                        Interlocked.Add(ref CompletedFolderCount, mailboxFlowTemplate.TotalDealedFolderCount);
                        Interlocked.Increment(ref CompletedMailboxCount);

                    });
                }

                var endByHour = pCounter.EndByHour();
                Progress.Report("Generate catalog {0} successful, Total mailbox count:{5}, Total folder count:{2},  total item count:{3}, total item size:{4}, TotalTime {1}h. Speed:{6}G/H",
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), endByHour, CompletedFolderCount, CompletedItemCount, CompletedItemSize, CompletedMailboxCount, ((CompletedItemSize) / (endByHour * 1024 * 1024 * 1024)).ToString("0.00"));
            }
            catch (Exception e)
            {
                var endByHour = pCounter.EndByHour();
                LogFactory.LogInstance.WriteException(LogLevel.DEBUG, "Generate catalog End with error", e, e.Message);
                Progress.Report("Generate catalog {0} failed, Total mailbox count:{5}, Total folder count:{2},  total item count:{3}, total item size:{4}, TotalTime {1}h. Speed:{6}G/H",
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), endByHour, CompletedFolderCount, CompletedItemCount, CompletedItemSize, CompletedMailboxCount, ((CompletedItemSize) / (endByHour * 1024 * 1024 * 1024)).ToString("0.00"));
                throw new Exception(e.Message, e);
            }
            finally
            {
            }
        }

        public abstract void Dispose();
    }

    public enum ItemUADStatus
    {
        None = 0,
        Update = 1,
        Add = 2,
        Delete = 3
    }

    public abstract class BackupMailboxFlowTemplate : ITaskSyncContext<IJobProgress>
    {
        public IMailboxDataSync MailboxInfo { get; set; }
        protected abstract Action ActionConnectExchangeService { get; }

        protected abstract Func<string, EwsWSData.ChangeCollection<EwsWSData.FolderChange>> FuncGetChangedFolders { get; }

        protected abstract Func<BackupFolderFlowTemplate> FuncNewFolderTemplate { get; }
        protected abstract Func<IMailboxDataSync, IEnumerable<IFolderDataSync>> FuncGetFoldersInLastCatalog { get; }

        protected abstract Func<IEnumerable<IFolderDataSync>, IEnumerable<IFolderDataSync>, FolderTree> FuncGetFolderTrees { get; }

        protected abstract Func<string, bool> FuncIsFolderInPlan { get; }

        protected abstract Func<IFolderDataSync, bool> FuncIsFolderValid { get; }

        protected abstract Func<string, bool> FuncIsFolderClassValid { get; }

        protected abstract Action<IMailboxDataSync> ActionUpdateMailboxSyncToCatalog { get; }
        protected abstract Action<IMailboxDataSync> ActionUpdateMailboxToCatalog { get; }
        protected abstract Action<IMailboxDataSync> ActionAddMailboxToCatalog { get; }

        protected abstract Action<EwsWSData.Folder> ActionLoadFolderProperties { get; }

        protected abstract Action<string> ActionDeleteFolderToCatalog { get; }

        protected abstract void ForEachLoop(ICollection<IFolderDataSync> folders, ItemUADStatus itemStatus, Action<IFolderDataSync, ItemUADStatus> DoEachFolderChange);

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
            this.CloneSyncContext(mainContext);
        }

        public int TotalDealedItemCount = 0;
        public long TotalDealedItemSize = 0;
        public int TotalDealedFolderCount = 0;

        public void BackupSync(ItemUADStatus uadStatus)
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
                Dictionary<ItemUADStatus, List<IFolderDataSync>> folderDataChangeUAD = new Dictionary<ItemUADStatus, List<IFolderDataSync>>(4)
                {
                    {ItemUADStatus.Add, new List<IFolderDataSync>() },
                    {ItemUADStatus.Update, new List<IFolderDataSync>() },
                     {ItemUADStatus.None, new List<IFolderDataSync>() }
                };


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
                        if (folderChange.ChangeType == EwsWSData.ChangeType.Delete)
                        {
                            ActionDeleteFolderToCatalog(folderChange.FolderId.UniqueId);
                            folderDealed.Add(folderChange.FolderId.UniqueId);
                            continue;
                        }

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
                                    if (FuncIsFolderValid(folderData))
                                    {
                                        folderDataChangeUAD[ItemUADStatus.Add].Add(folderData);
                                        addOrUpdateFolders.Add(folderData);
                                        folderDealed.Add(folderChange.FolderId.UniqueId);
                                    }
                                    break;
                                case Microsoft.Exchange.WebServices.Data.ChangeType.ReadFlagChange:
                                case Microsoft.Exchange.WebServices.Data.ChangeType.Update:
                                    folderData = DataConvert.Convert(folderChange.Folder, MailboxInfo);
                                    if (FuncIsFolderValid(folderData))
                                    {

                                        folderData.SyncStatus = foldersInDic[folderChange.Folder.Id.UniqueId].SyncStatus;
                                        folderDataChangeUAD[ItemUADStatus.Update].Add(folderData);
                                        addOrUpdateFolders.Add(folderData);
                                        folderDealed.Add(folderChange.FolderId.UniqueId);
                                    }
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

                Progress.Report("Get Folder Change {0} End, total {1} changed folders, total time {2}.", MailboxInfo.MailAddress, addOrUpdateFolders.Count, pCounter.EndBySecond());

                var folderTree = FuncGetFolderTrees(foldersInLastCatalog, addOrUpdateFolders);

                MailboxInfo.SyncStatus = lastSyncStatus;

                switch (uadStatus)
                {
                    case ItemUADStatus.None:
                        ActionUpdateMailboxSyncToCatalog(MailboxInfo);
                        break;
                    case ItemUADStatus.Update:
                        ActionUpdateMailboxToCatalog(MailboxInfo);
                        break;
                    case ItemUADStatus.Add:
                        ActionAddMailboxToCatalog(MailboxInfo);
                        break;
                    default:
                        throw new NotSupportedException();

                }

                List<IFolderDataSync> otherNeedDealedFolder = new List<IFolderDataSync>();
                foreach (var folder in foldersInLastCatalog)
                {
                    if (FuncIsFolderInPlan(folder.Id) && !folderDealed.Contains(folder.Id))
                    {
                        otherNeedDealedFolder.Add(folder);
                    }
                }

                folderDataChangeUAD[ItemUADStatus.None].AddRange(otherNeedDealedFolder);


                foreach (var folderItems in folderDataChangeUAD)
                {
                    ForEachLoop(folderItems.Value, folderItems.Key, (folder, itemStatus) =>
                    {
                        var folderTemplate = FuncNewFolderTemplate();
                        folderTemplate.FolderTree = folderTree;

                        var pCounter1 = PerformanceCounter.Start();
                        Progress.Report("  Folder {0} Start.", folder.Location);
                        folderTemplate.BackupSync(folder, itemStatus);
                        Progress.Report("  Folder {0} End, TotalTime {1}s.", folder.Location, pCounter1.EndBySecond());
                        Interlocked.Add(ref TotalDealedItemCount, folderTemplate.TotalDealedItemCount);
                        Interlocked.Add(ref TotalDealedItemSize, folderTemplate.TotalDealedItemSize);
                        Interlocked.Increment(ref TotalDealedFolderCount);
                    });
                }

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
        protected abstract Action<IFolderDataSync> ActionAddFolderToCatalog { get; }
        protected abstract Action<IFolderDataSync> ActionUpdateFolderToCatalog { get; }
        protected abstract Action<IFolderDataSync> ActionUpdateFolderStatusToCatalog { get; }


        protected abstract Func<string, string, EwsWSData.ChangeCollection<EwsWSData.ItemChange>> FuncGetChangedItems { get; }

        protected abstract Func<BackupItemFlow> FuncNewBackupItem { get; }

        public long TotalDealedItemSize = 0;
        public int TotalDealedItemCount = 0;
        public void BackupSync(IFolderDataSync folder, ItemUADStatus itemStatus)
        {
            try
            {
                var lastSyncStatus = BackupItemsSync(folder, folder.SyncStatus);
                folder.SyncStatus = lastSyncStatus;

                switch (itemStatus)
                {
                    case ItemUADStatus.None:
                        ActionUpdateFolderStatusToCatalog(folder);
                        break;
                    case ItemUADStatus.Update:
                        ActionUpdateFolderToCatalog(folder);
                        break;
                    case ItemUADStatus.Delete:
                        throw new ArgumentException("Can't deal delete items.");
                    case ItemUADStatus.Add:
                        ActionAddFolderToCatalog(folder);
                        break;
                }
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
            do
            {
                itemChanges = FuncGetChangedItems(folder.FolderId, lastSyncStatus);
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

            Interlocked.Add(ref TotalDealedItemSize, backupItemFlow.TotalDealedItemSize);
            Interlocked.Add(ref TotalDealedItemCount, backupItemFlow.TotalDealedItemCount);

            Progress.Report("  {4} Folder {0} items changed  end, total items {1}, total size:{3}, total time : {2}s.",
                folder.Location, backupItemFlow.TotalDealedItemCount, pCounter.EndBySecond(), backupItemFlow.TotalDealedItemSize, TotalDealedItemCount.ToString("D7"));
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

    }

    public abstract class BackupItemFlow : ITaskSyncContext<IJobProgress>
    {
        public void DealItem(EwsWSData.ItemChange itemChange)
        {
            if (FuncIsItemChangeValid(itemChange.ItemId.UniqueId))
            {
                switch (itemChange.ChangeType)
                {
                    case EwsWSData.ChangeType.Create:
                        DealAdded(itemChange);
                        break;
                    case Microsoft.Exchange.WebServices.Data.ChangeType.Update:
                        DealUpdate(itemChange);
                        break;
                    case Microsoft.Exchange.WebServices.Data.ChangeType.ReadFlagChange:
                        DealReadFlagChanged(itemChange);
                        break;
                    case Microsoft.Exchange.WebServices.Data.ChangeType.Delete:
                        DealDelete(itemChange);
                        break;
                }
            }
        }

        public virtual void DealFinish(HashSet<string> dealedItemIds)
        {
            DealBatchLeft();
            //IEnumerable<IItemDataSync> allItems = FuncGetItemsByParentFolderIdFromCatalog(ParentFolder.FolderId);
            //if (allItems != null)
            //{
            //    foreach (var item in allItems)
            //    {
            //        if (FuncIsItemValid(item) && !dealedItemIds.Contains(item.ItemId))
            //        {
            //            ActionAddItemToCatalog(item);
            //        }
            //    }
            //}
        }

        public abstract IDataConvert DataConvert { get; set; }
        private void DealAdded(EwsWSData.ItemChange itemChange)
        {
            ICollection<EwsWSData.ItemChange> items = null;
            ItemClass itemClass = (ItemClass)(int)this.ParentFolder.FolderType.GetFolderClass();
            if (CheckCanBatchAdded(itemChange, itemClass, out items))
            {
                BatchAddedItems(items, itemClass);
            }
        }

        private void DealReadFlagChanged(EwsWSData.ItemChange itemChange)
        {
            ICollection<EwsWSData.ItemChange> items = null;
            ItemClass itemClass = (ItemClass)(int)ParentFolder.FolderType.GetFolderClass();
            if (CheckCanBatchReadChange(itemChange, itemClass, out items))
            {
                BatchChangeRead(items);

            }
        }

        private void DealUpdate(EwsWSData.ItemChange itemChange)
        {
            ICollection<EwsWSData.ItemChange> items = null;
            ItemClass itemClass = (ItemClass)(int)ParentFolder.FolderType.GetFolderClass();
            if (CheckCanBatchUpdate(itemChange, itemClass, out items))
            {
                BatchUpdate(items, itemClass);
            }
        }

        private void DealDelete(EwsWSData.ItemChange itemChange)
        {
            ICollection<EwsWSData.ItemChange> items = null;
            if (CheckCanBatchDelete(itemChange, out items))
            {
                BatchDeleteItems(items);
            }
        }



        protected abstract Action<IEnumerable<EwsWSData.Item>, ItemClass> ActionLoadPropertyForItems { get; }
        protected abstract Func<IEnumerable<IItemDataSync>, int> FuncWriteItemsToStorage { get; }
        protected abstract Action<IEnumerable<IItemDataSync>> ActionAddItemsToCatalog { get; }
        protected abstract Action<IEnumerable<string>> ActionDeleteItemsToCatalog { get; }
        protected abstract Action<IEnumerable<IItemDataSync>> ActionUpdateItemsToCatalog { get; }
        protected abstract Action<IItemDataSync> ActionAddItemToCatalog { get; }
        protected abstract Func<IEnumerable<string>, IEnumerable<IItemDataSync>> FuncGetItemsFromCatalog { get; }
        protected abstract Func<string, IEnumerable<IItemDataSync>> FuncGetItemsByParentFolderIdFromCatalog { get; }
        protected abstract Func<IEnumerable<IItemDataSync>, IEnumerable<IItemDataSync>> FuncRemoveInvalidItem { get; }
        protected abstract Func<string, bool> FuncIsItemChangeValid { get; }
        protected abstract Func<IItemDataSync, bool> FuncIsItemValid { get; }

        protected abstract bool CheckCanBatchAdded(EwsWSData.ItemChange itemChange, ItemClass itemClass, out ICollection<EwsWSData.ItemChange> batchItems);
        protected abstract bool CheckCanBatchUpdate(EwsWSData.ItemChange itemChange, ItemClass itemClass, out ICollection<EwsWSData.ItemChange> batchItems);
        protected abstract bool CheckCanBatchDelete(EwsWSData.ItemChange itemChange, out ICollection<EwsWSData.ItemChange> batchItems);
        protected abstract bool CheckCanBatchReadChange(EwsWSData.ItemChange itemChange, ItemClass itemClass, out ICollection<EwsWSData.ItemChange> batchItems);

        protected abstract Dictionary<ItemClass, List<EwsWSData.ItemChange>> GetLeftBatchAdded();
        protected abstract Dictionary<ItemClass, List<EwsWSData.ItemChange>> GetLeftBatchUpdated();
        protected abstract List<EwsWSData.ItemChange> GetLeftBatchReadChanged();
        protected abstract List<EwsWSData.ItemChange> GetLeftBatchDeleted();


        protected abstract IEnumerable<IEnumerable<IItemDataSync>> GetLeftWriteToStorageItems();

        protected abstract bool CheckCanWriteToStorage(IItemDataSync item, out IEnumerable<IEnumerable<IItemDataSync>> items);

        public long TotalDealedItemSize = 0;
        public int TotalDealedItemCount = 0;

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

            BatchDeleteItems(deleteItems);


            var writeToStorageItems = GetLeftWriteToStorageItems();
            var pCounter = PerformanceCounter.Start();
            foreach (var itemPartition in writeToStorageItems)
            {
                Progress.Report("      Items export and write {0} Start.", itemPartition.Count());
                pCounter.Restart();
                long size = FuncWriteItemsToStorage(itemPartition);
                Interlocked.Add(ref TotalDealedItemSize, size);
                Progress.Report("      Items export and write {0} End, total count {0}, total size {2}, total time {1}s.", itemPartition.Count(), pCounter.EndBySecond(), size);
            }
        }

        protected virtual void BatchAddedItems(ICollection<EwsWSData.ItemChange> batchItems, ItemClass itemClass)
        {
            var items = from item in batchItems select item.Item;

            var pCounter = PerformanceCounter.Start();
            Progress.Report("      Items add {0} Start.", batchItems.Count());
            ActionLoadPropertyForItems(items, itemClass);
            IEnumerable<IItemDataSync> itemDatas = DataConvert.Convert(items, ParentFolder);
            itemDatas = FuncRemoveInvalidItem(itemDatas);

            ActionAddItemsToCatalog(itemDatas);
            BatchWriteItemToStorage(itemDatas);

            var itemCount = itemDatas.Count();
            Interlocked.Add(ref TotalDealedItemCount, itemCount);
            Progress.Report("      Items add {0} End, total count {0}, total time {1}s.", itemCount, pCounter.EndBySecond());
        }
        protected virtual void BatchDeleteItems(ICollection<EwsWSData.ItemChange> batchItems)
        {
            var itemIds = from item in batchItems select item.ItemId.UniqueId;
            ActionDeleteItemsToCatalog(itemIds);
        }
        protected virtual void BatchUpdate(ICollection<EwsWSData.ItemChange> batchItems, ItemClass itemClass)
        {
            var items = from item in batchItems select item.Item;
            var pCounter = PerformanceCounter.Start();
            Progress.Report("      Items update {0} Start.", batchItems.Count());
            ActionLoadPropertyForItems(items, itemClass);
            IEnumerable<IItemDataSync> itemDatas = DataConvert.Convert(items, ParentFolder);

            itemDatas = FuncRemoveInvalidItem(itemDatas);
            ActionUpdateItemsToCatalog(itemDatas);
            BatchWriteItemToStorage(itemDatas);
            var itemCount = itemDatas.Count();
            Interlocked.Add(ref TotalDealedItemCount, itemCount);
            Progress.Report("      Items update {0} End, total count {0}, total time {1}s.", itemCount, pCounter.EndBySecond());
        }
        protected virtual void BatchChangeRead(ICollection<EwsWSData.ItemChange> batchItems)
        {
            if (batchItems == null || batchItems.Count == 0)
                return;
            var pCounter = PerformanceCounter.Start();
            Progress.Report("      Items readChange {0} Start.", batchItems.Count());

            var items = from item in batchItems select item.Item;
            var ids = from item in batchItems select item.ItemId.UniqueId;
            var itemToIsRead = new Dictionary<string, bool>(batchItems.Count);
            foreach (var itemChange in batchItems)
            {
                itemToIsRead.Add(itemChange.ItemId.UniqueId, itemChange.IsRead);
            }
            var itemDatas = FuncGetItemsFromCatalog(ids);
            itemDatas = FuncRemoveInvalidItem(itemDatas);

            foreach (var item in itemDatas)
            {
                item.IsRead = itemToIsRead[item.ItemId];
            }
            ActionAddItemsToCatalog(itemDatas);
            if (IsRewriteDataIfReadFlagChanged)
            {
                BatchWriteItemToStorage(itemDatas);
            }
            var itemCount = itemDatas.Count();
            Interlocked.Add(ref TotalDealedItemCount, itemCount);
            Progress.Report("      Items readChange {0} End, total count {0}, total time {1}s.", itemCount, pCounter.EndBySecond());
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
                        Progress.Report("      Items export and write {0} End, total count {0}, total size {2}, total time {1}s.", itemPartition.Count(), pCounter.EndBySecond(), size);
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
            this.CloneSyncContext(mainContext);
        }
    }
}
