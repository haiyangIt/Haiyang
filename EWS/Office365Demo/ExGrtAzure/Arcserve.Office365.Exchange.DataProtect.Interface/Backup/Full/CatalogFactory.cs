using Arcserve.Office365.Exchange.Data;
using Arcserve.Office365.Exchange.EwsApi;
using Arcserve.Office365.Exchange.StorageAccess.Interface;
using Arcserve.Office365.Exchange.Util.Setting;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.DataProtect.Interface
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
                    var sqlDbImplPath = Path.Combine(LibPath, AssemblyConfig.Instance.BinaryStorageAccessInAzure);
                    var ewsServicePath = Path.Combine(LibPath, AssemblyConfig.Instance.BinaryExchangeEwsApi);
                    var dataProtectPath = Path.Combine(LibPath, AssemblyConfig.Instance.BinaryDataProtectImplement);

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
                    _interfaceImplTypeNameDic.Add(typeof(ICatalogDataAccess), AssemblyConfig.Instance.ICatalogDataAccessImplClass);
                    _interfaceImplTypeNameDic.Add(typeof(IDataConvert), AssemblyConfig.Instance.IDataConvertImplClass);

                    _interfaceImplTypeNameDic.Add(typeof(IMailbox), AssemblyConfig.Instance.IMailboxImplClass);
                    _interfaceImplTypeNameDic.Add(typeof(IFolder), AssemblyConfig.Instance.IFolderImplClass);
                    _interfaceImplTypeNameDic.Add(typeof(IItem), AssemblyConfig.Instance.IItemImplClass);

                    _interfaceImplTypeNameDic.Add(typeof(ICatalogService), AssemblyConfig.Instance.ICatalogServiceImplClass);
                    _interfaceImplTypeNameDic.Add(typeof(IFilterItem), AssemblyConfig.Instance.IFilterItemImplClass);
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

        public ICatalogDataAccess NewCatalogDataAccessInternal(EwsServiceArgument argument, string organization)
        {
            return (ICatalogDataAccess)CreateType<ICatalogDataAccess>(EwsDataImplAssembly, argument, organization);
        }

        public IDataConvert NewDataConvert()
        {
            return (IDataConvert)CreateType<IDataConvert>(EwsDataImplAssembly);
        }

        public IFilterItem NewFilterItemBySelectTree(LoadedTreeItem orgSelectItem)
        {
            return (IFilterItem)CreateType<IFilterItem>(DataProtectImplAssembly, orgSelectItem);
        }
    }
}
