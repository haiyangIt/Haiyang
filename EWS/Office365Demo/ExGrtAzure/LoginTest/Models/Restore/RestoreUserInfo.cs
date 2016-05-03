using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LoginTest.Models.Restore
{
    public class RestoreUserInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Organization { get; set; }
        public string AdminPassword { get; set; }
        public string AdminUserName { get; set; }
        public bool IsExistSetting { get; set; }
    }
}