using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EwsWSData = Microsoft.Exchange.WebServices.Data;
using Arcserve.Office365.Exchange.Data.Increment;

namespace Arcserve.Office365.Exchange.DataProtect.Interface.Backup.Increment
{
    public abstract class BackupFlowTemplate
    {
        public abstract Func<IList<IMailboxDataSync>> FuncGetAllMailboxFromPlan { get; }
        public abstract Func<IList<IMailboxDataSync>> FuncGetAllMailboxFromExchange { get; }
        public abstract Func<IList<IMailboxDataSync>> FuncGetAllMailboxFromLastCatalog { get; }

        public abstract Func<IList<IMailboxDataSync>, IList<IMailboxDataSync>, IList<IMailboxDataSync>> FuncGetValidMailbox { get; }
        public abstract Func<IList<IMailboxDataSync>, IList<IMailboxDataSync>, IList<IMailboxDataSync>> FuncGetMailboxCatalog { get; }
        public abstract Action<IList<IMailboxDataSync>> AddMailboxToCurrentCatalog { get; }

        public abstract void ForEachLoop(IList<IMailboxDataSync> items, Action<IMailboxDataSync> DoEachMailbox);

        public abstract BackupMailboxFlowTemplate MailboxTemplate { get; }
        public void BackupSync()
        {
            var mailboxesInExchange = FuncGetAllMailboxFromExchange();
            var mailboxesInPlan = FuncGetAllMailboxFromPlan();
            var mailboxesInLastCatalog = FuncGetAllMailboxFromLastCatalog();

            var mailboxesValid = FuncGetValidMailbox(mailboxesInExchange, mailboxesInPlan);
            var needAddMailboxInCurrentCatalog = FuncGetMailboxCatalog(mailboxesValid, mailboxesInLastCatalog);

            AddMailboxToCurrentCatalog(needAddMailboxInCurrentCatalog);

            ForEachLoop(mailboxesValid, MailboxTemplate.BackupSync);
        }
    }

    public abstract class BackupMailboxFlowTemplate
    {
        public EwsWSData.ExchangeService CurrentExchangeService { get; protected set; }
        public abstract Func<EwsWSData.ExchangeService> FuncGetExchangeService { get; }

        public abstract Func<string, EwsWSData.ChangeCollection<EwsWSData.FolderChange>> FuncGetChangedFolders { get; }

        public abstract BackupFolderFlowTemplate FolderTemplate { get; }
        public abstract void ForEachLoop(EwsWSData.ChangeCollection<EwsWSData.FolderChange> folderChanges, Action<EwsWSData.FolderChange> DoEachFolderChange);

        public void BackupSync(IMailboxDataSync mailbox)
        {
            CurrentExchangeService = FuncGetExchangeService();

            FolderTemplate.CurrentExchangeService = CurrentExchangeService;

            EwsWSData.ChangeCollection<EwsWSData.FolderChange> folderChanges = null;

            var lastSyncStatus = mailbox.SyncStatus;

            do
            {
                folderChanges = FuncGetChangedFolders(lastSyncStatus);
                lastSyncStatus = folderChanges.SyncState;

                if (folderChanges.Count == 0)
                    break;

                ForEachLoop(folderChanges, FolderTemplate.BackupSync);

            } while (folderChanges.MoreChangesAvailable);
        }
    }

    public abstract class BackupFolderFlowTemplate
    {
        public EwsWSData.ExchangeService CurrentExchangeService { get; set; }

        public abstract Func<EwsWSData.Folder, bool> FuncIsFolderInPlan { get; }

        /// <summary>
        /// in: folderChange, out: folderLastSyncStatus from last catalog.
        /// </summary>
        /// 
        public abstract Func<EwsWSData.FolderChange, string> FuncUpdateFolderToCatalog { get; }

        /// <summary>
        /// param1: folderId
        /// param2: folderSyncStatus
        /// </summary>
        public abstract Action<EwsWSData.FolderId, string> ActionUpdateFolderSync { get; }

        public abstract Action<EwsWSData.FolderChange> ActionDeleteFolderToCatalog { get; }

        public abstract Func<EwsWSData.FolderId, string, EwsWSData.ChangeCollection<EwsWSData.ItemChange>> FuncGetChangedItems { get; }

        public abstract Func<IEnumerable<EwsWSData.Item>, IList<EwsWSData.Item>> FuncLoadPropertyForItems { get; }

        public abstract Action<IEnumerable<EwsWSData.Item>> ActionUpdateItemsToCatalog { get; }

        public abstract Action<IEnumerable<EwsWSData.Item>> ActionWriterItemsToStorage { get; }

        public abstract Action<IEnumerable<EwsWSData.Item>> ActionAddItemsToCatalog { get; }

        public abstract Action<IEnumerable<EwsWSData.Item>> ActionDeleteItemsToCatalog { get; }
        public abstract Action<IEnumerable<EwsWSData.Item>> ActionDeleteItemsToStorage { get; }

