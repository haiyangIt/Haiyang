using Arcserve.Office365.Exchange.Data.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Data.Increment
{
    public interface IMailboxDataSync : IMailboxData, IDataSync
    {
        string Name { get; set; }
    }

    public class MailboxDataSyncBase : IMailboxDataSync
    {
        public MailboxDataSyncBase(string displayName, string mailboxAddress)
        {
            DisplayName = displayName;
            MailAddress = mailboxAddress;
        }

        public string ChangeKey
        {
            get; set;
        }

        public int ChildFolderCount
        {
            get; set;
        }

        public string DisplayName
        {
            get; set;
        }

        public string Id
        {
            get; set;
        }

        public ItemKind ItemKind
        {
            get
            {
                return ItemKind.Mailbox;
            }

            set
            {
            }
        }

        public string Location
        {
            get; set;
        }

        public string MailAddress
        {
            get; private set;
        }

        public string Name
        {
            get; set;
        }

        public string RootFolderId
        {
            get; set;
        }

        public string SyncStatus
        {
            get; set;
        }

        public IMailboxDataSync Clone()
        {
            return new MailboxDataSyncBase(DisplayName, MailAddress)
            {
                Id = Id,
                Location = Location,
                RootFolderId = RootFolderId,
                Name = Name,
                SyncStatus = SyncStatus
            };
        }

        IMailboxData IMailboxData.Clone()
        {
            return Clone();
        }
    }
}
