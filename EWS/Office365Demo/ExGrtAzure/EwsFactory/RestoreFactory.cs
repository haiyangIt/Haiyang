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
using Microsoft.WindowsAzure.ServiceRuntime;
using System.Threading;
using EwsFrame.Util;

namespace EwsFrame
{
    public class RestoreFactory : FactoryBase
    {

        private static object _lock = new object();
        private static RestoreFactory _instance;
        public static RestoreFactory Instance
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

        private static RestoreFactory CreateFactory()
        {
            var result = new RestoreFactory();
            var sqlDbImplPath = Path.Combine(LibPath, "SqlDbImpl.dll");
            var ewsServicePath = Path.Combine(LibPath, "EwsService.dll");
            var dataProtectPath = Path.Combine(LibPath, "DataProtectImpl.dll");

            result.EwsDataImplAssembly = Assembly.LoadFrom(sqlDbImplPath);
            result.EwsServiceImplAssembly = Assembly.LoadFrom(ewsServicePath);
            result.DataProtectImplAssembly = Assembly.LoadFrom(dataProtectPath);


            result._interfaceImplTypeNameDic = new Dictionary<Type, string>(8);
            result._interfaceImplTypeNameDic.Add(typeof(IQueryCatalogDataAccess), "SqlDbImpl.QueryCatalogDataAccess");
            result._interfaceImplTypeNameDic.Add(typeof(IDataConvertFromDb), "SqlDbImpl.DataConvertFromDb");
            result._interfaceImplTypeNameDic.Add(typeof(ICatalogJob), "SqlDbImpl.Model.CatalogInfoModel");

            result._interfaceImplTypeNameDic.Add(typeof(IMailbox), "EwsService.Impl.MailboxOperatorImpl");
            result._interfaceImplTypeNameDic.Add(typeof(IFolder), "EwsService.Impl.FolderOperatorImpl");
            result._interfaceImplTypeNameDic.Add(typeof(IItem), "EwsService.Impl.ItemOperatorImpl");
            result._interfaceImplTypeNameDic.Add(typeof(IRestoreDestination), "EwsService.Impl.RestoreDestinationImpl");
            result._interfaceImplTypeNameDic.Add(typeof(IRestoreDestinationEx), "EwsService.Impl.RestoreDestinationExImpl");


            result._interfaceImplTypeNameDic.Add(typeof(IRestoreService), "DataProtectImpl.RestoreService");
            result._interfaceImplTypeNameDic.Add(typeof(IRestoreServiceEx), "DataProtectImpl.RestoreServiceEx");
            return result;
        }

        private Dictionary<Type, string> _interfaceImplTypeNameDic;
        protected override Dictionary<Type, string> InterfaceImplTypeNameDic
        {
            get
            {
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
            return (IRestoreDestinationEx)(CreateTypeWithName<IRestoreDestinationEx>(EwsServiceImplAssembly, "EwsService.Impl.RestoreDestinationOrgExImpl", argument, dataAccess));
        }

        public IQueryCatalogDataAccess NewDataAccessForQuery()
        {
            return (IQueryCatalogDataAccess)CreateType<IQueryCatalogDataAccess>(EwsDataImplAssembly);
        }

        internal IQueryCatalogDataAccess NewCatalogDataAccessInternal()
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
            return (IRestoreDestinationEx)(CreateTypeWithName<IRestoreDestinationEx>(EwsDataImplAssembly, "SqlDbImpl.RestoreToAzure"));
        }
    }
}
