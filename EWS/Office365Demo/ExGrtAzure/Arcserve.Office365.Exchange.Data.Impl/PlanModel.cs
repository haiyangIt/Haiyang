using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Arcserve.Office365.Exchange.Data.Plan;
using Arcserve.Office365.Exchange.Data;
using Microsoft.WindowsAzure.Scheduler.Models;

namespace Arcserve.Office365.Exchange.Data.Impl.Mail
{
    [Table("PlanBaseInfo")]
    public class PlanModel : IPlanData
    {
        [Key]
        [CaseSensitive("Name")]
        [MaxLength(256)]
        public string Name { get; set; }
        [MaxLength(128)]
        public string Organization { get; set; }
        [MaxLength(512)]
        public string CredentialInfo { get; set; }
        public string PlanMailInfos { get; set; }

        public DateTime FirstStartTime
        {
            get; set;
        }

        public DateTime NextFullBackupTime
        {
            get; set;
        }

        public string LastSyncStaus { get; set; }
    }

    [Table("PlanMailInfo")]
    public class PlanMailInfo : IPlanMailInfo
    {
        [Key]
        [CaseSensitive("Name")]
        [MaxLength(256)]
        public string Name { get; set; }
        [MaxLength(256)]
        public string Mailbox { get; set; }
        public string FolderInfos { get; set; }

        public static string GetString(List<IPlanMailInfo> mailInfos)
        {
            throw new NotImplementedException();
        }
    }

    [Table("PlanAzureSchedulerInfo")]
    public class PlanAzureInfo : IPlanAzureInfo
    {
        [Key]
        [CaseSensitive("Name")]
        [MaxLength(256)]
        public string Name { get; set; }
        [MaxLength(256)]
        public string CloudService { get; set; }
        [MaxLength(256)]
        public string JobCollectionName { get; set; }
        [MaxLength(1024)]
        public string JobInfo { get; set; }
        public Job Job { get; set; }
    }
}
