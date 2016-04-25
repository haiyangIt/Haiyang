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

        //public static IDataAccess GetDataAccessInstance(TaskType taskType, EwsServiceArgument argument, string organization)
        //{
        //    if (_dataAccessObj == null)
        //        CreateDataAccess(taskType, argument, organization);
        //    return _dataAccessObj;
        //}

        private IDataAccess _dataAccessObj;
        public IDataAccess DataAccessObj
        {
            get
            {
                return _dataAccessObj;
            }
        }

        private static IDataAccess CreateDataAccess(TaskType taskType, EwsServiceArgument argument, string organization)
        {
            IDataAccess result;
            if (taskType == TaskType.Catalog)
            {
                result = CatalogFactory.Instance.NewCatalogDataAccessInternal(argument, organization);
            }
            else
            {
                result = RestoreFactory.Instance.NewCatalogDataAccessInternal();
            }
            return result;
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
            var result = new ServiceContext(userName, password, domainName, organization, taskType);
            return result;
        }

        public ServiceContext()
        {
            //if (ContextInstance == null)
            //    throw new NullReferenceException("Context not initialize.");
        }

        public string GetOrganizationPrefix()
        {
            return GetOrganizationPrefix(CurrentMailbox);
        }

        public static string GetOrganizationPrefix(string mailbox)
        {
            int atIndex = mailbox.IndexOf("@");
            string domain = mailbox.Substring(atIndex + 1, mailbox.Length - atIndex - 1).Replace(".", "-").ToLower();
            return domain;
        }
    }
}