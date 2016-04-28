using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Exchange.WebServices.Data;
using System.Reflection;
using System.IO;
using Arcserve.Office365.Exchange.StorageAccess.Interface;
using Arcserve.Office365.Exchange.Data;
using Arcserve.Office365.Exchange.EwsApi;
using Arcserve.Office365.Exchange.Util.Setting;

namespace Arcserve.Office365.Exchange.DataProtect.Interface
{
    public class RestoreFactory : FactoryBase
    {
        private static RestoreFactory _instance;
        public static RestoreFactory Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new RestoreFactory();
                    var sqlDbImplPath = Path.Combine(LibPath, AssemblyConfig.Instance.BinaryStorageAccessInAzure);
                    var ewsServicePath = Path.Combine(LibPath, AssemblyConfig.Instance.BinaryExchangeEwsApi);
                    var dataProtectPath = Path.Combine(LibPath, AssemblyConfig.Instance.BinaryDataProtectImplement);
                    var dataPath = Path.Combine(LibPath, AssemblyConfig.Instance.BinaryDataImplement);

                    _instance.EwsDataImplAssembly = Assembly.LoadFrom(sqlDbImplPath);
                    _instance.EwsServiceImplAssembly = Assembly.LoadFrom(ewsServicePath);
                    _instance.DataProtectImplAssembly = Assembly.LoadFrom(dataProtectPath);
                    _instance.DataImplAssembly = Assembly.LoadFrom(dataPath);
                }
                return _instance;
            }
        }

        private Assembly DataImplAssembly { get; set; }
        private Dictionary<Type, string> _interfaceImplTypeNameDic;
        protected override Dictionary<Type, string> InterfaceImplTypeNameDic
        {
            get
            {
                if (_interfaceImplTypeNameDic == null)
                {
                    _interfaceImplTypeNameDic = new Dictionary<Type, string>(8);
                    _interfaceImplTypeNameDic.Add(typeof(IQueryCatalogDataAccess), AssemblyConfig.Instance.IQueryCatalogDataAccessImplClass);
                    _interfaceImplTypeNameDic.Add(typeof(IDataConvertFromDb), AssemblyConfig.Instance.IDataConvertFromDbImplClass);
                    _interfaceImplTypeNameDic.Add(typeof(ICatalogJob), AssemblyConfig.Instance.ICatalogJobImplClass);

                    _interfaceImplTypeNameDic.Add(typeof(IMailbox), AssemblyConfig.Instance.IMailboxImplClass);
                    _interfaceImplTypeNameDic.Add(typeof(IFolder), AssemblyConfig.Instance.IFolderImplClass);
                    _interfaceImplTypeNameDic.Add(typeof(IItem), AssemblyConfig.Instance.IItemImplClass);
                    _interfaceImplTypeNameDic.Add(typeof(IRestoreDestination), AssemblyConfig.Instance.IRestoreDestinationImplClass);
                    _interfaceImplTypeNameDic.Add(typeof(IRestoreDestinationEx), AssemblyConfig.Instance.IRestoreDestinationExImplClass);


                    _interfaceImplTypeNameDic.Add(typeof(IRestoreService), AssemblyConfig.Instance.IRestoreServiceImplClass);
                    _interfaceImplTypeNameDic.Add(typeof(IRestoreServiceEx), AssemblyConfig.Instance.IRestoreServiceExImplClass);
                }

                return _interfaceImplTypeNameDic;
            }
        }

        public IRestoreService NewRestoreService(string adminUserName, string adminPassword, string domainName, string organizationName)
        {
            return (IRestoreService)CreateType<IRestoreService>(DataProtectImplAssembly, adminUserName, adminPassword, domainName, organizationName);
        }

        public IRestoreService NewRestoreService(string adminUserName, string organizationName)
        {
            return (IRestoreService)CreateType<IRestoreService>(DataProtectImplAssembly, adminUserName, organizationName);
        }

        public IRestoreDestination NewRestoreDestination(EwsServiceArgument argument, IDataAccess dataAccess)
        {
            return (IRestoreDestination)CreateType<IRestoreDestination>(EwsServiceImplAssembly, argument, dataAccess);
        }

        public IRestoreServiceEx NewRestoreServiceEx(string adminUserName, string adminPassword, string domainName, string organizationName)
        {
            return (IRestoreServiceEx)CreateType<IRestoreServiceEx>(DataProtectImplAssembly, adminUserName, adminPassword, domainName, organizationName);
        }

        public IRestoreDestinationEx NewRestoreDestinationEx(EwsServiceArgument argument, IDataAccess dataAccess)
        {
            return (IRestoreDestinationEx)CreateType<IRestoreDestinationEx>(EwsServiceImplAssembly, argument, dataAccess);
        }

        public IRestoreDestinationEx NewRestoreDestinationOrgEx(EwsServiceArgument argument, IDataAccess dataAccess)
        {
            return (IRestoreDestinationEx)(CreateTypeWithName<IRestoreDestinationEx>(EwsServiceImplAssembly, AssemblyConfig.Instance.IRestoreDestionationToOrgClass, argument, dataAccess));
        }

        public IQueryCatalogDataAccess NewDataAccessForQuery()
        {
            return (IQueryCatalogDataAccess)CreateType<IQueryCatalogDataAccess>(EwsDataImplAssembly);
        }

        public IQueryCatalogDataAccess NewCatalogDataAccessInternal()
        {
            return (IQueryCatalogDataAccess)CreateType<IQueryCatalogDataAccess>(EwsDataImplAssembly);
        }

        //public IServiceContext GetServiceContext()
        //{
        //    return ServiceContext.ContextInstance;
        //}

        public IDataConvertFromDb NewDataConvert()
        {
            return (IDataConvertFromDb)CreateType<IDataConvertFromDb>(EwsDataImplAssembly);
        }

        public IMailbox NewMailboxOperatorImpl()
        {
            return (IMailbox)CreateType<IMailbox>(EwsServiceImplAssembly);
        }

        public IFolder NewFolderOperatorImpl(ExchangeService currentExService)
        {
            return (IFolder)CreateType<IFolder>(EwsServiceImplAssembly, currentExService);
        }

        public IItem NewItemOperatorImpl(ExchangeService currentExService, IDataAccess dataAccess)
        {
            return (IItem)CreateType<IItem>(EwsServiceImplAssembly, currentExService, dataAccess);
        }

        public ICatalogJob NewCatalogJob(string organizationName, string catalogJobName, DateTime catalogTime)
        {
            return (ICatalogJob)CreateType<ICatalogJob>(EwsDataImplAssembly, organizationName, catalogJobName, catalogTime);
        }

        public IRestoreDestinationEx NewDumpDestination()
        {
            return (IRestoreDestinationEx)(CreateTypeWithName<IRestoreDestinationEx>(EwsDataImplAssembly, AssemblyConfig.Instance.IRestoreToAzureImplClass));
        }
    }
}
