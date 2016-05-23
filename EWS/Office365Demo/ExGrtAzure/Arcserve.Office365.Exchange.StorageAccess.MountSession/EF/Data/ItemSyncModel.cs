using Arcserve.Office365.Exchange.Data;
using Arcserve.Office365.Exchange.Data.Increment;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arcserve.Office365.Exchange.Data.Mail;

namespace Arcserve.Office365.Exchange.StorageAccess.MountSession.EF.Data
{
    [Table("TableSync")]
    public class ItemSyncModel : IItemDataSync
    {
        [Required]
        public DateTime? CreateTime { get; set; }

        [NotMapped]
        public object Data { get; set; }

        public string DisplayName { get; set; }

        [Required]
        [MaxLength(64)]
        public string ItemClass { get; set; }

        [Key]
        [Column(Order = 1)]
        [MaxLength(512)]
        [CaseSensitive("ItemId")]
        public string ItemId { get; set; }
        
        [MaxLength(512)]
        public string Location { get; set; }

        [Required]
        [MaxLength(512)]
        [CaseSensitive]
        public string ParentFolderId { get; set; }

        [Required]
        public int Size { get; set; }


        [NotMapped]
        public int ActualSize { get; set; }

        [NotMapped]
        public string Id
        {
            get
            {
                return ItemId;
            }
            set { }
        }

        [NotMapped]
        public ItemKind ItemKind
        {
            get
            {
                return ItemKind.Item;
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

        public bool? IsRead
        {
            get; set;
        }

        public string MailboxAddress
        {
            get; set;
        }

        public IItemData Clone()
        {
            throw new NotImplementedException();
        }
    }
}
