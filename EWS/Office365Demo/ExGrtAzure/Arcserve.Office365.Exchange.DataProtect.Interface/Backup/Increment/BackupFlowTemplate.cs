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

namespace Arcserve.Office365.Exchange.DataProtect.Interface.Backup.Increment
{
    public abstract class BackupFlowTemplate : IDisposable
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
        /// get current mailbox which must save to current catalog. params1: validExchange, param2: mailboxInLastCatalog, return: allMailboxes except not in validExchange.
        /// </summary>
        //public abstract Func<ICollection<IMailboxDataSync>, ICollection<IMailboxDataSync>, ICollection<IMailboxDataSync>> FuncGetMailboxCatalog { get; }

        /// <summary>
        /// add mailbox to catalog.
        /// </summary>
        public abstract Action<ICollection<IMailboxDataSync>> AddMailboxToCurrentCatalog { get; }

        ///// <summary>
        ///// delete mailbox to catalog.
        ///// </summary>
        //public abstract Action<ICollection<IMailboxDataSync>> RemoveMailboxToCurrentCatalog { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        /// <param name="DoEachMailbox"></param>
        public abstract void ForEachLoop(ICollection<IMailboxDataSync> items, Action<IMailboxDataSync> DoEachMailbox);

        public abstract Func<BackupMailboxFlowTemplate> FuncNewMailboxTemplate { get; }
        public void BackupSync()
        {
            try
            {
                var mailboxesInExchange = FuncGetAllMailboxFromExchange();
                var mailboxesInPlan = FuncGetAllMailboxFromPlan();
                var catalogJob = FuncGetLatestCatalogJob();
                var mailboxesInLastCatalog = FuncGetAllMailboxFromLastCatalog(catalogJob);

                var mailboxesValid = FuncGetIntersectionMailboxCollection(mailboxesInExchange, mailboxesInPlan);
                //var mailboxChangedInCurrentCatalog = FuncGetMailboxCatalog(mailboxesValid, mailboxesInLastCatalog);


                AddMailboxToCurrentCatalog(mailboxesValid);
                //RemoveMailboxToCurrentCatalog(mailboxChangedInCurrentCatalog[1]);
                // todo test case: if a mailbox delete and a new mailbox whose name is same as deleted mailbox create. how to generate catalog.
                
                ForEachLoop(mailboxesValid, (mailbox) => {
                    var mailboxFlowTemplate = FuncNewMailboxTemplate();
                    mailboxFlowTemplate.MailboxInfo = mailbox;
                    mailboxFlowTemplate.BackupSync();
                });
            }
            finally
            {
            }
        }

