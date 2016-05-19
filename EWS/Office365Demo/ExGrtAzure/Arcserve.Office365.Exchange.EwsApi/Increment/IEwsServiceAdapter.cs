using Arcserve.Office365.Exchange.Data.Account;
using Arcserve.Office365.Exchange.Data.Increment;
using Arcserve.Office365.Exchange.Thread;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Exchange.WebServices.Data;

namespace Arcserve.Office365.Exchange.EwsApi.Increment
{
    public interface IEwsServiceAdapter<ProgressType> : ITaskSyncContext<ProgressType>
    {
        Task<string> GetExchangeServiceAsync(string mailbox, OrganizationAdminInfo adminInfo);
        string GetExchangeService(string mailbox, OrganizationAdminInfo adminInfo);

        Task<ICollection<IMailboxDataSync>> GetAllMailboxesAsync(string adminUserName, string adminPassword);
        ICollection<IMailboxDataSync> GetAllMailboxes(string adminUserName, string adminPassword);

        ChangeCollection<FolderChange> SyncFolderHierarchy(string lastSyncStatus);
        Task<ChangeCollection<FolderChange>> SyncFoldersAsync(string lastSyncStatus);

        void LoadFolderProperties(Folder folder);
        System.Threading.Tasks.Task LoadFolderPropertiesAsync(Folder folder);
        ChangeCollection<ItemChange> SyncItems(FolderId folderId, string lastSyncStatus);
        Task<ChangeCollection<ItemChange>> SyncItemsAsync(FolderId folderId, string lastSyncStatus);
        void ExportItems(IEnumerable<Item> items, Action<IEnumerable<ItemDatas>> writeItemsToStorage);
        System.Threading.Tasks.Task ExportItemsAsync(IEnumerable<Item> items, Action<IEnumerable<Item>> writeItemsToStorage);
        void LoadItemsProperties(IEnumerable<Item> items);
        System.Threading.Tasks.Task LoadItemsPropertiesAsync(IEnumerable<Item> items);
    }
}
