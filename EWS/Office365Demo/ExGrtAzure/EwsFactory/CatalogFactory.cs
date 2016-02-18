using DataProtectInterface;
using EwsDataInterface;
using EwsServiceInterface;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.WindowsAzure.ServiceRuntime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EwsFrame
{
    public class CatalogFactory : FactoryBase
    {
        private static CatalogFactory _instance;
        public static CatalogFactory Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CatalogFactory();
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


        private static Dictionary<Type, string> _interfaceImplTypeNameDic = null;
        protected override Dictionary<Type, string> InterfaceImplTypeNameDic
        {
            get
            {
                if (_interfaceImplTypeNameDic == null)
                {
                    _interfaceImplTypeNameDic = new Dictionary<Type, string>();
                    _interfaceImplTypeNameDic.Add(typeof(ICatalogDataAccess), "SqlDbImpl.CatalogDataAccess");
                    _interfaceImplTypeNameDic.Add(typeof(IDataConvert), "SqlDbImpl.DataConvert");

                    _interfaceImplTypeNameDic.Add(typeof(IMailbox), "EwsService.Impl.MailboxOperatorImpl");
                    _interfaceImplTypeNameDic.Add(typeof(IFolder), "EwsService.Impl.FolderOperatorImpl");
                    _interfaceImplTypeNameDic.Add(typeof(IItem), "EwsService.Impl.ItemOperatorImpl");

                    _interfaceImplTypeNameDic.Add(typeof(ICatalogService), "DataProtectImpl.CatalogService");
                    _interfaceImplTypeNameDic.Add(typeof(IFilterItem), "DataProtectImpl.FilterBySelectedTree");
                }
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

        public IServiceContext GetServiceContext()
        {
            return ServiceContext.ContextInstance;
        }

        public IFilterItem NewFilterItemBySelectTree(LoadedTreeItem orgSelectItem)
        {
            return (IFilterItem)CreateType<IFilterItem>(DataProtectImplAssembly, orgSelectItem);
        }
    }
}
