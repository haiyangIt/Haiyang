using DataProtectInterface;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LoginTest.Models
{
    public class BackupModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Backup Admin Account")]
        public string BackupUserMailAddress { get; set; }

        [Display(Name = "Backup Admin Password")]
        [DataType(DataType.Password)]
        public string BackupUserPassword { get; set; }

        [Required]
        [Display(Name = "Backup User Organization")]
        public string BackupUserOrganization { get; set; }

        [Required]
        public int Index { get; set; }

        public string BackupSelectItems { get; set; }

        //[Required]
        public string BackupJobName { get; set; }
        public bool IsAdminUseExist { get; set; }
        public string EncryptPassword { get; set; }
    }

    public class ProgressModel
    {
        [Display(Name = "Mailbox Current/Total")]
        public string MailboxPercent { get; set; }
        [Display(Name = "Current Mailbox")]
        public string CurrentMailbox { get; set; }
        [Display(Name = "Folder Current/Total")]
        public string FolderPercent { get; set; }
        [Display(Name = "Current Folder")]
        public string CurrentFolder { get; set; }
        [Display(Name = "Item Current/Total")]
        public string ItemPercent { get; set; }
        [Display(Name = "Current Item")]
        public string CurrentItem { get; set; }
        [Display(Name = "Latest Information")]
        public string LatestInfo { get; set; }

        public Guid ServiceId { get; set; }

        [Display(Name ="Start Time")]
        public string StartTime { get; set; }
        [Display(Name = "End Time")]
        public string EndTime { get; set; }

        public ProgressModel() { }
        public ProgressModel(IDataProtectProgress progress, Guid serviceId)
        {
            ServiceId = serviceId;
            LatestInfo = string.Join("\r\n", progress.LatestInformation);
            MailboxPercent = progress.MailboxPercent;
            CurrentMailbox = progress.CurrentMailbox;
            CurrentFolder = progress.CurrentFolder;
            FolderPercent = progress.FolderPercent;
            CurrentItem = progress.CurrentItem;
            ItemPercent = progress.ItemPercent;
            StartTime = progress.StartTime == DateTime.MinValue ? "" : progress.StartTime.ToString("yyyy-MM-dd HH:mm:ss");
            EndTime = progress.EndTime == DateTime.MinValue ? "" : progress.EndTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public ProgressModel(Guid serviceId)
        {
            ServiceId = serviceId;
        }
    }
}