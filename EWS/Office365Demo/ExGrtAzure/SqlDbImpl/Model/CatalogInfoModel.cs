using EwsDataInterface;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SqlDbImpl.Model
{
    [Table("CatalogInformation")]
    public class CatalogInfoModel : ICatalogJob, ICatalogInfo
    {
        [MaxLength(255)]
        [Required]
        public string CatalogJobName
        {
            get; set;
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