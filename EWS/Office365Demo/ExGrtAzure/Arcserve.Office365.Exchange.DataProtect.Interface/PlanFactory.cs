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
        private static PlanFactory _instance;
        public static PlanFactory Instance {
            get
            {
                if (_instance == null)
                {
                    _instance = new PlanFactory();
                    var sqlDbImplPath = Path.Combine(LibPath, AssemblyConfig.Instance.BinaryStorageAccessInAzure);
                    var ewsServicePath = Path.Combine(LibPath, AssemblyConfig.Instance.BinaryExchangeEwsApi);
                    var dataProtectPath = Path.Combine(LibPath, AssemblyConfig.Instance.BinaryDataProtectImplement);

                    _instance.EwsDataImplAssembly = Assembly.LoadFrom(sqlDbImplPath);
                    _instance.EwsServiceImplAssembly = Assembly.LoadFrom(ewsServicePath);
                    _instance.DataProtectImplAssembly = Assembly.LoadFrom(dataProtectPath);
                }
                return _instance;
            }
        }

        private static Dictionary<Type, string> _interfaceImplTypeNameDic = null;
        protected override Dictionary<Type, string> InterfaceImplTypeNameDic
        {
            get
            {
                if (_interfaceImplTypeNameDic == null)
                {
                    _interfaceImplTypeNameDic = new Dictionary<Type, string>();
                    _interfaceImplTypeNameDic.Add(typeof(IPlanDataAccess), AssemblyConfig.Instance.IPlanDataAccessImplClass);
                }
                return _interfaceImplTypeNameDic;
            }
        }
        public IPlanDataAccess NewPlanDataAccess(string organization)
        {
            return (IPlanDataAccess)CreateType<IPlanDataAccess>(EwsDataImplAssembly, organization);
        }
    }
}
