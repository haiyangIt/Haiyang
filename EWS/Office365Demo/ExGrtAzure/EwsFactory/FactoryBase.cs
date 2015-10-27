using DataProtectInterface;
using EwsDataInterface;
using EwsServiceInterface;
using System;
using System.Collections.Generic;
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
                if(string.IsNullOrEmpty(_libPath))
                {
                    var directory = AppDomain.CurrentDomain.BaseDirectory;
                    directory = Path.Combine(directory, "..\\lib");
                    _libPath = directory;
                }
                return _libPath;
            }
            set
            {
                _libPath = value;
            }
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
    }
}
