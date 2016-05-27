using Arcserve.Office365.Exchange.StorageAccess.Interface;
using Arcserve.Office365.Exchange.Util.Setting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.DataProtect.Interface
{
    public class PlanFactory : FactoryBase
    {
        static PlanFactory()
        {
            Instance = new PlanFactory();
        }
        private PlanFactory()
        {
            var sqlDbImplPath = Path.Combine(LibPath, AssemblyConfig.Instance.BinaryStorageAccessInAzure);
            var ewsServicePath = Path.Combine(LibPath, AssemblyConfig.Instance.BinaryExchangeEwsApi);
            var dataProtectPath = Path.Combine(LibPath, AssemblyConfig.Instance.BinaryDataProtectImplement);

            EwsDataImplAssembly = Assembly.LoadFrom(sqlDbImplPath);
            EwsServiceImplAssembly = Assembly.LoadFrom(ewsServicePath);
            DataProtectImplAssembly = Assembly.LoadFrom(dataProtectPath);

            InterfaceImplTypeNameDic = new Dictionary<Type, string>();
            InterfaceImplTypeNameDic.Add(typeof(IPlanDataAccess), AssemblyConfig.Instance.IPlanDataAccessImplClass);
        }

        public static readonly PlanFactory Instance;

        protected override Dictionary<Type, string> InterfaceImplTypeNameDic
        {
            get; set;
        }
        public IPlanDataAccess NewPlanDataAccess(string organization)
        {
            return (IPlanDataAccess)CreateType<IPlanDataAccess>(EwsDataImplAssembly, organization);
        }
    }
}
