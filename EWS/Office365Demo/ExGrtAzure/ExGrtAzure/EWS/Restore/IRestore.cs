using ExGrtAzure.EWS.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExGrtAzure.EWS.Restore
{
    public interface IRestore
    {
        string Organization { get; set; }
        void RestoreItem(IItemData itemData, RestoreLocation destination);
        void RestoreFolder(IFolderData folderData, RestoreLocation destination);
        void RestoreMailbox(IMailboxData mailboxData, RestoreLocation destination);
    }
}
