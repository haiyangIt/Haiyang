using Arcserve.Office365.Exchange.Manager;
using Arcserve.Office365.Exchange.Util;
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
        static LogFactory()
        {
            Instance = CreateFactory();
            LogInstance = Instance._logInstance;
            EwsTraceLogInstance = Instance._ewsTraceLogInstance;

            DisposeManager.RegisterInstance(LogInstance);
            DisposeManager.RegisterInstance(EwsTraceLogInstance);
            IsInited = true;
        }
        private static readonly LogFactory Instance;
        public static readonly bool IsInited = false;

        private static LogFactory CreateFactory()
        {
            var result = new LogFactory();
            string logImplAssemblyPath = Path.Combine(LibPath, "Arcserve.Office365.Exchange.dll");
            result.LogImplAssembly = Assembly.LoadFrom(logImplAssemblyPath);

            result._logInstance = result.CreateLogInstance();
            result._ewsTraceLogInstance = result.CreateEWSLogInstance();
            return result;
        }

        private ILog _logInstance;
        public readonly static ILog LogInstance;

        private ILog _ewsTraceLogInstance;
        public readonly static ILog EwsTraceLogInstance;

        protected override Dictionary<Type, string> InterfaceImplTypeNameDic
        {
            get
            {
                throw new NotSupportedException();
            }
            set { }
        }


        private ILog CreateLogInstance()
        {
            ILog result;
            if (!IsRunningOnAzureOrStorageInAzure())
            {
                result = (ILog)(CreateTypeWithName<ILog>(LogImplAssembly, "Arcserve.Office365.Exchange.Log.Impl.DefaultLog"));
            }
            else
            {
                result = (ILog)(CreateTypeWithName<ILog>(LogImplAssembly, "Arcserve.Office365.Exchange.Log.Impl.LogToBlob"));
            }
            return result;
        }

        

        private ILog CreateEWSLogInstance()
        {
            ILog result;
            if (!IsRunningOnAzureOrStorageInAzure())
            {
                result = (ILog)(CreateTypeWithName<ILog>(LogImplAssembly, "Arcserve.Office365.Exchange.Log.Impl.DefaultEwsTraceLog"));
            }
            else
            {
                result = (ILog)(CreateTypeWithName<ILog>(LogImplAssembly, "Arcserve.Office365.Exchange.Log.Impl.LogToBlobEwsTrace"));
            }

            return result;
        }

        protected string EwsLogFileName
        {
            get
            {
                return string.Format("{0}EwsTrace.txt", DateTime.Now.ToString("yyyyMMdd"));
            }
        }

    }
}