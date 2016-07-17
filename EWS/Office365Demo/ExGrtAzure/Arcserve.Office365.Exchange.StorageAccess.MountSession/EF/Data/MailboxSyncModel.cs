using Arcserve.Office365.Exchange.Data.Increment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arcserve.Office365.Exchange.Data;
using Arcserve.Office365.Exchange.Data.Mail;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Arcserve.Office365.Exchange.StorageAccess.MountSession.EF.Data
{
    [Table("MailboxSync")]
    public class MailboxSyncModel : IMailboxDataSync
    {
        public MailboxSyncModel()
        {

        }
        [Key]
        public Int64 UniqueId { get; set; }

        public string FolderTree { get; set; }

        [NotMapped]
        public string ChangeKey
        {
            get; set;
        }
        
        [NotMapped]
        public int ChildFolderCount
        {
            get; set;
        }

        [MaxLength(255)]
        public string DisplayName
        {
            get; set;
        }

        [Index]
        public string Id
        {
            get; set;
        }

        [NotMapped]
        public ItemKind ItemKind
        {
            get
            {
                return ItemKind.Mailbox;
            }
            set { }
        }

        [NotMapped]
        public string Location
        {
            get
            {
                return MailAddress;
            }
            set { }
        }

        private string _mailAddress;
        [MaxLength(255)]
        [EmailAddress]
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
        
        [MaxLength(512)]
        [CaseSensitive]
        public string RootFolderId
        {
            get; set;
        }

        /// <summary>
        /// FolderHierarchy status. 
        /// </summary>
        [CaseSensitive]
        public string SyncStatus
        {
            get; set;
        }

        public IMailboxData Clone()
        {
            throw new NotImplementedException();
        }

        public void Clone(IMailboxDataSync source)
        {
            DisplayName = source.DisplayName;
            ChangeKey = source.ChangeKey;
            this.ChildFolderCount = source.ChildFolderCount;
            this.MailAddress = source.MailAddress;
            this.RootFolderId = source.RootFolderId;
            this.SyncStatus = source.SyncStatus;
            this.Name = source.Name;
            this.FolderTree = source.FolderTree;
        }
    }

    //[Table("MailboxSyncUpdate")]
    //public class UpdateMailboxSyncModel : MailboxSyncModel { }

    //[Table("MailboxSyncDelete")]
    //public class DeleteMailboxSync
    //{
    //    [Key]
    //    public string Id
    //    {
    //        get; set;
    //    }

    //    [MaxLength(255)]
    //    [EmailAddress]
    //    public string MailAddress
    //    {
    //        get; set;
    //    }
    //}
}
