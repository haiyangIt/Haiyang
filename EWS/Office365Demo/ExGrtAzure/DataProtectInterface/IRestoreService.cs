using DataProtectInterface.Event;
using DataProtectInterface.Util;
using EwsDataInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProtectInterface
{
    public interface IRestoreService : IRestoreEvent
    {
        DateTime StartTime { get; }

        string RestoreJobName { get; set; }

        OrganizationAdminInfo AdminInfo { get; set; }

        IServiceContext ServiceContext { get; }

        ICatalogJob CurrentRestoreCatalogJob { get; set; }

        IRestoreDestination Destination { get; set; }

        void RestoreOrganization();
        void RestoreMailboxes(List<string> mailboxes);
        void RestoreMailbox(string mailbox);
        void RestoreFolders(string mailbox, List<string> folderIds, bool isRecursion = false);
        void RestoreFolder(string mailbox, string folderId, bool isRecursion = false);
        void RestoreItems(string mailbox, List<IRestoreItemInformation> items);
        void RestoreItem(string mailbox, IRestoreItemInformation item);
        void RestoreItem(string mailbox, string itemId, string displayName);
    }

    public interface IRestoreItemInformation
    {
        List<IFolderDataBase> FolderPathes { get; } 
        string ItemId { get; }
        string DisplayName { get; }

        ItemClass ItemClass { get; }
    }

    public interface IRestoreServiceEx
    {
        void Restore(LoadedTreeItem items);
        
        IRestoreDestinationEx Destination { get; set; }
        ICatalogJob CurrentRestoreCatalogJob { get; set; }
        OrganizationAdminInfo AdminInfo { get; set; }
        string RestoreJobName { get; set; }
        DateTime StartTime { get; }
        IServiceContext ServiceContext { get; }
    }
    
}
