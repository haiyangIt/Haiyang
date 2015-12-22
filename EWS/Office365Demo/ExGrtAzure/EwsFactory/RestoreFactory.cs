using DataProtectInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EwsDataInterface;
using EwsServiceInterface;
using Microsoft.Exchange.WebServices.Data;
using System.Reflection;
using System.IO;

namespace EwsFrame
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

        private Dictionary<Type, string> _interfaceImplTypeNameDic;
        protected override Dictionary<Type, string> InterfaceImplTypeNameDic
        {
            get
            {
                if (_interfaceImplTypeNameDic == null)
                {
                    _interfaceImplTypeNameDic = new Dictionary<Type, string>(8);
                    _interfaceImplTypeNameDic.Add(typeof(IQueryCatalogDataAccess), "SqlDbImpl.QueryCatalogDataAccess");
                    _interfaceImplTypeNameDic.Add(typeof(IDataConvertFromDb), "SqlDbImpl.DataConvertFromDb");
                    _interfaceImplTypeNameDic.Add(typeof(ICatalogJob), "SqlDbImpl.Model.CatalogInfoModel");

                    _interfaceImplTypeNameDic.Add(typeof(IMailbox), "EwsService.Impl.MailboxOperatorImpl");
                    _interfaceImplTypeNameDic.Add(typeof(IFolder), "EwsService.Impl.FolderOperatorImpl");
                    _interfaceImplTypeNameDic.Add(typeof(IItem), "EwsService.Impl.ItemOperatorImpl");
                    _interfaceImplTypeNameDic.Add(typeof(IRestoreDestination), "EwsService.Impl.RestoreDestinationImpl");
                    _interfaceImplTypeNameDic.Add(typeof(IRestoreDestinationEx), "EwsService.Impl.RestoreDestinationExImpl");


                    _interfaceImplTypeNameDic.Add(typeof(IRestoreService), "DataProtectImpl.RestoreService");
                    _interfaceImplTypeNameDic.Add(typeof(IRestoreServiceEx), "DataProtectImpl.RestoreServiceEx");
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

        public IRestoreDestination NewRestoreDestination()
        {
            return (IRestoreDestination)CreateType<IRestoreDestination>(EwsServiceImplAssembly);
        }

        public IRestoreServiceEx NewRestoreServiceEx(string adminUserName, string adminPassword, string domainName, string organizationName)
        {
            return (IRestoreServiceEx)CreateType<IRestoreServiceEx>(DataProtectImplAssembly, adminUserName, adminPassword, domainName, organizationName);
        }

        public IRestoreDestinationEx NewRestoreDestinationEx()
        {
            return (IRestoreDestinationEx)CreateType<IRestoreDestinationEx>(EwsServiceImplAssembly);
        }

        public IQueryCatalogDataAccess NewDataAccessForQuery()
        {
            return (IQueryCatalogDataAccess)CreateType<IQueryCatalogDataAccess>(EwsDataImplAssembly);
        }

        internal IQueryCatalogDataAccess NewCatalogDataAccessInternal()
        {
            return (IQueryCatalogDataAccess)CreateType<IQueryCatalogDataAccess>(EwsDataImplAssembly);
        }

        public IServiceContext GetServiceContext()
        {
            return ServiceContext.ContextInstance;
        }

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

        public IItem NewItemOperatorImpl(ExchangeService currentExService)
        {
            return (IItem)CreateType<IItem>(EwsServiceImplAssembly, currentExService);
        }

        public ICatalogJob NewCatalogJob(string organizationName, string catalogJobName, DateTime catalogTime)
        {
            return (ICatalogJob)CreateType<ICatalogJob>(EwsDataImplAssembly, organizationName, catalogJobName, catalogTime);
        }

        public IRestoreDestinationEx NewDumpDestination()
        {
            return (IRestoreDestinationEx)(CreateTypeWithName<IRestoreDestinationEx>(EwsDataImplAssembly, "SqlDbImpl.RestoreToAzure"));
        }
    }
}