        public void Dispose()
        {
        }
    }

    public abstract class BackupMailboxFlowTemplate
    {
        public IMailboxDataSync MailboxInfo { get; set; }
        public abstract Action ActionConnectExchangeService { get; }

        public abstract Func<string, EwsWSData.ChangeCollection<EwsWSData.FolderChange>> FuncGetChangedFolders { get; }

        public abstract Func<BackupFolderFlowTemplate> FuncNewFolderTemplate { get; }
        public abstract Func<IMailboxDataSync, IEnumerable<IFolderDataSync>> FuncGetFoldersInLastCatalog { get; }

        public abstract Func<IEnumerable<IFolderDataSync>, IEnumerable<EwsWSData.FolderChange>, TreeNode<IFolderDataSync>> FuncGetFolderTrees { get; }

        public abstract Func<string, bool> FuncIsFolderInPlan { get; }

        public abstract Action<IMailboxDataSync> ActionUpdateMailbox { get; }

        public abstract void ForEachLoop(ICollection<EwsWSData.FolderChange> folderChanges, Dictionary<string, IFolderDataSync> folderDic, Action<EwsWSData.FolderChange, Dictionary<string, IFolderDataSync>> DoEachFolderChange);

        public abstract void ForEachLoop(ICollection<IFolderDataSync> folders, Action<IFolderDataSync> DoEachFolderChange);

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
                else {
                    foldersInDic = new Dictionary<string, IFolderDataSync>(foldersInLastCatalog.Count());
                    foreach(var folder in foldersInLastCatalog)
                    {
                        foldersInDic.Add(folder.FolderId, folder);
                    }
                }
                
                EwsWSData.ChangeCollection<EwsWSData.FolderChange> folderChanges = null;

                Dictionary<string, EwsWSData.Folder> folderDealed = new Dictionary<string, EwsWSData.Folder>();
                var lastSyncStatus = MailboxInfo.SyncStatus;

                List<EwsWSData.FolderChange> validFolders = new List<Microsoft.Exchange.WebServices.Data.FolderChange>();
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
                            validFolders.Add(folderChange);
                            folderDealed.Add(folderChange.FolderId.UniqueId, folderChange.Folder);
                        }
                    }

                } while (folderChanges.MoreChangesAvailable);

                var folderTree = FuncGetFolderTrees(foldersInLastCatalog, validFolders);

                MailboxInfo.SyncStatus = lastSyncStatus;
                ActionUpdateMailbox(MailboxInfo);

                ForEachLoop(validFolders, foldersInDic, (folder, dic) =>
                {
                    var folderTemplate = FuncNewFolderTemplate();
                    folderTemplate.FolderTree = folderTree;
                    folderTemplate.BackupSync(folder, dic);
                });

                List<IFolderDataSync> otherNeedDealedFolder = new List<IFolderDataSync>();
                foreach (var folder in foldersInLastCatalog)
                {
                    if (FuncIsFolderInPlan(folder.Id) && !folderDealed.ContainsKey(folder.Id))
                    {
                        otherNeedDealedFolder.Add(folder);
                    }
                }

                ForEachLoop(otherNeedDealedFolder, (folder) =>
                {
                    var folderTemplate = FuncNewFolderTemplate();
                    folderTemplate.FolderTree = folderTree;
                    folderTemplate.BackupNoChangeFolderSync(folder);
                });
            }
            finally
            {
                
            }
        }
    }

    public abstract class BackupFolderFlowTemplate
    {
        public TreeNode<IFolderDataSync> FolderTree { get; set; }

        public abstract Action<EwsWSData.Folder> ActionLoadFolderProperties { get; }

        /// <summary>
        /// in: folderChange, out: folderLastSyncStatus from last catalog.
        /// </summary>
        /// 
        public abstract Action<IFolderDataSync> ActionAddFolderToCatalog { get; }

        //public abstract Func<string, string> FuncGetFolderLastSyncStatusInCatalog { get; }

        /// <summary>
        /// param1: folderId
        /// param2: folderSyncStatus
        /// </summary>
        //public abstract Action<IFolderDataSync, string> ActionUpdateFolderSyncToCatalog { get; }

        //public abstract Action<EwsWSData.FolderChange> ActionDeleteFolderToCatalog { get; }

        public abstract Func<EwsWSData.FolderId, string, EwsWSData.ChangeCollection<EwsWSData.ItemChange>> FuncGetChangedItems { get; }

        public abstract Func<BackupItemFlow> FuncNewBackupItem { get; }

        public abstract ProtectConfig BackupConfig { get; }

        public abstract IDataConvert DataConvert { get; }
        
        public void BackupSync(EwsWSData.FolderChange folderChange, Dictionary<string, IFolderDataSync> folderDics)
        {
            string lastSyncStatus = string.Empty;
            try {
                switch (folderChange.ChangeType)
                {
                    case EwsWSData.ChangeType.Create:
                        ActionLoadFolderProperties(folderChange.Folder);
                        IFolderDataSync addFolders = DataConvert.Convert(folderChange.Folder, FolderTree);
                        lastSyncStatus = BackupItemsSync(addFolders, string.Empty);
                        addFolders.SyncStatus = lastSyncStatus;
                        ActionAddFolderToCatalog(addFolders);
                        //ActionUpdateFolderSyncToCatalog(addFolders, lastSyncStatus);
                        break;
                    case Microsoft.Exchange.WebServices.Data.ChangeType.Update:
                    case Microsoft.Exchange.WebServices.Data.ChangeType.ReadFlagChange:
                        ActionLoadFolderProperties(folderChange.Folder);
                        IFolderDataSync oldFolder = folderDics[folderChange.Folder.Id.UniqueId];
                        IFolderDataSync newFolder = DataConvert.Convert(oldFolder, folderChange.Folder, FolderTree);

                        
                        //var folderLastSyncStatus = FuncGetFolderLastSyncStatusInCatalog(folderChange.FolderId.UniqueId);
                        lastSyncStatus = BackupItemsSync(newFolder, oldFolder.SyncStatus);
                        newFolder.SyncStatus = lastSyncStatus;
                        ActionAddFolderToCatalog(newFolder);
                        //ActionUpdateFolderSyncToCatalog(newFolder, lastSyncStatus);
                        break;
                    case Microsoft.Exchange.WebServices.Data.ChangeType.Delete:
                        //ActionDeleteFolderToCatalog(folderChange);
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
            finally
            {
            }
        }

        public void BackupNoChangeFolderSync(IFolderDataSync folder)
        {
            try {
                
                //var folderLastSyncStatus = FuncGetFolderLastSyncStatusInCatalog(folder.FolderId);
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
            List<EwsWSData.ItemChange> addedItems = new List<EwsWSData.ItemChange>(BackupConfig.OnceLoadpropertiesForItemsCount);
            List<EwsWSData.ItemChange> changedItems = new List<EwsWSData.ItemChange>(BackupConfig.OnceLoadpropertiesForItemsCount);
            List<EwsWSData.ItemChange> deletedItems = new List<EwsWSData.ItemChange>(BackupConfig.OnceLoadpropertiesForItemsCount);
            List<EwsWSData.ItemChange> readChangeItems = new List<EwsWSData.ItemChange>(BackupConfig.OnceLoadpropertiesForItemsCount);
            HashSet<string> dealItemIds = new HashSet<string>();
            do
            {
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

            return lastSyncStatus;
        }

    }

    public abstract class BackupItemFlow
    {
        public abstract IDataConvert DataConvert { get; }
        public void DealAdded(EwsWSData.ItemChange itemChange)
        {
            ICollection<EwsWSData.ItemChange> items = null;
            if (CheckCanBatchAdded(itemChange, out items))
            {
                BatchAddedItems(items);
            }
        }

        public void DealReadFlagChanged(EwsWSData.ItemChange itemChange)
        {
            ICollection<EwsWSData.ItemChange> items = null;
            if (CheckCanBatchAdded(itemChange, out items))
            {
                BatchChangeRead(items);
            }
        }

        public void DealUpdate(EwsWSData.ItemChange itemChange)
        {
            ICollection<EwsWSData.ItemChange> items = null;
            if (CheckCanBatchAdded(itemChange, out items))
            {
                BatchUpdate(items);
            }
        }

        public void DealDelete(EwsWSData.ItemChange itemChange)
        {
            ICollection<EwsWSData.ItemChange> items = null;
            if (CheckCanBatchAdded(itemChange, out items))
            {
                BatchDeleteItems(items);
            }
        }

        public virtual void DealFinish(HashSet<string> dealedItemIds)
        {
            IEnumerable<IItemDataSync> allItems = FuncGetItemsByParentFolderIdFromCatalog(ParentFolder.FolderId);
            if(allItems != null)
            {
                foreach(var item in allItems)
                {
                    if (!dealedItemIds.Contains(item.ItemId))
                    {
                        ActionAddItemToCatalog(item);
                    }
                }
            }
        }

        public abstract Action<IEnumerable<EwsWSData.Item>> ActionLoadPropertyForItems { get; }
        public abstract Action<IEnumerable<EwsWSData.Item>> ActionWriteItemsToStorage { get; }
        //public abstract Action<IEnumerable<EwsWSData.Item>> ActionUpdateReadFlagItemsToCatalog { get; }
        public abstract Action<IEnumerable<IItemDataSync>> ActionAddItemsToCatalog { get; }
        public abstract Action<IItemDataSync> ActionAddItemToCatalog { get; }
        public abstract Func<IEnumerable<string>, IEnumerable<IItemDataSync>> FuncGetItemsFromCatalog { get; }
        public abstract Func<string, IEnumerable<IItemDataSync>> FuncGetItemsByParentFolderIdFromCatalog { get; }
        //public abstract Action<IEnumerable<EwsWSData.Item>> ActionUpdateItemsToCatalog { get; }

        //public abstract Action<IEnumerable<EwsWSData.Item>> ActionDeleteItemsToCatalog { get; }
        protected abstract bool CheckCanBatchAdded(EwsWSData.ItemChange itemChange, out ICollection<EwsWSData.ItemChange> batchItems);
        protected abstract bool CheckCanBatchUpdate(EwsWSData.ItemChange itemChange, out ICollection<EwsWSData.ItemChange> batchItems);
        protected abstract bool CheckCanBatchDelete(EwsWSData.ItemChange itemChange, out ICollection<EwsWSData.ItemChange> batchItems);
        protected abstract bool CheckCanBatchReadChange(EwsWSData.ItemChange itemChange, out ICollection<EwsWSData.ItemChange> batchItems);

        protected virtual void BatchAddedItems(ICollection<EwsWSData.ItemChange> batchItems)
        {
            var items = from item in batchItems select item.Item;
            ActionLoadPropertyForItems(items);
            IEnumerable<IItemDataSync> itemDatas = DataConvert.Convert(items, ParentFolder);
            ActionAddItemsToCatalog(itemDatas);
            ActionWriteItemsToStorage(items);
        }
        protected virtual void BatchDeleteItems(ICollection<EwsWSData.ItemChange> batchItems)
        {
            //var items = from item in batchItems select item.Item;
            //ActionDeleteItemsToCatalog(items);
        }
        protected virtual void BatchUpdate(ICollection<EwsWSData.ItemChange> batchItems)
        {
            var items = from item in batchItems select item.Item;
            ActionLoadPropertyForItems(items);
            IEnumerable<IItemDataSync> itemDatas = DataConvert.Convert(items, ParentFolder);
            ActionAddItemsToCatalog(itemDatas);
            ActionWriteItemsToStorage(items);
        }
        protected virtual void BatchChangeRead(ICollection<EwsWSData.ItemChange> batchItems)
        {
            var items = from item in batchItems select item.Item;
            var ids = from item in batchItems select item.ItemId.UniqueId;
            var itemDatas = FuncGetItemsFromCatalog(ids);
            foreach(var item in itemDatas)
            {
                item.IsRead = !item.IsRead;
            }
            ActionAddItemsToCatalog(itemDatas);
            if (IsRewriteDataIfReadFlagChanged)
            {
                ActionWriteItemsToStorage(items);
            }
        }

        protected abstract bool IsRewriteDataIfReadFlagChanged { get; }
        public TreeNode<IFolderDataSync> FolderTree { get; internal set; }
        public IFolderDataSync ParentFolder { get; internal set; }
    }


}
