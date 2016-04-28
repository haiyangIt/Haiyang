using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Util.Setting
{
    public class AssemblyConfig
    {
        public static AssemblyConfig Instance = new AssemblyConfig();

        public virtual string BinaryStorageAccessInAzure
        {
            get
            {
                return "Arcserve.Office365.Exchange.StorageAccess.Azure.dll";
            }
        }

        public virtual string BinaryExchangeEwsApi
        {
            get
            {
                return "Arcserve.Office365.Exchange.EwsApi.Impl.dll";
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
                return "Arcserve.Office365.Exchange.Data.Impl";
            }
        }

        public virtual string ICatalogDataAccessImplClass
        {
            get
            {
                return "Arcserve.Office365.Exchange.StorageAccess.Azure.CatalogDataAccess";
            }
        }

        public virtual string IDataConvertImplClass
        {
            get
            {
                return "Arcserve.Office365.Exchange.StorageAccess.Azure.DataConvert";
            }
        }

        public virtual string IMailboxImplClass
        {
            get
            {
                return "Arcserve.Office365.Exchange.EwsApi.Impl.Impl.MailboxOperatorImpl";
            }
        }

        public virtual string IFolderImplClass
        {
            get
            {
                return "Arcserve.Office365.Exchange.EwsApi.Impl.Impl.Impl.FolderOperatorImpl";
            }
        }

        public virtual string IItemImplClass
        {
            get
            {
                return "Arcserve.Office365.Exchange.EwsApi.Impl.Impl.Impl.ItemOperatorImpl";
            }
        }

        public virtual string ICatalogServiceImplClass
        {
            get
            {
                return "Arcserve.Office365.Exchange.DataProtect.Impl.CatalogService";
            }
        }

        public virtual string IFilterItemImplClass
        {
            get
            {
                return "Arcserve.Office365.Exchange.DataProtect.Impl.FilterBySelectedTree";
            }
        }

        public virtual string IQueryCatalogDataAccessImplClass
        {
            get
            {
                return "Arcserve.Office365.Exchange.StorageAccess.Azure.QueryCatalogDataAccess";
            }
        }

        public virtual string IDataConvertFromDbImplClass
        {
            get
            {
                return "Arcserve.Office365.Exchange.StorageAccess.Azure.DataConvertFromDb";
            }
        }

        public virtual string ICatalogJobImplClass
        {
            get
            {
                return "Arcserve.Office365.Exchange.Data.Impl.CatalogInfoModel";
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
