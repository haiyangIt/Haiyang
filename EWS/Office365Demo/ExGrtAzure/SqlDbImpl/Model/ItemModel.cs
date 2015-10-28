using EwsDataInterface;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SqlDbImpl.Model
{
    [Table("ItemInformation")]
    public class ItemModel : IItemData, ICatalogInfo
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
        public string ItemId { get; set; }
        [NotMapped]
        [MaxLength(512)]
        public string Location { get; set; }

        [Required]
        [MaxLength(512)]
        public string ParentFolderId { get; set; }
        [Key]
        [Column(Order = 2)]
        public DateTime StartTime { get; set; }

        [Required]
        public int Size { get; set; }


        [NotMapped]
        public int ActualSize { get; set; }

        public IItemData Clone()
        {
            return new ItemModel()
            {
                CreateTime = CreateTime,
                DisplayName = DisplayName,
                ItemClass = ItemClass,
                ItemId = ItemId,
                Location = Location,
                ParentFolderId = ParentFolderId,
                StartTime = StartTime,
                Size = Size,
                ActualSize = ActualSize
            };
        }
    }
}