using EwsDataInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LoginTest.Models.Restore
{
    [Serializable]
    public class CatalogJobParameter : ICatalogJob
    {
        public string CatalogJobName
        {
            get; set;
        }

        public string DisplayName
        {
            get
            {
                return CatalogJobName;
            }
        }

        public string Id
        {
            get
            {
                return "0";
            }
            
        }

        public ItemKind ItemKind
        {
            get
            {
                return ItemKind.Organization;
            }
        }

        public string Organization
        {
            get; set;
        }

        public DateTime StartTime
        {
            get; set;
        }
    }
}