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
using DataProtectInterface.Event;

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
        public string CatalogJobName
        {
            get
            {
                if (string.IsNullOrEmpty(_catalogJobName))
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

                        if (!IsNeedGenerateMailbox(address))
                            result.Add(new MailboxInfo(displayName, address));
                    }
                    return result;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void GenerateCatalog()
        {
            StartTime = DateTime.Now;

            GenerateCatalogStart();
            bool isFinished = false;
            try
            {
                OnProgressChanged(CatalogProgressType.GetAllMailboxStart);
                List<IMailboxData> allUserMailbox = GetAllUserMailbox();
                OnProgressChanged(CatalogProgressType.GetAllMailboxEnd, new Process(-1, allUserMailbox.Count));
                ICatalogDataAccess dataAccess = NewDataAccessInstance();
                IDataConvert dataConvert = NewDataConvertInstance();
                dataConvert.StartTime = StartTime;
                dataConvert.OrganizationName = AdminInfo.OrganizationName;
                ICatalogJob lastCatalogInfo = dataAccess.GetLastCatalogJob(StartTime);
                if (lastCatalogInfo == null)
                    LastCatalogTime = DateTime.MinValue;
                else
                    LastCatalogTime = lastCatalogInfo.StartTime;

                int mailboxIndex = 0;
                int totalMailboxCount = allUserMailbox.Count;
                Stack<IFolderData> folderStack = new Stack<IFolderData>(4);
                foreach (IMailboxData userMailbox in allUserMailbox)
                {
                    mailboxIndex++;

                    MailboxGenerateStart(userMailbox, mailboxIndex, totalMailboxCount);

                    bool hasError = true;
                    try
                    {
                        IMailbox mailboxOperator = NewMailboxOperatorInstance();

                        OnMailboxProgressChanged(CatalogMailboxProgressType.ConnectMailboxStart, userMailbox);
                        mailboxOperator.ConnectMailbox(_serviceContext.Argument, userMailbox.MailAddress);
                        OnMailboxProgressChanged(CatalogMailboxProgressType.ConnectMailboxEnd, userMailbox);

                        IFolder folderOperator = CatalogFactory.Instance.NewFolderOperatorImpl(mailboxOperator.CurrentExchangeService);

                        OnMailboxProgressChanged(CatalogMailboxProgressType.GetRootFolderStart, userMailbox);
                        Folder rootFolder = folderOperator.GetRootFolder();
                        OnMailboxProgressChanged(CatalogMailboxProgressType.GetRootFolderEnd, userMailbox);

                        ((MailboxInfo)userMailbox).RootFolderId = rootFolder.Id.UniqueId;
                        IMailboxData mailboxData = dataConvert.Convert(userMailbox);
                        OnMailboxProgressChanged(CatalogMailboxProgressType.SaveMailboxStart, userMailbox);
                        dataAccess.SaveMailbox(mailboxData);
                        OnMailboxProgressChanged(CatalogMailboxProgressType.SaveMailboxEnd, userMailbox);

                        IFolderData rootFolderData = dataConvert.Convert(rootFolder);
                        IItem itemOperator = CatalogFactory.Instance.NewItemOperatorImpl(folderOperator.CurrentExchangeService);
                        //dataAccess.SaveFolder(rootFolderData, mailboxData, null); // root folder don't need save.

                        folderStack.Push(rootFolderData);
                        GenerateEachFolderCatalog(mailboxData,
                            rootFolderData, rootFolder,
                            folderOperator, itemOperator,
                            dataAccess,
                            dataConvert,
                            folderStack);
                        folderStack.Pop();

                        hasError = false;
                    }

                    finally
                    {
                        MailboxGenerateEnd(userMailbox, hasError, mailboxIndex, totalMailboxCount);
                    }
                }

                ICatalogJob job = dataConvert.Convert(this);
                dataAccess.SaveCatalogJob(job);
                isFinished = true;
            }
            catch(Exception e)
            {
                OnExceptionThrowed(e);
                throw new CatalogException("Catalog Failure, please view inner Exception.", e);
            }
            finally
            {
                GenerateCatalogEnd(isFinished);
            }
        }

        public void GenerateCatalog(string mailbox)
        {
            _generateSpecificMailbox = mailbox;
            GenerateCatalog();
        }

        public void GenerateCatalog(string mailbox, string folder)
        {
            _generateSpecificMailbox = mailbox;
            _generateSpecificFolder = folder;
            GenerateCatalog();
        }

        private void GenerateEachFolderCatalog(IMailboxData mailboxData,
           IFolderData folderData,
           Folder folder,
           IFolder folderOperator,
           IItem itemOperator,
           ICatalogDataAccess dataAccess,
           IDataConvert dataConvert,
           Stack<IFolderData> folderStack,
           int level = 0)
        {
            GenerateFolderStart(folderStack, mailboxData, folderData);
            bool hasError = true;
            try
            {
                if (level != 0) // root folder don't need save items;
                {
                    OnFolderProgressChanged(CatalogFolderProgressType.ChildItemStart, mailboxData, folderStack, folderData);
                    if (folder.TotalCount > 0)
                    {
                        OnFolderProgressChanged(CatalogFolderProgressType.GetChildItemStart, mailboxData, folderStack, folderData);
                        List<Item> folderItems = itemOperator.GetFolderItems(folder);
                        int itemCount = folderItems.Count;
                        OnFolderProgressChanged(CatalogFolderProgressType.GetChildItemsEnd, mailboxData, folderStack, folderData, new Process(-1, itemCount));

                        if (itemCount > 0)
                        {
                            int itemIndex = 0;

                            foreach (var item in folderItems)
                            {
                                itemIndex++;
                                bool itemHasError = true;

                                IItemData itemData = dataConvert.Convert(item);
                                OnFolderProgressChanged(CatalogFolderProgressType.ProcessingItemStart, mailboxData, folderStack, folderData, new Process(itemIndex, itemCount), itemData);
                                GenerateItemStart(itemData, mailboxData, folderStack, folderData);
                                try
                                {
                                    OnItemProgressChanged(CatalogItemProgressType.SaveItemStart, mailboxData, folderStack, folderData, itemData);
                                    dataAccess.SaveItem(itemData, mailboxData, folderData);
                                    OnItemProgressChanged(CatalogItemProgressType.SaveItemEnd, mailboxData, folderStack, folderData, itemData);

                                    bool isCheckExist = false;
                                    bool isExist = false;
                                    var itemIsNew = itemOperator.IsItemNew(item, LastCatalogTime, StartTime);
                                    if (itemIsNew)
                                    {
                                        OnItemProgressChanged(CatalogItemProgressType.SaveItemContentStart, mailboxData, folderStack, folderData, itemData);
                                        dataAccess.SaveItemContent(itemData, StartTime, isCheckExist, isExist);
                                        OnItemProgressChanged(CatalogItemProgressType.SaveItemContentEnd, mailboxData, folderStack, folderData, itemData);
                                    }
                                    else
                                    {
                                        OnItemProgressChanged(CatalogItemProgressType.SaveItemContentEndForExist, mailboxData, folderStack, folderData, itemData);
                                    }
                                    itemHasError = false;
                                }
                                finally
                                {
                                    GenerateItemEnd(itemData, mailboxData, folderStack, folderData, itemHasError);
                                    if (itemHasError)
                                        OnFolderProgressChanged(CatalogFolderProgressType.ProcessingItemEndWithError, mailboxData, folderStack, folderData, new Process(itemIndex, itemCount), itemData);
                                    else
                                        OnFolderProgressChanged(CatalogFolderProgressType.ProcessingItemEndNoError, mailboxData, folderStack, folderData, new Process(itemIndex, itemCount), itemData);
                                }
                            }
                        }
                        else
                        {
                            OnFolderProgressChanged(CatalogFolderProgressType.NoChildItem, mailboxData, folderStack, folderData);
                        }
                    }
                    OnFolderProgressChanged(CatalogFolderProgressType.ChildItemEnd, mailboxData, folderStack, folderData);
                }

                OnFolderProgressChanged(CatalogFolderProgressType.ChildFolderStart, mailboxData, folderStack, folderData);
                if (folder.ChildFolderCount > 0)
                {
                    OnFolderProgressChanged(CatalogFolderProgressType.GetChildFoldersStart, mailboxData, folderStack, folderData);
                    List<Folder> childFolders = folderOperator.GetChildFolder(folder);
                    int childFolderIndex = 0;
                    int childFolderCount = childFolders.Count;
                    OnFolderProgressChanged(CatalogFolderProgressType.GetChildFoldersEnd, mailboxData, folderStack, folderData, null, null, new Process(-1, childFolderCount));

                    foreach (var childFolder in childFolders)
                    {
                        childFolderIndex++;
                        if (!folderOperator.IsFolderNeedGenerateCatalog(childFolder))
                        {
                            OnFolderProgressChanged(CatalogFolderProgressType.ChildFolderSkip, mailboxData, folderStack, folderData, null, null, new Process(childFolderIndex, childFolderCount));
                            continue;
                        }

                        if (!IsNeedGenerateFolder(childFolder, level))
                        {
                            OnFolderProgressChanged(CatalogFolderProgressType.ChildFolderSkip, mailboxData, folderStack, folderData, null, null, new Process(childFolderIndex, childFolderCount));
                            continue;
                        }

                        IFolderData childFolderData = dataConvert.Convert(childFolder);

                        OnFolderProgressChanged(CatalogFolderProgressType.SaveFolderStart, mailboxData, folderStack, folderData, null, null, new Process(childFolderIndex, childFolderCount));
                        dataAccess.SaveFolder(childFolderData, mailboxData, folderData);
                        OnFolderProgressChanged(CatalogFolderProgressType.SaveFolderEnd, mailboxData, folderStack, folderData, null, null, new Process(childFolderIndex, childFolderCount));

                        folderStack.Push(childFolderData);
                        GenerateEachFolderCatalog(mailboxData, childFolderData, childFolder, folderOperator, itemOperator, dataAccess, dataConvert, folderStack, level + 1);
                        folderStack.Pop();
                    }
                }
                else
                {
                    OnFolderProgressChanged(CatalogFolderProgressType.NoChildFolder, mailboxData, folderStack, folderData);
                }
                OnFolderProgressChanged(CatalogFolderProgressType.ChildFolderEnd, mailboxData, folderStack, folderData);

                hasError = false;
            }
            finally
            {
                GenerateFolderEnd(folderStack, mailboxData, folderData, hasError);
            }
        }

        public event EventHandler<CatalogProgressArgs> ProgressChanged;
        public event EventHandler<CatalogMailboxArgs> MailboxProgressChanged;
        public event EventHandler<CatalogFolderArgs> FolderProgressChanged;
        public event EventHandler<CatalogItemArgs> ItemProgressChanged;
        public event EventHandler<EventExceptionArgs> ExceptionThrowed;

        private void OnProgressChanged(CatalogProgressType type, Process mailboxProcess = null, IMailboxData mailbox = null)
        {
            if(ProgressChanged != null)
            {
                try {
                    ProgressChanged(this, new CatalogProgressArgs(type, mailboxProcess, mailbox));
                }
                catch(Exception e)
                {
                    OnExceptionThrowed(e);
                }
            }
        }

        private void OnMailboxProgressChanged(CatalogMailboxProgressType type, IMailboxData mailbox = null)
        {
            if(MailboxProgressChanged != null)
            {
                try
                {
                    MailboxProgressChanged(this, new CatalogMailboxArgs(type, mailbox));
                }
                catch(Exception e)
                {
                    OnExceptionThrowed(e);
                }
            }
        }

        private void OnFolderProgressChanged(CatalogFolderProgressType type,
            IMailboxData currentMailbox,
            Stack<IFolderData> folderStack = null,
            IFolderData currentFolder = null,
            Process itemProcess = null,
            IItemData currentItem = null,
            Process childFolderProcess = null)
        {
            if (FolderProgressChanged != null)
            {
                try
                {
                    FolderProgressChanged(this, new CatalogFolderArgs(type, currentMailbox, folderStack, currentFolder, itemProcess, currentItem, childFolderProcess));
                }
                catch (Exception e)
                {
                    OnExceptionThrowed(e);
                }
            }
        }

        private void OnItemProgressChanged(CatalogItemProgressType type, IMailboxData mailbox, Stack<IFolderData> folderStack, IFolderData folder, IItemData currentItem = null)
        {
            if (ItemProgressChanged != null)
            {
                try
                {
                    ItemProgressChanged(this, new CatalogItemArgs(type, mailbox, folderStack, folder,  currentItem));
                }
                catch (Exception e)
                {
                    OnExceptionThrowed(e);
                }
            }
        }

        private void OnExceptionThrowed(Exception e)
        {
            if(ExceptionThrowed != null)
            {
                try
                {
                    ExceptionThrowed(this, new EventExceptionArgs(e));
                }
                catch(Exception ex)
                {

                }
            }
        }

        public void MailboxGenerateStart(IMailboxData mailbox, int mailboxIndex, int mailboxCount)
        {
            OnProgressChanged(CatalogProgressType.GRTForMailboxStart, new Process(mailboxIndex, mailboxCount), mailbox);
            OnMailboxProgressChanged(CatalogMailboxProgressType.Start, mailbox);

            _serviceContext.CurrentMailbox = mailbox.MailAddress;
            MailboxCacheManager.CacheManager.ReleaseCache();
        }

        public void MailboxGenerateEnd(IMailboxData mailbox, bool hasError, int mailboxIndex, int mailboxCount)
        {
            if (!hasError)
            {
                MailboxCacheManager.CacheManager.ReleaseCache(true);
                OnProgressChanged(CatalogProgressType.GRTForMailboxEndWithNoError, new Process(mailboxIndex, mailboxCount), mailbox);
                OnMailboxProgressChanged(CatalogMailboxProgressType.EndWithNoError, mailbox);
            }
            else
            {
                MailboxCacheManager.CacheManager.ReleaseCache();
                OnProgressChanged(CatalogProgressType.GRTForMailboxEndWithError, new Process(mailboxIndex, mailboxCount), mailbox);
                OnMailboxProgressChanged(CatalogMailboxProgressType.EndWithError, mailbox);
            }
            _serviceContext.CurrentMailbox = string.Empty;

        }

        public void GenerateCatalogStart()
        {
            CatalogFactory.Instance.NewCatalogDataAccess().BeginTransaction();
            OnProgressChanged(CatalogProgressType.Start);
        }

        public void GenerateCatalogEnd(bool isFinished)
        {
            CatalogFactory.Instance.NewCatalogDataAccess().EndTransaction(isFinished);
            if(isFinished)
                OnProgressChanged(CatalogProgressType.EndWithNoError);
            else
                OnProgressChanged(CatalogProgressType.EndWithError);
        }

        private void GenerateItemEnd(IItemData item,IMailboxData mailboxData,Stack<IFolderData> folderStack,IFolderData folderData, bool itemHasError)
        {
            if (itemHasError)
                OnItemProgressChanged(CatalogItemProgressType.EndWithError,mailboxData,folderStack,folderData, item);
            else
                OnItemProgressChanged(CatalogItemProgressType.EndWithNoError, mailboxData, folderStack, folderData, item);
        }

        private void GenerateItemStart(IItemData item, IMailboxData mailboxData, Stack<IFolderData> folderStack, IFolderData folderData)
        {
            OnItemProgressChanged(CatalogItemProgressType.Start, mailboxData, folderStack, folderData, item);
        }

        private void GenerateFolderEnd(Stack<IFolderData> folderStack, IMailboxData mailboxData, IFolderData folderData, bool hasError)
        {
            if (!hasError)
                OnFolderProgressChanged(CatalogFolderProgressType.EndWithNoError, mailboxData, folderStack, folderData);
            else
                OnFolderProgressChanged(CatalogFolderProgressType.EndWithError, mailboxData, folderStack, folderData);
        }

        private void GenerateFolderStart(Stack<IFolderData> folderStack, IMailboxData mailboxData, IFolderData folderData)
        {
            OnFolderProgressChanged(CatalogFolderProgressType.Start, mailboxData, folderStack, folderData);
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

        private bool IsNeedGenerateMailbox(string mailboxAddress)
        {
            if (!string.IsNullOrEmpty(_generateSpecificMailbox) && mailboxAddress.ToLower() != _generateSpecificMailbox.ToLower())
                return false;
            return true;
        }

        private bool IsNeedGenerateFolder(Folder childFolder, int level)
        {
            if (level == 0)
            {
                if (!string.IsNullOrEmpty(_generateSpecificFolder) && _generateSpecificFolder != childFolder.DisplayName)
                    return false;
            }
            return true;
        }

        private string _generateSpecificMailbox;

        private string _generateSpecificFolder;

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

            public IMailboxData Clone()
            {
                return new MailboxInfo(DisplayName, MailAddress)
                {
                    Location = Location,
                    RootFolderId = RootFolderId
                };
            }
        }
    }
}