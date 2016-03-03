using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebRoleUI.Models.Setting
{
    public class SettingViewModel
    {
        [Display(AutoGenerateField = false)]
        public string UserMail { get; set; }

        [Required]
        [Display(Name = "Admin User")]
        [EmailAddress]
        public string AdminUserName { get; set; }

        [Required]
        [Display(Name = "Password")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string AdminPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("AdminPassword", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "Exchange Connect Url")]
        [Url]
        [Editable(false)]
        public string EwsConnectUrl { get; set; }

        public bool IsExist { get; set; }
    }
}