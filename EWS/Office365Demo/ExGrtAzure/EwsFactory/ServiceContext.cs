using DataProtectInterface;
using EwsServiceInterface;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EwsFrame
{
    public class ServiceContext : IServiceContext
    {
        public OrganizationAdminInfo AdminInfo { get; private set; }

        [ThreadStatic]
        private static IServiceContext _ContextInstance;

        public static IServiceContext ContextInstance
        {
            get
            {
                if (_ContextInstance == null)
                {
                    throw new NullReferenceException();
                }
                return _ContextInstance;
            }
        }

        public IServiceContext CurrentContext
        {
            get
            {
                return ContextInstance;
            }
        }

        [ThreadStatic]
        private static IDataAccess _dataAccessObj;

        public static IDataAccess GetDataAccessInstance(TaskType taskType, EwsServiceArgument argument, string organization)
        {
            if (_dataAccessObj == null)
                CreateDataAccess(taskType, argument, organization);
            return _dataAccessObj;
        }

        public IDataAccess DataAccessObj
        {
            get
            {
                if (_dataAccessObj == null)
                {
                    CreateDataAccess(TaskType, Argument, AdminInfo.OrganizationName);
                }
                return _dataAccessObj;
            }
        }

        private static void CreateDataAccess(TaskType taskType, EwsServiceArgument argument, string organization)
        {
            if (_dataAccessObj == null)
            {
                if (taskType == TaskType.Catalog)
                {
                    _dataAccessObj = CatalogFactory.Instance.NewCatalogDataAccessInternal(argument, organization);
                }
                else
                {
                    _dataAccessObj = RestoreFactory.Instance.NewCatalogDataAccessInternal();
                }
            }
        }

        public TaskType TaskType { get; private set; }

        private EwsServiceArgument _argument;
        public EwsServiceArgument Argument
        {
            get
            {
                if (!String.IsNullOrEmpty(CurrentMailbox))
                {
                    _argument.SetConnectMailbox(CurrentMailbox);
                }
                return _argument;
            }
        }

        public string Organization
        {
            get
            {
                if (AdminInfo != null)
                    return AdminInfo.OrganizationName;
                return string.Empty;
            }
        }

        private string _currentMailbox;
        public string CurrentMailbox
        {
            get { return _currentMailbox; }
            set
            {
                _currentMailbox = value;
                if (!String.IsNullOrEmpty(CurrentMailbox))
                {
                    _argument.SetConnectMailbox(CurrentMailbox);
                }
            }
        }

        public Exception LastException
        {
            get; set;
        }

        private ServiceContext(string adminUserName, string adminPassword, string domainName, string organization, TaskType taskType)
        {
            AdminInfo = new OrganizationAdminInfo();

            AdminInfo.UserName = adminUserName;
            AdminInfo.UserPassword = adminPassword;
            AdminInfo.UserDomain = domainName;
            AdminInfo.OrganizationName = organization;

            _argument = new EwsServiceArgument();
            _argument.ServiceCredential = new System.Net.NetworkCredential(adminUserName, adminPassword);
            _argument.UseDefaultCredentials = false;

            CurrentMailbox = adminUserName;

            TaskType = taskType;
            CreateDataAccess(TaskType, Argument, organization);
        }

        public static IServiceContext NewServiceContext(string userName, string password, string domainName, string organization, TaskType taskType)
        {
            if (_ContextInstance == null)
                _ContextInstance = new ServiceContext(userName, password, domainName, organization, taskType);
            return ContextInstance;
        }

        public ServiceContext()
        {
            if (ContextInstance == null)
                throw new NullReferenceException("Context not initialize.");
        }

        public string GetOrganizationPrefix()
        {
            int atIndex = CurrentMailbox.IndexOf("@");
            string domain = CurrentMailbox.Substring(atIndex + 1, CurrentMailbox.Length - atIndex - 1).Replace(".", "-").ToLower();
            return domain;
        }
    }
}