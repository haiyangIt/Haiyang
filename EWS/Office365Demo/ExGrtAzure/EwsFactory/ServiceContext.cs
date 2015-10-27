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

        public Dictionary<string, object> OtherInformation { get; private set; }

        [ThreadStatic]
        public static IServiceContext ContextInstance;

        public IServiceContext CurrentContext { get { return ContextInstance; } }

        public IDataAccess DataAccessObj { get; private set; }

        public TaskType TaskType { get; private set; }

        private EwsServiceArgument _argument;
        public EwsServiceArgument Argument {
            get
            {
                if(!String.IsNullOrEmpty(CurrentMailbox))
                {
                    _argument.ServiceEmailAddress = CurrentMailbox;
                    _argument.UserToImpersonate = new ImpersonatedUserId(ConnectingIdType.SmtpAddress, CurrentMailbox);
                    _argument.SetXAnchorMailbox = true;
                    _argument.XAnchorMailbox = CurrentMailbox;
                }
                return _argument;
            }
        }

        public string Organization { get; private set; }

        public string CurrentMailbox { get; set; }

        public ServiceContext(string userName, string password, string domainName, string organization, TaskType taskType)
        {
            if (ContextInstance != null)
                return;

            ContextInstance = this;
            AdminInfo = new OrganizationAdminInfo();

            AdminInfo.UserName = userName;
            AdminInfo.UserPassword = password;
            AdminInfo.UserDomain = domainName;
            AdminInfo.OrganizationName = organization;

            OtherInformation = new Dictionary<string, object>();

            _argument = new EwsServiceArgument();
            _argument.ServiceCredential = new System.Net.NetworkCredential(userName, password);
            _argument.UseDefaultCredentials = false;

            TaskType = taskType;
            if(taskType == TaskType.Catalog)
            {
                DataAccessObj = CatalogFactory.Instance.NewCatalogDataAccess();
            }
            else
            {
                DataAccessObj = RestoreFactory.Instance.NewDataAccess();
            }
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