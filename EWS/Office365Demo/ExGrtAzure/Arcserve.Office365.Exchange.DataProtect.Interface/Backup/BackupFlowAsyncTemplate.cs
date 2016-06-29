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

    public interface BackupFlowAsyncTemplate : ITaskSyncContext<IJobProgress>, IDisposable
    {
        /// <summary>
        /// get all mailbox information from plan/client
        /// </summary>
        Task<ICollection<IMailboxDataSync>> GetAllMailboxFromPlan(Func<IEnumerable<string>, Task<ICollection<IMailboxDataSync>>> funcGetAllMailboxFromExchange);
        /// <summary>
        /// get all mailbox from exchange
        /// </summary>
        Task<ICollection<IMailboxDataSync>> GetAllMailboxFromExchange(IEnumerable<string> mailboxAddresses);
        /// <summary>
        /// get last catalog job information
        /// </summary>
        Task<ICatalogJob> GetLatestCatalogJob();
        /// <summary>
        /// get all mailbox information from last catalog .param:catalogjob.
        /// </summary>
        Task<IEnumerable<IMailboxDataSync>> GetAllMailboxFromLastCatalog(ICatalogJob catalogJob);

        /// <summary>
        /// get intersection mailbox collection from param1 mailbox from exchange and param2 mailbox from plan
        /// </summary>
        ICollection<IMailboxDataSync> GetIntersectionMailboxCollection(ICollection<IMailboxDataSync> mailboxInExchange, ICollection<IMailboxDataSync> mailboxInPlan);

        IDictionary<ItemUADStatus, ICollection<IMailboxDataSync>> GetMailboxUAD(ICollection<IMailboxDataSync> mailboxValid, IEnumerable<IMailboxDataSync> mailboxInLastCatalog);
        /// <summary>
        /// add mailbox to catalog.
        /// </summary>
        Task AddMailboxToCurrentCatalog(ICollection<IMailboxDataSync> mailboxes);

        Task UpdateMailboxToCurrentCatalog(ICollection<IMailboxDataSync> mailboxes);

        Task DeleteMailboxToCurrentCatalog(ICollection<IMailboxDataSync> mailboxes);

        Task DoEachMailbox(IMailboxDataSync mailbox, ItemUADStatus mailboxStatus);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        /// <param name="DoEachMailbox"></param>
        Task ForEachLoop(ICollection<IMailboxDataSync> items, ItemUADStatus uadStatus, Func<IMailboxDataSync, ItemUADStatus, Task> FuncDoEachMailbox);

        BackupMailboxAsyncFlowTemplate NewMailboxTemplate();
        Task BackupAsync();
    }


    public interface BackupMailboxAsyncFlowTemplate : ITaskSyncContext<IJobProgress>
    {
        IMailboxDataSync MailboxInfo { get; set; }
        Task ConnectExchangeService();

        Task<EwsWSData.ChangeCollection<EwsWSData.FolderChange>> GetChangedFolders(string syncStatus);

        BackupFolderAsyncFlowTemplate NewFolderTemplate();
        Task<IEnumerable<IFolderDataSync>> GetFoldersInLastCatalog(IMailboxDataSync mailbox);

        FolderTree GetFolderTrees(IEnumerable<IFolderDataSync> folderList1, IEnumerable<IFolderDataSync> folderList2);

        bool IsFolderInPlan(string folderId);

        bool IsFolderValid(IFolderDataSync folder);

        bool IsFolderClassValid(string folderClass);

        Task UpdateMailboxSyncAndTreeToCatalog(IMailboxDataSync mailbox);
        Task UpdateMailboxToCatalog(IMailboxDataSync mailbox);
        Task AddMailboxToCatalog(IMailboxDataSync mailbox);

        Task LoadFolderProperties(EwsWSData.Folder folders);

        Task DeleteFolderToCatalog(string folderId);

        System.Threading.Tasks.Task ForEachFolderChagnes(EwsWSData.ChangeCollection<EwsWSData.FolderChange> folderChanges,
            HashSet<string> folderDealed,
            List<IFolderDataSync> addOrUpdateFolders,
            Dictionary<ItemUADStatus, List<IFolderDataSync>> folderDataChangeUAD,
            Dictionary<string, IFolderDataSync> foldersInDic);

        System.Threading.Tasks.Task DoEachFolderChanges(EwsWSData.FolderChange folderChange,
            HashSet<string> folderDealed,
            List<IFolderDataSync> addOrUpdateFolders,
            Dictionary<ItemUADStatus, List<IFolderDataSync>> folderDataChangeUAD,
            Dictionary<string, IFolderDataSync> foldersInDic);

        Task ForEachLoop(ICollection<IFolderDataSync> folders, ItemUADStatus itemStatus, Func<IFolderDataSync, ItemUADStatus, Task> DoEachFolderChange);

        Task DoEachFolder(IFolderDataSync folder, ItemUADStatus folderStatus, FolderTree folderTree);

        Task BackupAsync(ItemUADStatus uadStatus);
    }

    public interface BackupFolderAsyncFlowTemplate : ITaskSyncContext<IJobProgress>
    {
        FolderTree FolderTree { get; set; }


        /// <summary>
        /// in: folderChange, out: folderLastSyncStatus from last catalog.
        /// </summary>
        /// 
        Task AddFolderToCatalog(IFolderDataSync folder);
        Task UpdateFolderToCatalog(IFolderDataSync folder);
        Task UpdateFolderStatusToCatalog(IFolderDataSync folder);


        Task<EwsWSData.ChangeCollection<EwsWSData.ItemChange>> GetChangedItems(string folderId, string lastSyncStatus);

        BackupItemAsyncFlow NewBackupItem();

        Task BackupAsync(IFolderDataSync folder, ItemUADStatus itemStatus);

        Task<string> BackupItemsAsync(IFolderDataSync folder, string folderLastSyncStatus);

    }

    public interface BackupItemAsyncFlow : ITaskSyncContext<IJobProgress>
    {
        Task DealItem(EwsWSData.ItemChange itemChange);
        Task DealFinish(HashSet<string> dealedItemIds);

        Task LoadPropertyForItems(IEnumerable<EwsWSData.Item> items, ItemClass itemClass);
        Task<int> WriteItemsToStorage(IEnumerable<IItemDataSync> item);
        Task AddItemsToCatalog(IEnumerable<IItemDataSync> items);
        Task DeleteItemsToCatalog(IEnumerable<string> itemIds);
        Task UpdateItemsToCatalog(IEnumerable<IItemDataSync> items);
        System.Threading.Tasks.Task AddItemToCatalog(IItemDataSync item);
        Task<IEnumerable<IItemDataSync>> GetItemsFromCatalog(IEnumerable<string> itemIds);
        Task<IEnumerable<IItemDataSync>> GetItemsByParentFolderIdFromCatalog(string parentFolderId);
        IEnumerable<IItemDataSync> RemoveInvalidItem(IEnumerable<IItemDataSync> invalidItems);
        bool IsItemChangeValid(string itemId);
        bool IsItemValid(IItemDataSync item);

        bool IsRewriteDataIfReadFlagChanged { get; }
        FolderTree FolderTree { get; set; }
        IFolderDataSync ParentFolder { get; set; }
    }
}
