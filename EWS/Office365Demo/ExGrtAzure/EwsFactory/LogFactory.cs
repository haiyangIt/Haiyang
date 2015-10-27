using LogInterface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;

namespace EwsFrame
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
                    var logImplAssemblyPath = Path.Combine(LibPath, "LogImpl.dll");
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
            _logInstance = (ILog)(CreateTypeWithName<ILog>(LogImplAssembly, "LogImpl.DefaultLog"));
            _ewsTraceLogInstance = (ILog)(CreateTypeWithName<ILog>(LogImplAssembly, "LogImpl.DefaultEwsTraceLog"));
        }
    }
}