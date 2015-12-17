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
        [Display(Name = "Backup User Mail Address")]
        public string BackupUserMailAddress { get; set; }

        [Required]
        [Display(Name = "Backup User Password")]
        [DataType(DataType.Password)]
        public string BackupUserPassword { get; set; }

        [Required]
        [Display(Name = "Backup User Organization")]
        public string BackupUserOrganization { get; set; }

        [Required]
        public int Index { get; set; }

        public string BackupSelectItems { get; set; }
    }
}