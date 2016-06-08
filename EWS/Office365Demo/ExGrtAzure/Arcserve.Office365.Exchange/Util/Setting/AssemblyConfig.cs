using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Util.Setting
{
    public class AssemblyConfig
    {
        static AssemblyConfig()
        {
            Instance = new AssemblyConfig();
        }
        public static readonly AssemblyConfig Instance;
        

        public virtual string BinaryExchangeEwsApi
        {
            get
            {
                return "Arcserve.Office365.Exchange.EwsApi.dll";
            }
        }

        public virtual string BinaryDataProtectImplement
        {
            get
            {
                return "Arcserve.Office365.Exchange.DataProtect.Impl.dll";
            }
        }

        public virtual string BinaryDataImplement
        {
            get
            {
                return "Arcserve.Office365.Exchange.Data.dll";
            }
        }
        
        

        public virtual string IRestoreDestinationImplClass
        {
            get
            {
                return "Arcserve.Office365.Exchange.EwsApi.Impl.Impl.RestoreDestinationImpl";
            }
        }

        public virtual string IRestoreDestinationExImplClass
        {
            get
            {
                return "Arcserve.Office365.Exchange.EwsApi.Impl.Impl.RestoreDestinationExImpl";
            }
        }

        public virtual string IRestoreServiceImplClass
        {
            get
            {
                return "Arcserve.Office365.Exchange.DataProtect.Impl.RestoreService";
            }
        }

        public virtual string IRestoreServiceExImplClass
        {
            get
            {
                return "Arcserve.Office365.Exchange.DataProtect.Impl.RestoreServiceEx";
            }
        }

        public virtual string IPlanDataAccessImplClass
        {
            get
            {
                return "Arcserve.Office365.Exchange.StorageAccess.Azure.PlanDataAccess";
            }
        }

        public virtual string IRestoreDestionationToOrgClass
        {
            get
            {
                return "Arcserve.Office365.Exchange.EwsApi.Impl.Impl.RestoreDestinationOrgExImpl";
            }
        }

        public virtual string IRestoreToAzureImplClass
        {
            get
            {
                return "Arcserve.Office365.Exchange.StorageAccess.Azure.RestoreToAzure";
            }
        }
        
    }
}
