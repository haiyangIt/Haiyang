using EwsDataInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProtectInterface
{
    public interface IRestoreService
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
        void RestoreItem(string mailbox, string itemId);
    }

    public interface IRestoreItemInformation
    {
        List<string> FolderPathes { get; } 
        string ItemId { get; }
        string MailAddress { get; }
    }
}
