using EwsDataInterface;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SqlDbImpl.Model
{
    [Table("FolderInformation")]
    public class FolderModel : IFolderData, ICatalogInfo
    {
        [Key]
        [Column(Order = 2)]
        public DateTime StartTime
        {
            get; set;
        }

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
        [Column(Order = 1)]
        [MaxLength(512)]
        public string FolderId
        {
            get; set;
        }
        
        [MaxLength(64)]
        public string FolderType
        {
            get; set;
        }

        [NotMapped]
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
        public string ParentFolderId
        {
            get; set;
        }

        [Required]
        public int ChildFolderCount
        {
            get; set;
        }
    }
}