using EwsFrame.Util;
using LogInterface;
using Microsoft.WindowsAzure.ServiceRuntime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;

namespace EwsFrame
{
    public class LogFactory : FactoryBase
    {

        private static object _lock = new object();
        private static LogFactory _instance = null;
        private static LogFactory Instance
        {
            get
            {
                if (_instance == null)
                {
                    using (_lock.LockWhile(() =>

                          {
                              if (_instance == null)
                              {
                                  _instance = CreateFactory();
                              }
                          }))
                    { }
                }
                return _instance;
            }
        }

        private static LogFactory CreateFactory()
        {
            var result = new LogFactory();
            string logImplAssemblyPath = Path.Combine(LibPath, "LogImpl.dll");
            result.LogImplAssembly = Assembly.LoadFrom(logImplAssemblyPath);

            result._logInstance = result.CreateLogInstance();
            result._ewsTraceLogInstance = result.CreateEWSLogInstance();
            return result;
        }


        private ILog _logInstance;
        public static ILog LogInstance
        {
            get
            {
                return Instance._logInstance;
            }
        }

        private ILog _ewsTraceLogInstance;
        public static ILog EwsTraceLogInstance
        {
            get
            {
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


        private ILog CreateLogInstance()
        {
            ILog result;
            if (!IsRunningOnAzureOrStorageInAzure())
            {
                result = (ILog)(CreateTypeWithName<ILog>(LogImplAssembly, "LogImpl.DefaultLog"));
            }
            else
            {
                result = (ILog)(CreateTypeWithName<ILog>(LogImplAssembly, "LogImpl.LogToBlob"));
            }
            return result;
        }

        private ILog CreateEWSLogInstance()
        {
            ILog result;
            if (!IsRunningOnAzureOrStorageInAzure())
            {
                result = (ILog)(CreateTypeWithName<ILog>(LogImplAssembly, "LogImpl.DefaultEwsTraceLog"));
            }
            else
            {
                result = (ILog)(CreateTypeWithName<ILog>(LogImplAssembly, "LogImpl.LogToBlobEwsTrace"));
            }

            return result;
        }
    }
}