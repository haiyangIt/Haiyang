using EwsFrame.EF;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDbImpl.Model
{
    public class PlanModel
    {
        [Key]
        [CaseSensitive("Name")]
        [MaxLength(256)]
        public string Name { get; set; }
        [MaxLength(128)]
        public string Organization { get; set; }
        [MaxLength(512)]
        public string CredentialInfo { get; set; }
        [MaxLength(512)]
        public string ScheduleInfo { get; set; }
    }

    public class PlanMailInfo
    {
        [Key]
        [CaseSensitive("Name")]
        [MaxLength(256)]
        public string Name { get; set; }
        [MaxLength(256)]
        public string Mailbox { get; set; }
        public string FolderInfos { get; set; }
    }

    public class PlanAzureInfo
    {
        [Key]
        [CaseSensitive("Name")]
        [MaxLength(256)]
        public string Name { get; set; }
        [MaxLength(256)]
        public string CloudService { get; set; }
        [MaxLength(256)]
        public string JobCollectionName { get; set; }
    }
}
