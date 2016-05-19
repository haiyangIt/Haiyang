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

        [NotMapped]
        public string ChangeKey
        {
            get; set;
        }

        public int ChildFolderCount
        {
            get; set;
        }
        
        [MaxLength(255)]
        public string DisplayName
        {
            get; set;
        }

        [Key]
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

        public string Location
        {
            get; set;
        }

        [MaxLength(255)]
        [EmailAddress]
        public string MailAddress
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
    }
}