        public abstract ProtectConfig BackupConfig { get; }
        public void BackupSync(EwsWSData.FolderChange folderChange)
        {
            var isInPlan = FuncIsFolderInPlan(folderChange.Folder);

            if (isInPlan)
            {
                switch (folderChange.ChangeType)
                {
                    case EwsWSData.ChangeType.Create:
                    case Microsoft.Exchange.WebServices.Data.ChangeType.Update:
                    case Microsoft.Exchange.WebServices.Data.ChangeType.ReadFlagChange:
                        var folderLastSyncStatus = FuncUpdateFolderToCatalog(folderChange);
                        var lastSyncStatus = BackupItemsSync(folderChange, folderChange.FolderId, folderLastSyncStatus);
                        ActionUpdateFolderSync(folderChange.FolderId, lastSyncStatus);
                        break;
                    case Microsoft.Exchange.WebServices.Data.ChangeType.Delete:
                        ActionDeleteFolderToCatalog(folderChange);
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        protected string BackupItemsSync(EwsWSData.FolderChange folderChange, EwsWSData.FolderId folderId, string folderLastSyncStatus)
        {
            EwsWSData.ChangeCollection<EwsWSData.ItemChange> itemChanges = null;

            var lastSyncStatus = folderLastSyncStatus;

            List<EwsWSData.ItemChange> addedItems = new List<EwsWSData.ItemChange>(BackupConfig.OnceLoadpropertiesForItemsCount);
            List<EwsWSData.ItemChange> changedItems = new List<EwsWSData.ItemChange>(BackupConfig.OnceLoadpropertiesForItemsCount);
            List<EwsWSData.ItemChange> deletedItems = new List<EwsWSData.ItemChange>(BackupConfig.OnceLoadpropertiesForItemsCount);
            do
            {
                itemChanges = FuncGetChangedItems(folderId, lastSyncStatus);
                lastSyncStatus = itemChanges.SyncState;

                if (itemChanges.Count == 0)
                    break;

                foreach (var itemChange in itemChanges)
                {
                    switch (itemChange.ChangeType)
                    {
                        case EwsWSData.ChangeType.Create:
                            addedItems.Add(itemChange);
                            break;
                        case Microsoft.Exchange.WebServices.Data.ChangeType.Update:
                        case Microsoft.Exchange.WebServices.Data.ChangeType.ReadFlagChange:
                            changedItems.Add(itemChange);
                            break;
                        case Microsoft.Exchange.WebServices.Data.ChangeType.Delete:
                            deletedItems.Add(itemChange);
                            break;
                    }

                    DealAddedItems(addedItems);
                    DealChangedItems(changedItems);
                    DealDeletedItems(deletedItems);
                }

                DealAddedItems(addedItems);
                DealChangedItems(changedItems);
                DealDeletedItems(deletedItems);

            } while (itemChanges.MoreChangesAvailable);

            DealAddedItems(addedItems, true);
            DealChangedItems(changedItems, true);
            DealDeletedItems(deletedItems, true);

            return lastSyncStatus;
        }

        private void DealChangedItems(List<EwsWSData.ItemChange> changedItems, bool forceUpdated = false)
        {
            if (changedItems.Count >= BackupConfig.OnceLoadpropertiesForItemsCount || forceUpdated)
            {
                var temp = changedItems;
                changedItems = new List<EwsWSData.ItemChange>(BackupConfig.OnceLoadpropertiesForItemsCount);
                var items = from item in temp select item.Item;
                FuncLoadPropertyForItems(items);
                ActionUpdateItemsToCatalog(items);
                ActionWriterItemsToStorage(items);
            }
        }

        private void DealAddedItems(List<EwsWSData.ItemChange> addedItems, bool forceUpdated = false)
        {
            if (addedItems.Count >= BackupConfig.OnceLoadpropertiesForItemsCount || forceUpdated)
            {
                var temp = addedItems;
                addedItems = new List<EwsWSData.ItemChange>(BackupConfig.OnceLoadpropertiesForItemsCount);
                var items = from item in temp select item.Item;
                FuncLoadPropertyForItems(items);
                ActionAddItemsToCatalog(items);
                ActionWriterItemsToStorage(items);
            }
        }

        private void DealDeletedItems(List<EwsWSData.ItemChange> deletedItems, bool forceUpdated = false)
        {
            if (deletedItems.Count >= BackupConfig.OnceLoadpropertiesForItemsCount || forceUpdated)
            {
                var temp = deletedItems;
                deletedItems = new List<EwsWSData.ItemChange>(BackupConfig.OnceLoadpropertiesForItemsCount);
                var items = from item in temp select item.Item;
                ActionDeleteItemsToCatalog(items);
                ActionDeleteItemsToStorage(items);
            }
        }
    }

    
}
