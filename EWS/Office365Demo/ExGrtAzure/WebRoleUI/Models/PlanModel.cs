using DataProtectInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebRoleUI.Models
{
    public class PlanModel
    {
        public string PlanName { get; set; }
        public ScheduleInfo Schedule { get; set; }
        public LoadedTreeItem SelectedItems { get; set; }
        public OrganizationAdminInfo UserAdminInfo { get; set; }
    }

    
}