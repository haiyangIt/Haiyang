using DataProtectInterface;
using EwsDataInterface;
using EwsFrame;
using EwsFrame.Cache;
using EwsServiceInterface;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security;

namespace DataProtectImpl
{
    /// <summary>
    /// If we need generate catalog concurrently, we need control the granularity to mailbox level.
    /// And we need use the database to record the current generate catalog job information.
    /// </summary>
    public class CatalogService : ICatalogService, ICatalogJob
    {

        public DateTime LastCatalogTime { get; set; }

        public DateTime StartTime { get; set; }

        private string _catalogJobName;
        public string CatalogJobName {
            get
            {
                if(string.IsNullOrEmpty(_catalogJobName))
                {
                    _catalogJobName = string.Format("{0} Catalog Job {1}", AdminInfo.OrganizationName, StartTime.ToString("yyyyMMddHHmmss"));
                }
                return _catalogJobName;
            }
            set
            {
                _catalogJobName = value;
            }
        }

        public string Organization { get { return AdminInfo.OrganizationName; } }

        private IServiceContext _serviceContext;
        public IServiceContext ServiceContext
        {
            get
            {
                return _serviceContext.CurrentContext;
            }
        }

        public OrganizationAdminInfo AdminInfo { get; private set; }


        public CatalogService(string adminUserName, string adminPassword, string domainName, string organizationName)
        {
            if (string.IsNullOrEmpty(adminUserName))
                throw new ArgumentException("user name is null", "adminUserName");
            if (string.IsNullOrEmpty(adminPassword))
                throw new ArgumentException("password is null", "adminPassword");

            AdminInfo = new OrganizationAdminInfo();
            AdminInfo.UserName = adminUserName;
            AdminInfo.UserPassword = adminPassword;
            AdminInfo.UserDomain = domainName;
            AdminInfo.OrganizationName = organizationName;

            _serviceContext = new ServiceContext(AdminInfo.UserName, AdminInfo.UserPassword, AdminInfo.UserDomain, AdminInfo.OrganizationName, TaskType.Catalog);
        }

        public List<IMailboxData> GetAllUserMailbox()
        {
            const string liveIDConnectionUri = "https://outlook.office365.com/PowerShell-LiveID";
            const string schemaUri = "http://schemas.microsoft.com/powershell/Microsoft.Exchange";
            PSCredential credentials = new PSCredential(AdminInfo.UserName, StringToSecureString(AdminInfo.UserPassword));

            WSManConnectionInfo connectionInfo = new WSManConnectionInfo(
        new Uri(liveIDConnectionUri),
        schemaUri, credentials);
            connectionInfo.AuthenticationMechanism = AuthenticationMechanism.Basic;

            using (Runspace runspace = RunspaceFactory.CreateRunspace(connectionInfo))
            {
                using (Pipeline pipe = runspace.CreatePipeline())
                {

                    Command CommandGetMailbox = new Command("Get-Mailbox");
                    CommandGetMailbox.Parameters.Add("RecipientTypeDetails", "UserMailbox");
                    pipe.Commands.Add(CommandGetMailbox);

                    var props = new string[] { "Name", "DisplayName", "UserPrincipalName" };
                    Command CommandSelect = new Command("Select-Object");
                    CommandSelect.Parameters.Add("Property", props);
                    pipe.Commands.Add(CommandSelect);

                    
                    runspace.Open();
                    
                    var information = pipe.Invoke();
                    List<IMailboxData> result = new List<IMailboxData>(information.Count);
                    string displayName = string.Empty;
                    string address = string.Empty;
                    foreach (PSObject eachUserMailBox in information)
                    {
                        displayName = string.Empty;
                        address = string.Empty;
                        foreach (PSPropertyInfo propertyInfo in eachUserMailBox.Properties)
                        {
                            if (propertyInfo.Name == "DisplayName")
                                displayName = propertyInfo.Value.ToString();
                            if (propertyInfo.Name == "UserPrincipalName")
                                address = propertyInfo.Value.ToString().ToLower();
                        }

                        if (address == "haiyang.ling@arcserve.com")
                            result.Add(new MailboxInfo(displayName, address));
                    }
                    return result;
                }
            }
        }
        private static SecureString StringToSecureString(string str)
        {
            SecureString ss = new SecureString();
            char[] passwordChars = str.ToCharArray();

            foreach (char c in passwordChars)
            {
                ss.AppendChar(c);
            }
            return ss;
        }

        public IMailbox NewMailboxOperatorInstance()
        {
            IMailbox obj = CatalogFactory.Instance.NewMailboxOperatorImpl();
            return obj;
        }

        public virtual ICatalogDataAccess NewDataAccessInstance()
        {
            return CatalogFactory.Instance.NewCatalogDataAccess();
        }

