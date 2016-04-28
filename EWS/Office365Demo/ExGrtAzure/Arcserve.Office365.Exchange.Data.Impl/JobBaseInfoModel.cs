using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Arcserve.Office365.Exchange.ArcJob;

namespace Arcserve.Office365.Exchange.Data.Impl
{
    [Table("JobBaseInfo")]
    public class JobBaseInfoModel : IJobBaseInfo
    {
        [Key]
        [CaseSensitive("JobId")]
        [MaxLength(64)]
        public string JobId
        {
            get; set;
        }
        
        public DateTime CreateTime
        {
            get; set;
        }

        public string JobName
        {
            get; set;
        }

        [CaseSensitive("Name")]
        [MaxLength(256)]
        public string PlanName
        {
            get; set;
        }

        public TaskType TaskType
        {
            get; set;
        }
    }
}
