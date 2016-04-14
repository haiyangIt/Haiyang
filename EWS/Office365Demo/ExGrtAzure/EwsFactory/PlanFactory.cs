using DataProtectInterface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EwsFrame
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
                    var sqlDbImplPath = Path.Combine(LibPath, "SqlDbImpl.dll");
                    var ewsServicePath = Path.Combine(LibPath, "EwsService.dll");
                    var dataProtectPath = Path.Combine(LibPath, "DataProtectImpl.dll");

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
                    _interfaceImplTypeNameDic.Add(typeof(IPlanDataAccess), "SqlDbImpl.PlanDataAccess");
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
