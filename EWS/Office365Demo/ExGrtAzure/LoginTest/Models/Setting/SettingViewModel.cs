using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LoginTest.Models.Setting
{
    public class SettingViewModel
    {
        [Display(AutoGenerateField = false)]
        public string UserMail { get; set; }

        [Required]
        [Display(Name = "Admin User")]
        [EmailAddress]
        public string AdminUserName { get; set; }

        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string AdminPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "Exchange Connect Url")]
        [Url]
        [Editable(false)]
        public string EwsConnectUrl { get; set; }

        public bool IsExist { get; set; }

        public string EncryptPassword { get; set; }
    }
}