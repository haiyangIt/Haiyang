using DataProtectInterface;
using EwsDataInterface;
using EwsFrame.Util;
using EwsServiceInterface;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.WindowsAzure.ServiceRuntime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EwsFrame
{
    public class CatalogFactory : FactoryBase
    {
        private static object _lock = new object();
        private static CatalogFactory _instance = null;
        public static CatalogFactory Instance
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

        private static CatalogFactory CreateFactory()
        {
            var result = new CatalogFactory();
            var sqlDbImplPath = Path.Combine(LibPath, "SqlDbImpl.dll");
            var ewsServicePath = Path.Combine(LibPath, "EwsService.dll");
            var dataProtectPath = Path.Combine(LibPath, "DataProtectImpl.dll");

            result.EwsDataImplAssembly = Assembly.LoadFrom(sqlDbImplPath);
            result.EwsServiceImplAssembly = Assembly.LoadFrom(ewsServicePath);
            result.DataProtectImplAssembly = Assembly.LoadFrom(dataProtectPath);

            result._interfaceImplTypeNameDic = new Dictionary<Type, string>();
            result._interfaceImplTypeNameDic.Add(typeof(ICatalogDataAccess), "SqlDbImpl.CatalogDataAccess");
            result._interfaceImplTypeNameDic.Add(typeof(IDataConvert), "SqlDbImpl.DataConvert");

            result._interfaceImplTypeNameDic.Add(typeof(IMailbox), "EwsService.Impl.MailboxOperatorImpl");
            result._interfaceImplTypeNameDic.Add(typeof(IFolder), "EwsService.Impl.FolderOperatorImpl");
            result._interfaceImplTypeNameDic.Add(typeof(IItem), "EwsService.Impl.ItemOperatorImpl");

            result._interfaceImplTypeNameDic.Add(typeof(ICatalogService), "DataProtectImpl.CatalogService");
            result._interfaceImplTypeNameDic.Add(typeof(IFilterItem), "DataProtectImpl.FilterBySelectedTree");
            return result;
        }


        private Dictionary<Type, string> _interfaceImplTypeNameDic = null;
        protected override Dictionary<Type, string> InterfaceImplTypeNameDic
        {
            get
            {
                return _interfaceImplTypeNameDic;
            }
        }

        public ICatalogService NewCatalogService(string adminUserName, string adminUserPassword, string domainName, string organization)
        {
            return (ICatalogService)CreateType<ICatalogService>(DataProtectImplAssembly, adminUserName, adminUserPassword, domainName, organization);
        }

        public IMailbox NewMailboxOperatorImpl()
        {
            return (IMailbox)CreateType<IMailbox>(EwsServiceImplAssembly);
        }
        public IFolder NewFolderOperatorImpl(ExchangeService service)
        {
            return (IFolder)CreateType<IFolder>(EwsServiceImplAssembly, service);
        }
        public IItem NewItemOperatorImpl(ExchangeService service, IDataAccess dataAccess)
        {
            return (IItem)CreateType<IItem>(EwsServiceImplAssembly, service, dataAccess);
        }

        internal ICatalogDataAccess NewCatalogDataAccessInternal(EwsServiceArgument argument, string organization)
        {
            return (ICatalogDataAccess)CreateType<ICatalogDataAccess>(EwsDataImplAssembly, argument, organization);
        }

        public IDataConvert NewDataConvert()
        {
            return (IDataConvert)CreateType<IDataConvert>(EwsDataImplAssembly);
        }

        //public IServiceContext GetServiceContext()
        //{
        //    return ServiceContext.ContextInstance;
        //}

        public IFilterItem NewFilterItemBySelectTree(LoadedTreeItem orgSelectItem)
        {
            return (IFilterItem)CreateType<IFilterItem>(DataProtectImplAssembly, orgSelectItem);
        }
    }
}
