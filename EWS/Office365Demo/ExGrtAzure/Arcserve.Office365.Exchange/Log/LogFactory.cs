using Arcserve.Office365.Exchange.Util.Setting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Arcserve.Office365.Exchange.Log
{
    public class LogFactory : FactoryBase
    {
        private static LogFactory _instance = null;
        private static LogFactory Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new LogFactory();
                    string logImplAssemblyPath = Path.Combine(LibPath, "Arcserve.Office365.Exchange.Log.Impl.dll");
                    _instance.LogImplAssembly = Assembly.LoadFrom(logImplAssemblyPath);
                }
                return _instance;
            }
        }
        

        private ILog _logInstance;
        public static ILog LogInstance
        {
            get
            {
                if(Instance._logInstance == null)
                {
                    Instance.InitLog();
                }
                return Instance._logInstance;
            }
        }

        private ILog _ewsTraceLogInstance;
        public static ILog EwsTraceLogInstance
        {
            get
            {
                if(Instance._ewsTraceLogInstance == null)
                {
                    Instance.InitLog();
                }
                return Instance._ewsTraceLogInstance;
            }
        }
        
        protected override Dictionary<Type, string> InterfaceImplTypeNameDic
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        private void InitLog()
        {
            if (!CloudConfig.IsRunningOnAzure())
            {
                _logInstance = (ILog)(CreateTypeWithName<ILog>(LogImplAssembly, "Arcserve.Office365.Exchange.Log.Impl.DefaultLog"));
                _ewsTraceLogInstance = (ILog)(CreateTypeWithName<ILog>(LogImplAssembly, "Arcserve.Office365.Exchange.Log.Impl.DefaultEwsTraceLog"));
            }
            else
            {
                _logInstance = (ILog)(CreateTypeWithName<ILog>(LogImplAssembly, "Arcserve.Office365.Exchange.Log.Impl.LogToBlob"));
                _ewsTraceLogInstance = (ILog)(CreateTypeWithName<ILog>(LogImplAssembly, "Arcserve.Office365.Exchange.Log.Impl.LogToBlobEwsTrace"));
            }
        }
        
    }
}