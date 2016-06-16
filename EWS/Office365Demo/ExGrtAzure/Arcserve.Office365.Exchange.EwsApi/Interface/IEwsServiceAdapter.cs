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
using System.IO;

namespace Arcserve.Office365.Exchange.EwsApi.Interface
{
    public interface IEwsServiceAdapter<ProgressType> : ITaskSyncContext<ProgressType>
    {
        Task<string> GetExchangeServiceAsync(string mailbox, OrganizationAdminInfo adminInfo);
        string GetExchangeService(string mailbox, OrganizationAdminInfo adminInfo);

        Task<ICollection<IMailboxDataSync>> GetAllMailboxesAsync(string adminUserName, string adminPassword, IEnumerable<string> mailboxes);
        ICollection<IMailboxDataSync> GetAllMailboxes(string adminUserName, string adminPassword, IEnumerable<string> mailboxes);

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

        void ImportItem(Stream dataStream, FolderId folder);
    }

    public interface IEwsServiceAdapterExtension<ProgressType> : IEwsServiceAdapter<ProgressType>
    {
        Folder FolderBind(WellKnownFolderName name, PropertySet propertySet);
        Folder FolderBind(WellKnownFolderName name);
        Task<Folder> FolderBindAsync(WellKnownFolderName name, PropertySet propertySet);
        Task<Folder> FolderBindAsync(WellKnownFolderName name);

        FindFoldersResults FindFolders(WellKnownFolderName parentFolderName, FolderView view);
        Task<FindFoldersResults> FindFoldersAsync(WellKnownFolderName parentFolderName, FolderView view);

        //
        // Summary:
        //     Obtains a list of folders by searching the sub-folders of the specified folder.
        //
        // Parameters:
        //   parentFolderId:
        //     The Id of the folder in which to search for folders.
        //
        //   view:
        //     The view controlling the number of folders returned.
        //
        // Returns:
        //     An object representing the results of the search operation.
        FindFoldersResults FindFolders(FolderId parentFolderId, FolderView view);
        Task<FindFoldersResults> FindFoldersAsync(FolderId parentFolderId, FolderView view);

        //
        // Summary:
        //     Obtains a list of folders by searching the sub-folders of the specified folder.
        //
        // Parameters:
        //   parentFolderName:
        //     The name of the folder in which to search for folders.
        //
        //   searchFilter:
        //     The search filter. Available search filter classes include SearchFilter.IsEqualTo,
        //     SearchFilter.ContainsSubstring and SearchFilter.SearchFilterCollection
        //
        //   view:
        //     The view controlling the number of folders returned.
        //
        // Returns:
        //     An object representing the results of the search operation.
        FindFoldersResults FindFolders(WellKnownFolderName parentFolderName, SearchFilter searchFilter, FolderView view);
        Task<FindFoldersResults> FindFoldersAsync(WellKnownFolderName parentFolderName, SearchFilter searchFilter, FolderView view);

        //
        // Summary:
        //     Obtains a list of folders by searching the sub-folders of the specified folder.
        //
        // Parameters:
        //   parentFolderId:
        //     The Id of the folder in which to search for folders.
        //
        //   searchFilter:
        //     The search filter. Available search filter classes include SearchFilter.IsEqualTo,
        //     SearchFilter.ContainsSubstring and SearchFilter.SearchFilterCollection
        //
        //   view:
        //     The view controlling the number of folders returned.
        //
        // Returns:
        //     An object representing the results of the search operation.
        FindFoldersResults FindFolders(FolderId parentFolderId, SearchFilter searchFilter, FolderView view);
        Task<FindFoldersResults> FindFoldersAsync(FolderId parentFolderId, SearchFilter searchFilter, FolderView view);

