using DataProtectInterface;
using EwsDataInterface;
using EwsServiceInterface;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EwsFrame
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
                    if (IsDllExist(temp))
                    {
                        _libPath = temp;
                    }
                    else
                    {
                        temp = Path.Combine(directory, "..\\lib");

                        if (IsDllExist(temp))
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
            var result = Directory.EnumerateFiles(directory, "*EwsDataInterface.dll*");
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
            if (!IsRunningOnAzure())
                return CloudStorageAccount.Parse(
        ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);
            else
            {
                return CloudStorageAccount.Parse(
        ConfigurationManager.ConnectionStrings["StorageConnectionStringRunning"].ConnectionString);
            }
        }

        public static bool IsRunningOnAzure()
        {
            return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME"));
        }
    }
}
