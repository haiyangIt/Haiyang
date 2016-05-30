using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LoginTest.Models.Setting
{
    public class SettingModel
    {
        [Key]
        [EmailAddress]
        [MaxLength(255)]
        public string UserMail { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string AdminUserName { get; set; }

        [Required]
        [MaxLength(64)]
        [DataType(DataType.Password)]
        public string AdminPassword { get; set; }

        [Required]
        [MaxLength(512)]
        [Url]
        public string EwsConnectUrl { get; set; }
    }
}