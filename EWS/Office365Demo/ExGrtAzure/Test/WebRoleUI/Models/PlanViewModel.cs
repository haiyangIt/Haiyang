using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Scheduler.Models;
using Arcserve.Office365.Exchange.Data.Plan;
using Arcserve.Office365.Exchange.Data.Account;

namespace WebRoleUI.Models
{
    public class PlanViewModel : IPlanData
    {
        public OrganizationAdminInfo AdminInfo { get; set; }
        public SyncData SyncDatas { get; set; }

        public string CredentialInfo
        {
            get
            {
                return JsonConvert.SerializeObject(AdminInfo);
            }
        }

        public DateTime FirstStartTime
        {
            get; set;
        }

        public string LastSyncStaus
        {
            get; set;
        }

        public string Name
        {
            get; set;
        }

        public DateTime NextFullBackupTime
        {
            get; set;
        }

        public string Organization
        {
            get; set;
        }

        public string PlanMailInfos
        {
            get; set;
        }
        
        public DateTime SchedulerStartTime { get; set; }

        public int SchedulerRecInterval { get; set; }

        public JobRecurrenceFrequency SchedulerFrequency { get; set; }

        public string MailSettingInfo { get; set; }

        public RecType RecType { get; set; }
    }
    public enum RecType : byte
    {
        Once = 0,
        Recurring
    }
    public class SyncData : List<IEwsSyncStatusItem>, IEwsSyncStatus
    {

    }
}