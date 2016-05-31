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

        bool IsItemContentExist(string itemId);

        void AddMailboxesToCatalog(IEnumerable<IMailboxDataSync> mailboxes);
        System.Threading.Tasks.Task AddMailboxesToCatalogAsync(IEnumerable<IMailboxDataSync> mailboxes);
        void DeleteItemsToCatalog(IEnumerable<string> itemIds);
        System.Threading.Tasks.Task DeleteItemsToCatalogAsync(IEnumerable<string> itemIds);

        void UpdateFolderToCatalog(IFolderDataSync folder);
        System.Threading.Tasks.Task UpdateFolderToCatalogAsync(IFolderDataSync folder);
        void UpdateFolderSyncStatusToCatalog(IFolderDataSync folder);
        System.Threading.Tasks.Task UpdateFolderSyncStatusToCatalogAsync(IFolderDataSync folder);
        void UpdateItemsToCatalog(IEnumerable<IItemDataSync> items);
        System.Threading.Tasks.Task UpdateItemsToCatalogAsync(IEnumerable<IItemDataSync> items);

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
        void UpdateMailboxSyncToCatalog(IMailboxDataSync mailbox);
        System.Threading.Tasks.Task UpdateMailboxSyncToCatalogAsync(IMailboxDataSync mailbox);
        void DeleteFolderToCatalog(string folderId);
        System.Threading.Tasks.Task DeleteFolderToCatalogAsync(string folderId);
        void UpdateMailboxToCatalog(ICollection<IMailboxDataSync> mailboxes);
        System.Threading.Tasks.Task UpdateMailboxToCatalogAsync(ICollection<IMailboxDataSync> mailboxes);
        void DeleteMailboxToCatalog(ICollection<IMailboxDataSync> mailboxes);
        System.Threading.Tasks.Task DeleteMailboxToCatalogAsync(ICollection<IMailboxDataSync> mailboxes);
        void UpdateMailboxToCatalog(IMailboxDataSync mailbox);
        System.Threading.Tasks.Task UpdateMailboxToCatalogAsync(IMailboxDataSync mailbox);
        void AddMailboxesToCatalog(IMailboxDataSync mailbox);
        System.Threading.Tasks.Task AddMailboxesToCatalogAsync(IMailboxDataSync mailbox);
    }
}