        public IDataConvert NewDataConvertInstance()
        {
            var result = CatalogFactory.Instance.NewDataConvert();
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        public void GenerateCatalog()
        {
            StartTime = DateTime.Now;

            GenerateCatalogStart();
            bool isFinished = false;
            try {
                List<IMailboxData> allUserMailbox = GetAllUserMailbox();
                ICatalogDataAccess dataAccess = NewDataAccessInstance();
                IDataConvert dataConvert = NewDataConvertInstance();
                dataConvert.StartTime = StartTime;
                dataConvert.OrganizationName = AdminInfo.OrganizationName;
                ICatalogJob lastCatalogInfo = dataAccess.GetLastCatalogJob(StartTime);
                if (lastCatalogInfo == null)
                    LastCatalogTime = DateTime.MinValue;
                else
                    LastCatalogTime = lastCatalogInfo.StartTime;


                foreach (IMailboxData userMailbox in allUserMailbox)
                {
                    if (!IsNeedGenerateMailbox(userMailbox))
                        continue;

                    MailboxGenerateStart(userMailbox);
                    bool hasError = true;
                    try
                    {
                        IMailbox mailboxOperator = NewMailboxOperatorInstance();
                        mailboxOperator.ConnectMailbox(_serviceContext.Argument, userMailbox.MailAddress);

                        IFolder folderOperator = CatalogFactory.Instance.NewFolderOperatorImpl(mailboxOperator.CurrentExchangeService);
                        Folder rootFolder = folderOperator.GetRootFolder();

                        ((MailboxInfo)userMailbox).RootFolderId = rootFolder.Id.UniqueId;
                        IMailboxData mailboxData = dataConvert.Convert(userMailbox);
                        dataAccess.SaveMailbox(mailboxData);

                        IFolderData rootFolderData = dataConvert.Convert(rootFolder);
                        IItem itemOperator = CatalogFactory.Instance.NewItemOperatorImpl(folderOperator.CurrentExchangeService);
                        //dataAccess.SaveFolder(rootFolderData, mailboxData, null); // root folder don't need save.
                        GenerateEachFolderCatalog(mailboxData, rootFolderData, rootFolder, folderOperator, itemOperator, dataAccess, dataConvert);
                        hasError = false;
                    }

                    finally
                    {
                        MailboxGenerateEnd(userMailbox, hasError);
                    }
                }

                ICatalogJob job = dataConvert.Convert(this);
                dataAccess.SaveCatalogJob(job);
                isFinished = true;
            }
            finally
            {
                GenerateCatalogEnd(isFinished);
            }
        }

        private bool IsNeedGenerateMailbox(IMailboxData userMailbox)
        {
            if (!string.IsNullOrEmpty(_generateSpecificMailbox) && userMailbox.MailAddress.ToLower() != _generateSpecificMailbox.ToLower())
                return false;
            return true;
        }

        public void MailboxGenerateStart(IMailboxData mailbox)
        {
            _serviceContext.CurrentMailbox = mailbox.MailAddress;
            MailboxCacheManager.CacheManager.ReleaseCache();
        }

        public void MailboxGenerateEnd(IMailboxData mailbox, bool hasError)
        {
            if (!hasError)
            {
                MailboxCacheManager.CacheManager.ReleaseCache(true);
            }
            else
                MailboxCacheManager.CacheManager.ReleaseCache();
            _serviceContext.CurrentMailbox = string.Empty;
        }

        public void GenerateCatalogStart()
        {
            CatalogFactory.Instance.NewCatalogDataAccess().BeginTransaction();
        }

        public void GenerateCatalogEnd(bool isFinished)
        {
            CatalogFactory.Instance.NewCatalogDataAccess().EndTransaction(isFinished);
        }

        private void GenerateEachFolderCatalog(IMailboxData mailboxData, IFolderData folderData, Folder folder, IFolder folderOperator, IItem itemOperator,
            ICatalogDataAccess dataAccess, IDataConvert dataConvert, int level = 0)
        {
            if (level != 0) // root folder don't need save items;
            {
                if (folder.TotalCount > 0)
                {
                    List<Item> folderItems = itemOperator.GetFolderItems(folder);
                    foreach (var item in folderItems)
                    {
                        IItemData itemData = dataConvert.Convert(item);
                        dataAccess.SaveItem(itemData, mailboxData, folderData);

                        bool isCheckExist = false;
                        bool isExist = false;
                        var itemIsNew = itemOperator.IsItemNew(item, LastCatalogTime, StartTime);
                        if (itemIsNew)
                        {
                            dataAccess.SaveItemContent(itemData, StartTime, isCheckExist, isExist);
                        }
                    }
                }
            }

            if (folder.ChildFolderCount > 0)
            {
                List<Folder> childFolders = folderOperator.GetChildFolder(folder);
                foreach (var childFolder in childFolders)
                {
                    if (!folderOperator.IsFolderNeedGenerateCatalog(childFolder))
                        continue;

                    if (!IsNeedGenerateFolder(childFolder, level))
                        continue;

                    IFolderData childFolderData = dataConvert.Convert(childFolder);

                    dataAccess.SaveFolder(childFolderData, mailboxData, folderData);
                    GenerateEachFolderCatalog(mailboxData, childFolderData, childFolder, folderOperator, itemOperator, dataAccess, dataConvert, level + 1);
                }
            }
        }

        private bool IsNeedGenerateFolder(Folder childFolder, int level)
        {
            if(level == 0)
            {
                if (!string.IsNullOrEmpty(_generateSpecificFolder) && _generateSpecificFolder != childFolder.DisplayName)
                    return false;
            }
            return true;
        }

        private string _generateSpecificMailbox;
        public void GenerateCatalog(string mailbox)
        {
            _generateSpecificMailbox = mailbox;
            GenerateCatalog();
        }


        private string _generateSpecificFolder;
        public void GenerateCatalog(string mailbox, string folder)
        {
            _generateSpecificMailbox = mailbox;
            _generateSpecificFolder = folder;
            GenerateCatalog();
        }

        class MailboxInfo : IMailboxData
        {
            public MailboxInfo(string displayName, string mailboxAddress)
            {
                DisplayName = displayName;
                MailAddress = mailboxAddress;
            }

            public string DisplayName
            {
                get; private set;
            }

            public string Location
            {
                get; set;
            }

            public string MailAddress
            {
                get; private set;
            }

            public string RootFolderId
            {
                get; set;
            }
        }
    }
}