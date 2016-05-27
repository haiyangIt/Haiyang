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
        public MailboxDataSyncBase() { }
        public MailboxDataSyncBase(string displayName, string mailboxAddress)
        {
            DisplayName = displayName;
            MailAddress = mailboxAddress.ToLower();
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

        private string _mailAddress;
        public string MailAddress
        {
            get
            {
                return _mailAddress;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    _mailAddress = string.Empty;
                else
                    _mailAddress = value.ToLower();
            }
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
