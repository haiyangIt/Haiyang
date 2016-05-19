using Arcserve.Office365.Exchange.Util.Setting;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange
{
    public abstract class FactoryBase
    {
        protected FactoryBase() { }

        protected Assembly EwsDataImplAssembly = null;
        protected Assembly EwsServiceImplAssembly = null;
        protected Assembly DataProtectImplAssembly = null;
        protected Assembly LogImplAssembly = null;

        protected abstract Dictionary<Type, string> InterfaceImplTypeNameDic
        {
            get;
        }

        private static string _libPath;
        public static string LibPath
        {
            get
            {
                if (string.IsNullOrEmpty(_libPath))
                {
                    var directory = AppDomain.CurrentDomain.BaseDirectory;

                    var temp = Path.Combine(directory, "bin");
                    if (Directory.Exists(temp))
                    {
                        if (IsDllExist(temp))
                        {
                            _libPath = temp;
                        }
                    }
                    else if (Directory.Exists(directory))
                    {
                        if (IsDllExist(directory))
                        {
                            _libPath = directory;
                        }
                    }
                    else
                    {
                        temp = Path.Combine(directory, "..\\lib");

                        if (Directory.Exists(temp) && IsDllExist(temp))
                        {
                            _libPath = directory;
                        }
                        else
                        {
                            throw new NotSupportedException();
                        }
                    }
                }
                return _libPath;
            }
            set
            {
                _libPath = value;
            }
        }

        private static bool IsDllExist(string directory)
        {
            var result = Directory.EnumerateFiles(directory, AssemblyConfig.Instance.BinaryExchangeEwsApi);
            return result.Count() > 0;
        }

        protected object CreateType<T>(Assembly assemble)
        {
            var dataAccessImplType = assemble.GetType(InterfaceImplTypeNameDic[typeof(T)]);
            return Activator.CreateInstance(dataAccessImplType);
        }

        protected object CreateType<T>(Assembly assemble, params object[] constructParams)
        {
            var dataAccessImplType = assemble.GetType(InterfaceImplTypeNameDic[typeof(T)]);
            return Activator.CreateInstance(dataAccessImplType, constructParams);
        }

        protected object CreateTypeWithName<T>(Assembly assemble, string typeName)
        {
            var dataAccessImplType = assemble.GetType(typeName);
            return Activator.CreateInstance(dataAccessImplType);
        }

        protected object CreateTypeWithName<T>(Assembly assemble, string typeName, params object[] constructParams)
        {
            var dataAccessImplType = assemble.GetType(typeName);
            return Activator.CreateInstance(dataAccessImplType, constructParams);
        }

        public static CloudStorageAccount GetStorageAccount()
        {
            if (!IsRunningOnAzureOrStorageInAzure())
                return CloudStorageAccount.Parse(
        ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);
            else
            {
                return CloudStorageAccount.Parse(
        ConfigurationManager.ConnectionStrings["StorageConnectionStringRunning"].ConnectionString);
            }
        }

        public static bool IsRunningOnAzureOrStorageInAzure()
        {
            var isDebugAzure = ConfigurationManager.AppSettings["ForDebugAzure"];
            return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME")) || isDebugAzure == "1";
        }

        public static bool IsRunningOnAzure()
        {
            return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME"));
        }
    }

}
