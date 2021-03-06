﻿using DataProtectInterface;
using EwsServiceInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EwsFrame
{
    public class ServiceContext : IServiceContext
    {
        public OrganizationAdminInfo AdminInfo { get; private set; }
        

        //public static IServiceContext ContextInstance
        //{
        //    get
        //    {
        //        if (_ContextInstance == null)
        //        {
        //            throw new NullReferenceException();
        //        }
        //        return _ContextInstance;
        //    }
        //}

        //public IServiceContext CurrentContext
        //{
        //    get
        //    {
        //        if(_ContextInstance == null)
        //        return ContextInstance;
        //    }
        //}

        

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
        }

        public static IServiceContext NewServiceContext(string userName, string password, string domainName, string organization, TaskType taskType)
        {
            //if (_ContextInstance == null)
            return new ServiceContext(userName, password, domainName, organization, taskType);
        }

        public static string GetOrganizationPrefix(string mailbox)
        {
            int atIndex = mailbox.IndexOf("@");
            string domain = mailbox.Substring(atIndex + 1, mailbox.Length - atIndex - 1).Replace(".", "-").ToLower();
            return domain;
        }

        public void Dispose()
        {
        }
    }
}