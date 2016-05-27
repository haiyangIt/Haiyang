﻿using Arcserve.Office365.Exchange.Data;
using Arcserve.Office365.Exchange.Data.Increment;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arcserve.Office365.Exchange.Data.Mail;
using System.IO;
using Arcserve.Office365.Exchange.Util;

namespace Arcserve.Office365.Exchange.StorageAccess.MountSession.EF.Data
{
    [Table("ItemSync")]
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

    public static class IItemDataSyncExtension
    {
        private static string DirectorySeparatorChar = Path.DirectorySeparatorChar.ToString();
        public static string GetFileName(this IItemDataSync item, List<string> folderPath)
        {
            var itemName = MD5Utility.ConvertToMd5(item.DisplayName);
            itemName = string.Format("{0}_{1}.bin", item.CreateTime.Value.ToString("yyyyMMdd_HHmmss"), itemName);

            var parentFolderPath = string.Join(DirectorySeparatorChar, folderPath);
            if (parentFolderPath.Length > 180)
            {
                parentFolderPath = MD5Utility.ConvertToMd5(parentFolderPath);
            }

            var fileName = Path.Combine(parentFolderPath, itemName);

            return fileName;
        }

        public static string GetFilePath(this IItemDataSync item, string dataFolder)
        {
            var workFolder = Path.Combine(dataFolder, item.MailboxAddress);
            var file = Path.Combine(workFolder, item.Location);
            return file;
        }
    }
}