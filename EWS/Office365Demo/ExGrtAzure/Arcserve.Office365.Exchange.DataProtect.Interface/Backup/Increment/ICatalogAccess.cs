using Arcserve.Office365.Exchange.Data;
using Arcserve.Office365.Exchange.Data.Increment;
using Arcserve.Office365.Exchange.Thread;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Exchange.WebServices.Data;
using Arcserve.Office365.Exchange.EwsApi.Increment;

namespace Arcserve.Office365.Exchange.DataProtect.Interface.Backup.Increment
{
    public interface ICatalogAccess<ProgressType> : ITaskSyncContext<ProgressType>, IExportItemsOper
    {

        IEnumerable<IMailboxDataSync> GetMailboxesFromLatestCatalog(ICatalogJob catalogJob);
        Task<IEnumerable<IMailboxDataSync>> GetMailboxFromLatestCatalogAsync(ICatalogJob catalogJob);

        IEnumerable<IMailboxDataSync> GetMailboxesFromLatestCatalog(DateTime currentJobStartTime);
        Task<IEnumerable<IMailboxDataSync>> GetMailboxesFromLatestCatalogAsync(DateTime currentJobStartTime);

        void AddItemsToCatalog(IEnumerable<IItemDataSync> items);
        System.Threading.Tasks.Task AddItemsToCatalogAsync(IEnumerable<IItemDataSync> items);

        //void DeleteItemsToCatalog(IEnumerable<Item> items);
        //System.Threading.Tasks.Task DeleteItemsToCatalogAsync(IEnumerable<Item> items);

        bool IsItemContentExist(string itemId);

        void AddMailboxesToCatalog(IEnumerable<IMailboxDataSync> mailboxes);
        System.Threading.Tasks.Task AddMailboxesToCatalogAsync(IEnumerable<IMailboxDataSync> mailboxes);

        //void UpdateItems(IEnumerable<Item> items);
        //System.Threading.Tasks.Task UpdateItemsAsync(IEnumerable<Item> items);

        //void DeleteMailboxesToCatalog(ICollection<IMailboxDataSync> mailboxes);
        //System.Threading.Tasks.Task DeleteMailboxesToCatalogAsync(ICollection<IMailboxDataSync> mailboxes);

        //void UpdateReadFlagItems(IEnumerable<Item> itemsWithReadFlagChange);
        //System.Threading.Tasks.Task UpdateReadFlagItemsAsync(IEnumerable<Item> itemsWithReadFlagChange);

        //void DeleteFolderToCatalog(Folder folder);
        //System.Threading.Tasks.Task DeleteFolderToCatalogAsync(Folder folder);

        //void WriteItemsToStorage(IEnumerable<ItemDatas> items);
        //System.Threading.Tasks.Task WriteItemsToStorageAsync(IEnumerable<ItemDatas> items);

        IEnumerable<IFolderDataSync> GetFoldersFromLatestCatalog(IMailboxDataSync mailboxData);
        Task<IEnumerable<IFolderDataSync>> GetFoldersFromLatestCatalogAsync(IMailboxDataSync mailboxData);
        void AddFolderToCatalog(IFolderDataSync folder);
        System.Threading.Tasks.Task AddFolderToCatalogAsync(IFolderDataSync folder);
        void AddItemToCatalog(IItemDataSync item);
        System.Threading.Tasks.Task AddItemToCatalogAsync(IItemDataSync item);
        IEnumerable<IItemDataSync> GetItemsFromLatestCatalog(IEnumerable<string> itemIds);
        Task<IEnumerable<IItemDataSync>> GetItemsFromLatestCatalogAsync(IEnumerable<string> itemIds);
        IEnumerable<IItemDataSync> GetItemsByParentFolderIdFromCatalog(string parentFolderId);
        Task<IEnumerable<IItemDataSync>> GetItemsByParentFolderIdFromCatalogAsync(string parentFolderId);
        void UpdateMailbox(IMailboxDataSync mailbox);
        System.Threading.Tasks.Task UpdateMailboxAsync(IMailboxDataSync mailbox);

        /// <summary>
        /// Update folder by folderChange type
        /// </summary>
        /// <param name="folderChange"></param>
        /// <returns>last catalog folder sync status.</returns>
        //string UpdateFolderByChangeType(FolderChange folderChange);
        //Task<string> UpdateFolderByChangeTypeAsync(FolderChange folderChange);

        //void UpdateFolderSyncStatus(FolderId folderId, string newSyncStatus);
        //System.Threading.Tasks.Task UpdateFolderSyncStatusAsync(FolderId folderId, string newSyncStatus);

        //string GetFolderLastSyncStatusFromLatestCatalog(string folderId);
        //Task<string> GetFolderLastSyncStatusFromLatestCatalogAsync(string folderId);
    }
}
