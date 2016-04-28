using Arcserve.Office365.Exchange.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Arcserve.Office365.Exchange.Data.Impl
{
    [Table("CatalogInformation")]
    public class CatalogInfoModel : ICatalogJob, ICatalogInfo
    {
        public CatalogInfoModel() { }

        public CatalogInfoModel(string organization, string catalogJobName, DateTime startTime)
        {
            CatalogJobName = catalogJobName;
            Organization = organization;
            StartTime = startTime;
        }

        [MaxLength(255)]
        [Required]
        public string CatalogJobName
        {
            get; set;
        }

        [NotMapped]
        public string DisplayName
        {
            get
            {
                return CatalogJobName;
            }
            set { }
        }

        [NotMapped]
        public string Id
        {
            get
            {
                return "0";
            }
            set { }
        }

        [NotMapped]
        public ItemKind ItemKind
        {
            get
            {
                return ItemKind.Organization;
            }
            set { }
        }

        [MaxLength(255)]
        [Required]
        public string Organization
        {
            get; set;
        }
        [Key]
        public DateTime StartTime
        {
            get; set;
        }
    }
}