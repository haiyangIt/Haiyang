using Arcserve.Office365.Exchange.Data.Account;
using Arcserve.Office365.Exchange.Data.Increment;
using Arcserve.Office365.Exchange.Thread;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Exchange.WebServices.Data;
using Arcserve.Office365.Exchange.Data.Mail;

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
        int ExportItems(IEnumerable<IItemDataSync> items, IExportItemsOper exportItemOper);
        System.Threading.Tasks.Task<int> ExportItemsAsync(IEnumerable<IItemDataSync> items, IExportItemsOper exportItemOper);
        void LoadItemsProperties(IEnumerable<Item> items, ItemClass itemClass);
        System.Threading.Tasks.Task LoadItemsPropertiesAsync(IEnumerable<Item> items, ItemClass itemClass);
    }
}
