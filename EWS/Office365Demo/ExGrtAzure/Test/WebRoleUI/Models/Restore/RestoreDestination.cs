using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebRoleUI.Models.Restore
{
    public class RestoreDestination
    {
        public string MailboxAddress { get; set; }
        /// <summary>
        /// split with \
        /// </summary>
        public string FolderPath { get; set; }
    }
}