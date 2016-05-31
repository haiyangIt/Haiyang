using Arcserve.Office365.Exchange.Data.Increment;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arcserve.Office365.Exchange.Data;
using Arcserve.Office365.Exchange.Data.Mail;
using System.ComponentModel.DataAnnotations;
using Microsoft.Exchange.WebServices.Data;

namespace Arcserve.Office365.Exchange.StorageAccess.MountSession.EF.Data
{
    [Table("FolderSync")]
    public class FolderSyncModel : IFolderDataSync
    {
        [Required]
        public int ChildItemCount
        {
            get; set;
        }

        [MaxLength(255)]
        [Required]
        public string DisplayName
        {
            get; set;
        }

        [Key]
        [MaxLength(512)]
        [CaseSensitive("FolderId")]
        public string FolderId
        {
            get; set;
        }

        [MaxLength(64)]
        public string FolderType
        {
            get; set;
        }

        public string Location
        {
            get; set;
        }

        [EmailAddress]
        [MaxLength(255)]
        public string MailboxAddress
        {
            get; set;
        }

        [Required]
        [MaxLength(512)]
        [CaseSensitive]
        public string ParentFolderId
        {
            get; set;
        }

        [Required]
        public int ChildFolderCount
        {
            get; set;
        }

        [NotMapped]
        public string Id
        {
            get
            {
                return FolderId;
            }
            set { }
        }

        [NotMapped]
        public ItemKind ItemKind
        {
            get
            {
                return ItemKind.Folder;
            }
            set { }
        }

        [CaseSensitive]
        public string SyncStatus
        {
            get; set;
        }

        [CaseSensitive]
        public string ChangeKey
        {
            get; set;
        }

        public string MailboxId
        {
            get; set;
        }

        [NotMapped]
        public FolderId FolderIdInExchange
        {
            get; set;
        }

        public IFolderData Clone()
        {
            throw new NotImplementedException();
        }

        public void Clone(IFolderDataSync source)
        {
            this.ChangeKey = source.ChangeKey;
            this.ChildFolderCount = source.ChildFolderCount;
            this.ChildItemCount = source.ChildItemCount;
            this.DisplayName = ((IFolderDataBase)source).DisplayName;
            this.FolderType = source.FolderType;
            this.Location = source.Location;
            this.MailboxAddress = source.MailboxAddress;
            this.MailboxId = source.MailboxId;
            this.ParentFolderId = source.ParentFolderId;
            this.SyncStatus = source.SyncStatus;
        }
    }
    

}