        //
        // Summary:
        //     Obtains a list of items by searching the contents of a specific folder. Calling
        //     this method results in a call to EWS.
        //
        // Parameters:
        //   parentFolderName:
        //     The name of the folder in which to search for items.
        //
        //   view:
        //     The view controlling the number of items returned.
        //
        // Returns:
        //     An object representing the results of the search operation.
        FindItemsResults<Item> FindItems(WellKnownFolderName parentFolderName, ViewBase view);
        Task<FindItemsResults<Item>> FindItemsAsync(WellKnownFolderName parentFolderName, ViewBase view);
        //
        // Summary:
        //     Obtains a list of items by searching the contents of a specific folder. Calling
        //     this method results in a call to EWS.
        //
        // Parameters:
        //   parentFolderId:
        //     The Id of the folder in which to search for items.
        //
        //   view:
        //     The view controlling the number of items returned.
        //
        // Returns:
        //     An object representing the results of the search operation.
        FindItemsResults<Item> FindItems(FolderId parentFolderId, ViewBase view);
        Task<FindItemsResults<Item>> FindItemsAsync(FolderId parentFolderId, ViewBase view);

        //
        // Summary:
        //     Obtains a list of items by searching the contents of a specific folder. Calling
        //     this method results in a call to EWS.
        //
        // Parameters:
        //   parentFolderId:
        //     The Id of the folder in which to search for items.
        //
        //   searchFilter:
        //     The search filter. Available search filter classes include SearchFilter.IsEqualTo,
        //     SearchFilter.ContainsSubstring and SearchFilter.SearchFilterCollection
        //
        //   view:
        //     The view controlling the number of items returned.
        //
        // Returns:
        //     An object representing the results of the search operation.
        FindItemsResults<Item> FindItems(FolderId parentFolderId, SearchFilter searchFilter, ViewBase view);
        Task<FindItemsResults<Item>> FindItemsAsync(FolderId parentFolderId, SearchFilter searchFilter, ViewBase view);
        //
        // Summary:
        //     Obtains a list of items by searching the contents of a specific folder. Calling
        //     this method results in a call to EWS.
        //
        // Parameters:
        //   parentFolderName:
        //     The name of the folder in which to search for items.
        //
        //   searchFilter:
        //     The search filter. Available search filter classes include SearchFilter.IsEqualTo,
        //     SearchFilter.ContainsSubstring and SearchFilter.SearchFilterCollection
        //
        //   view:
        //     The view controlling the number of items returned.
        //
        // Returns:
        //     An object representing the results of the search operation.
        FindItemsResults<Item> FindItems(WellKnownFolderName parentFolderName, SearchFilter searchFilter, ViewBase view);
        Task<FindItemsResults<Item>> FindItemsAsync(WellKnownFolderName parentFolderName, SearchFilter searchFilter, ViewBase view);
        //
        // Summary:
        //     Obtains a list of items by searching the contents of a specific folder. Calling
        //     this method results in a call to EWS.
        //
        // Parameters:
        //   parentFolderName:
        //     The name of the folder in which to search for items.
        //
        //   queryString:
        //     query string to be used for indexed search
        //
        //   view:
        //     The view controlling the number of items returned.
        //
        // Returns:
        //     An object representing the results of the search operation.
        FindItemsResults<Item> FindItems(WellKnownFolderName parentFolderName, string queryString, ViewBase view);
        Task<FindItemsResults<Item>> FindItemsAsync(WellKnownFolderName parentFolderName, string queryString, ViewBase view);
        //
        // Summary:
        //     Obtains a list of items by searching the contents of a specific folder. Calling
        //     this method results in a call to EWS.
        //
        // Parameters:
        //   parentFolderId:
        //     The Id of the folder in which to search for items.
        //
        //   queryString:
        //     the search string to be used for indexed search, if any.
        //
        //   view:
        //     The view controlling the number of items returned.
        //
        // Returns:
        //     An object representing the results of the search operation.
        FindItemsResults<Item> FindItems(FolderId parentFolderId, string queryString, ViewBase view);
        Task<FindItemsResults<Item>> FindItemsAsync(FolderId parentFolderId, string queryString, ViewBase view);
        void FolderDelete(Folder folder, DeleteMode deleteMode);
        System.Threading.Tasks.Task FolderDeleteAsync(Folder folder, DeleteMode deleteMode);

        void FolderEmpty(Folder folder, DeleteMode deleteMode, bool deleteSubFolders);
        System.Threading.Tasks.Task FolderEmptyAsync(Folder folder, DeleteMode deleteMode, bool deleteSubFolders);

        void FolderCreate(string folderName, string folderType, Folder parentFolder);
    }
}
