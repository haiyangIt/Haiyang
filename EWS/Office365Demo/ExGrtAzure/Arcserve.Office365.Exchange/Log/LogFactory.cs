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
                result = (ILog)(CreateTypeWithName<ILog>(LogImplAssembly, "Arcserve.Office365.Exchange.Log.Impl.DefaultLog"));
                result.RegisterLogStream(new LogToStreamManage(LogFolder, LogFileNameFormat, LogMaxRecordCount));
            }
            else
            {
                result = (ILog)(CreateTypeWithName<ILog>(LogImplAssembly, "Arcserve.Office365.Exchange.Log.Impl.LogToBlob"));
            }
            return result;
        }

        private int LogMaxRecordCount
        {
            get
            {
                return CloudConfig.Instance.LogFileMaxRecordCount;
            }
        }


        private string LogFileNameFormat
        {
            get
            {
                return "{0}_{1}.txt";
            }
        }


        private string _logFolder;
        private string LogFolder
        {
            get
            {
                if (string.IsNullOrEmpty(_logFolder))
                {
                    string logFolder = CloudConfig.Instance.LogPath;
                    if (string.IsNullOrEmpty(logFolder))
                    {
                        logFolder = AppDomain.CurrentDomain.BaseDirectory;
                        logFolder = Path.Combine(logFolder, "Log");
                    }
                    if (!Directory.Exists(logFolder))
                    {
                        Directory.CreateDirectory(logFolder);
                    }
                    _logFolder = logFolder;
                    //var logPath = Path.Combine(logFolder, LogFileNameFormat);
                    //_logPath = logPath;

                }
                return _logFolder;
            }
        }

        private ILog CreateEWSLogInstance()
        {
            ILog result;
            if (!IsRunningOnAzureOrStorageInAzure())
            {
                result = (ILog)(CreateTypeWithName<ILog>(LogImplAssembly, "Arcserve.Office365.Exchange.Log.Impl.DefaultEwsTraceLog"));
                result.RegisterLogStream(new LogToStreamManage(LogFolder, EwsLogFileName, LogMaxRecordCount));
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