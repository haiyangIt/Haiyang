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
using EwsFrame.Manager.Impl;
using EwsFrame.Manager.IF;

namespace DataProtectImpl
{
    /// <summary>
    /// If we need generate catalog concurrently, we need control the granularity to mailbox level.
    /// And we need use the database to record the current generate catalog job information.
    /// </summary>
    public class CatalogService : ArcJobBase, ICatalogService, ICatalogJob, IDisposable
    {
        public string Organization { get { return AdminInfo.OrganizationName; } }

        private IFilterItem Filter { get; set; }

        public CatalogService(string adminUserName, string adminPassword, string domainName, string organizationName)
        {
            if (string.IsNullOrEmpty(adminUserName))
                throw new ArgumentException("user name is null", "adminUserName");
            if (string.IsNullOrEmpty(adminPassword))
                throw new ArgumentException("password is null", "adminPassword");

            LatestStatus = new CatalogProgressArgs(CatalogProgressType.Init);

            AdminInfo = new OrganizationAdminInfo();
            AdminInfo.UserName = adminUserName;
            AdminInfo.UserPassword = adminPassword;
            AdminInfo.UserDomain = domainName;
            AdminInfo.OrganizationName = organizationName;

            _serviceContext = EwsFrame.ServiceContext.NewServiceContext(AdminInfo.UserName, AdminInfo.UserPassword, AdminInfo.UserDomain, AdminInfo.OrganizationName, TaskType.Catalog);
        }

        private int GetFolderCount()
        {
            if (Filter is IFilterItemWithMailbox)
            {
                return ((IFilterItemWithMailbox)Filter).GetFolderCount();
            }
            throw new NotSupportedException("Cannot get folders count.");
        }

        private Process MailboxProgress = null;
        private Process FolderProgress = null;
        private Process ItemProgress = null;
        /// <summary>
        /// 
        /// </summary>

        #region IItemBase

        public string Id
        {
            get
            {
                return "0";
            }

            set
            {

            }
        }

        public string DisplayName
        {
            get
            {
                return CatalogJobName;
            }

            set
            {
                CatalogJobName = value;
            }
        }

        public ItemKind ItemKind
        {
            get
            {
                return ItemKind.Organization;
            }

            set
            {

            }
        }
        #endregion

        #region ICatalogServiceEvent
        public event EventHandler<CatalogProgressArgs> ProgressChanged;

        public event EventHandler<EventExceptionArgs> ExceptionThrowed;
        #endregion

        #region ICatalogService
        public CatalogProgressArgs LatestStatus { get; private set; }
        public DateTime StartTime { get; set; }

        private void InitCatalogJobName()
        {
            if(string.IsNullOrEmpty(_catalogJobName))
                _catalogJobName = string.Format("{0} Catalog Job {1}", AdminInfo.OrganizationName, StartTime.ToString("yyyyMMddHHmmss"));
        }
        private string _catalogJobName;
        public string CatalogJobName
        {
            get
            {
                return _catalogJobName;
            }
            set
            {
                _catalogJobName = value;
            }
        }

        private IServiceContext _serviceContext;
        public IServiceContext ServiceContext
        {
            get
            {
                return _serviceContext;
            }
        }

        public OrganizationAdminInfo AdminInfo { get; private set; }


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

                        //if (IsNeedGenerateMailbox(address) && address.ToLower() == "haiyang.ling@arcserve.com") // todo remove the specific mail address.
                        result.Add(new MailboxInfo(displayName, address));
                    }
                    return result;
                }
            }
        }

        private List<IMailboxData> GetAllUserMailboxFromFilter()
        {
            if (Filter is IFilterItemWithMailbox)
            {
                return ((IFilterItemWithMailbox)Filter).GetAllMailbox();
            }
            return GetAllUserMailbox();
        }

        public List<IFolderData> GetFolder(string mailbox, string parentId, bool containRootFolder)
        {
            ServiceContext.CurrentMailbox = mailbox;
            IMailbox obj = CatalogFactory.Instance.NewMailboxOperatorImpl();
            obj.ConnectMailbox(ServiceContext.Argument, mailbox);
            IFolder folderOper = CatalogFactory.Instance.NewFolderOperatorImpl(obj.CurrentExchangeService);
            List<Folder> folders = null;
            if (string.IsNullOrEmpty(parentId) || parentId == "0")
            {
                var rootFolder = folderOper.GetRootFolder();
                if (containRootFolder)
                {
                    folders = new List<Folder>() { rootFolder };
                }
                else
                {
                    folders = folderOper.GetChildFolder(rootFolder);
                }
            }
            else
            {
                folders = folderOper.GetChildFolder(parentId);
            }

            List<IFolderData> folderdatas = new List<IFolderData>(folders.Count);
            IDataConvert dataConvert = CatalogFactory.Instance.NewDataConvert();
            dataConvert.OrganizationName = AdminInfo.OrganizationName;
            dataConvert.StartTime = DateTime.Now;

            foreach (var folder in folders)
            {
                if (folderOper.IsFolderNeedGenerateCatalog(folder))
                {
                    IFolderData folderData = dataConvert.Convert(folder, mailbox);
                    folderdatas.Add(folderData);
                }
            }
            return folderdatas;
        }

        public void GenerateCatalog(IFilterItem filter)
        {
            Filter = filter;
            GenerateCatalog();
        }

        public void GenerateCatalog()
        {
            StartTime = DateTime.Now;
            if (Filter == null)
            {
                Filter = new NoFilter();
            }

            GenerateCatalogStart();
            bool isFinished = false;
            try
            {
                OnProgressChanged(CatalogProgressType.GetAllMailboxStart);
                List<IMailboxData> allUserMailbox = GetAllUserMailboxFromFilter();
                OnProgressChanged(CatalogProgressType.GetAllMailboxEnd);
                ICatalogDataAccess dataAccess = NewDataAccessInstance();
                IDataConvert dataConvert = NewDataConvertInstance();
                dataConvert.StartTime = StartTime;
                dataConvert.OrganizationName = AdminInfo.OrganizationName;

                int mailboxIndex = 0;
                int totalMailboxCount = allUserMailbox.Count;
                Stack<IFolderData> folderStack = new Stack<IFolderData>(4);
                foreach (IMailboxData userMailbox in allUserMailbox)
                {
                    mailboxIndex++;
                    MailboxProgress = new Process(mailboxIndex, allUserMailbox.Count);

                    if (IsJobNeedCanceledOrEnded())
                        break;

                    if (Filter.IsFilterMailbox(userMailbox))
                    {
                        OnProgressChanged(CatalogMailboxProgressType.SkipMailbox, MailboxProgress, userMailbox);
                        continue;
                    }

                    MailboxGenerateStart(userMailbox);

                    bool hasError = true;
                    try
                    {
                        IMailbox mailboxOperator = NewMailboxOperatorInstance();

                        OnProgressChanged(CatalogMailboxProgressType.ConnectMailboxStart, MailboxProgress, userMailbox);
                        _serviceContext.CurrentMailbox = userMailbox.MailAddress;
                        mailboxOperator.ConnectMailbox(ServiceContext.Argument, userMailbox.MailAddress);
                        OnProgressChanged(CatalogMailboxProgressType.ConnectMailboxEnd, MailboxProgress, userMailbox);

                        if (IsJobNeedCanceledOrEnded())
                            break;

                        IFolder folderOperator = CatalogFactory.Instance.NewFolderOperatorImpl(mailboxOperator.CurrentExchangeService);

                        OnProgressChanged(CatalogMailboxProgressType.GetRootFolderStart, MailboxProgress, userMailbox);
                        Folder rootFolder = folderOperator.GetRootFolder();

                        OnProgressChanged(CatalogMailboxProgressType.GetRootFolderEnd, MailboxProgress, userMailbox);

                        if (IsJobNeedCanceledOrEnded())
                            break;

                        userMailbox.RootFolderId = rootFolder.Id.UniqueId;
                        IMailboxData mailboxData = dataConvert.Convert(userMailbox);

                        IFolderData rootFolderData = dataConvert.Convert(rootFolder, userMailbox.MailAddress);
                        IItem itemOperator = CatalogFactory.Instance.NewItemOperatorImpl(folderOperator.CurrentExchangeService, dataAccess);
                        //dataAccess.SaveFolder(rootFolderData, mailboxData, null); // root folder don't need save.

                        int folderCount = GetFolderCount();
                        folderStack.Push(rootFolderData);
                        GenerateEachFolderCatalog(mailboxData,
                            rootFolderData, rootFolder,
                            folderOperator, itemOperator,
                            dataAccess,
                            dataConvert, folderCount, 0,
                            folderStack);
                        folderStack.Pop();

                        OnProgressChanged(CatalogMailboxProgressType.SaveMailboxStart, MailboxProgress, userMailbox);
                        mailboxData.ChildFolderCount = rootFolderData.ChildFolderCount;
                        dataAccess.SaveMailbox(mailboxData);
                        OnProgressChanged(CatalogMailboxProgressType.SaveMailboxEnd, MailboxProgress, userMailbox);

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
            catch (Exception e)
            {
                OnExceptionThrowed(e);
                throw new CatalogException("Catalog Failure, please view inner Exception.", e);
            }
            finally
            {
                GenerateCatalogEnd(isFinished);
            }
        }

        private void GenerateEachFolderCatalog(IMailboxData mailboxData,
           IFolderData folderData,
           Folder folder,
           IFolder folderOperator,
           IItem itemOperator,
           ICatalogDataAccess dataAccess,
           IDataConvert dataConvert,
           int totalFolderCount,
           int currentFolderIndex,
           Stack<IFolderData> folderStack,
           int level = 0)
        {
            FolderProgress = new Process(currentFolderIndex, totalFolderCount);
            GenerateFolderStart(folderStack, mailboxData, folderData);
            bool hasError = true;
            try
            {
                if (level != 0) // root folder don't need save items;
                {
                    OnProgressChanged(CatalogFolderProgressType.ChildItemStart, MailboxProgress, mailboxData, FolderProgress, folderData);
                    if (folder.TotalCount > 0)
                    {
                        OnProgressChanged(CatalogFolderProgressType.GetChildItemStart, MailboxProgress, mailboxData, FolderProgress, folderData);
                        List<Item> folderItems = itemOperator.GetFolderItems(folder);
                        int itemCount = folderItems.Count;
                        OnProgressChanged(CatalogFolderProgressType.GetChildItemsEnd, MailboxProgress, mailboxData, FolderProgress, folderData);

                        if (itemCount > 0)
                        {
                            int itemIndex = 0;

                            foreach (var item in folderItems)
                            {
                                itemIndex++;
                                ItemProgress = new Process(itemIndex, itemCount);
                                IItemData itemData = dataConvert.Convert(item);
                                if (Filter.IsFilterItem(itemData, mailboxData, folderStack))
                                {
                                    OnProgressChanged(CatalogItemProgressType.SkipItem, MailboxProgress, mailboxData, FolderProgress, folderData, ItemProgress, itemData);
                                    continue;
                                }

                                bool itemHasError = true;

                                OnProgressChanged(CatalogFolderProgressType.ProcessingItemStart, MailboxProgress, mailboxData, FolderProgress, folderData);
                                GenerateItemStart(itemData, mailboxData, folderStack, folderData);
                                try
                                {
                                    OnProgressChanged(CatalogItemProgressType.SaveItemStart, MailboxProgress, mailboxData, FolderProgress, folderData, ItemProgress, itemData);
                                    dataAccess.SaveItem(itemData, mailboxData, folderData);
                                    folderData.ChildItemCount++;
                                    OnProgressChanged(CatalogItemProgressType.SaveItemEnd, MailboxProgress, mailboxData, FolderProgress, folderData, ItemProgress, itemData);

                                    bool isCheckExist = false;
                                    bool isExist = false;
                                    var itemIsNew = itemOperator.IsItemNew(item, DateTime.MinValue, StartTime);
                                    if (itemIsNew)
                                    {
                                        OnProgressChanged(CatalogItemProgressType.SaveItemContentStart, MailboxProgress, mailboxData, FolderProgress, folderData, ItemProgress, itemData);
                                        dataAccess.SaveItemContent(itemData, StartTime, true, !itemIsNew);
                                        OnProgressChanged(CatalogItemProgressType.SaveItemContentEnd, MailboxProgress, mailboxData, FolderProgress, folderData, ItemProgress, itemData);
                                    }
                                    else
                                    {
                                        OnProgressChanged(CatalogItemProgressType.SaveItemContentEndForExist, MailboxProgress, mailboxData, FolderProgress, folderData, ItemProgress, itemData);
                                    }
                                    itemHasError = false;
                                }
                                finally
                                {
                                    GenerateItemEnd(itemData, mailboxData, folderStack, folderData, itemHasError);
                                    if (itemHasError)
                                        OnProgressChanged(CatalogFolderProgressType.ProcessingItemEndWithError, MailboxProgress, mailboxData, FolderProgress, folderData);
                                    else
                                        OnProgressChanged(CatalogFolderProgressType.ProcessingItemEndNoError, MailboxProgress, mailboxData, FolderProgress, folderData);
                                }
                            }
                        }
                        else
                        {
                            OnProgressChanged(CatalogFolderProgressType.NoChildItem, MailboxProgress, mailboxData, FolderProgress, folderData);
                        }
                    }
                    OnProgressChanged(CatalogFolderProgressType.ChildItemEnd, MailboxProgress, mailboxData, FolderProgress, folderData);
                }

                OnProgressChanged(CatalogFolderProgressType.ChildFolderStart, MailboxProgress, mailboxData, FolderProgress, folderData);
                if (folder.ChildFolderCount > 0)
                {
                    OnProgressChanged(CatalogFolderProgressType.GetChildFoldersStart, MailboxProgress, mailboxData, FolderProgress, folderData);
                    List<Folder> childFolders = folderOperator.GetChildFolder(folder);
                    int childFolderIndex = 0;
                    int childFolderCount = childFolders.Count;
                    OnProgressChanged(CatalogFolderProgressType.GetChildFoldersEnd, MailboxProgress, mailboxData, FolderProgress, folderData);

                    foreach (var childFolder in childFolders)
                    {
                        childFolderIndex++;

                        IFolderData childFolderData = dataConvert.Convert(childFolder, mailboxData.MailAddress);
                        if (!folderOperator.IsFolderNeedGenerateCatalog(childFolder))
                        {
                            OnProgressChanged(CatalogFolderProgressType.ChildFolderSkip, MailboxProgress, mailboxData, FolderProgress, folderData);
                            continue;
                        }

                        if (Filter.IsFilterFolder(childFolderData, mailboxData, folderStack))
                        {
                            OnProgressChanged(CatalogFolderProgressType.ChildFolderSkip, MailboxProgress, mailboxData, FolderProgress, folderData);
                            continue;
                        }

                        folderStack.Push(childFolderData);
                        GenerateEachFolderCatalog(mailboxData, childFolderData, childFolder,
                            folderOperator, itemOperator, dataAccess, dataConvert, totalFolderCount, currentFolderIndex + 1,
                            folderStack, level + 1);
                        //FolderProgress = new Process(currentFolderIndex, totalFolderCount);
                        folderStack.Pop();

                        OnProgressChanged(CatalogFolderProgressType.SaveFolderStart, MailboxProgress, mailboxData, FolderProgress, folderData);
                        folderData.ChildFolderCount++;
                        dataAccess.SaveFolder(childFolderData, mailboxData, folderData);
                        OnProgressChanged(CatalogFolderProgressType.SaveFolderEnd, MailboxProgress, mailboxData, FolderProgress, folderData);
                    }
                }
                else
                {
                    OnProgressChanged(CatalogFolderProgressType.NoChildFolder, MailboxProgress, mailboxData, FolderProgress, folderData);
                }
                OnProgressChanged(CatalogFolderProgressType.ChildFolderEnd, MailboxProgress, mailboxData, FolderProgress, folderData);

                hasError = false;
            }
            finally
            {
                GenerateFolderEnd(folderStack, mailboxData, folderData, hasError);
            }
        }
        #endregion

        #region Progress
        private void OnProgressChanged(CatalogMailboxProgressType type, Process mailboxProcess, IMailboxData mailbox)
        {
            if (ProgressChanged != null)
            {
                try
                {
                    //OnMailboxProgressChanged(type, mailbox);
                    LatestStatus = new CatalogProgressArgs(type, mailboxProcess, mailbox);
                    ProgressChanged(this, LatestStatus);
                }
                catch (Exception e)
                {
                    OnExceptionThrowed(e);
                }
            }
        }

        private void OnProgressChanged(CatalogProgressType type)
        {
            if (ProgressChanged != null)
            {
                try
                {
                    LatestStatus = new CatalogProgressArgs(type);
                    ProgressChanged(this, LatestStatus);
                }
                catch (Exception e)
                {
                    OnExceptionThrowed(e);
                }
            }
        }

        private void OnProgressChanged(CatalogFolderProgressType type, Process mailboxProcess, IMailboxData mailbox,
            Process folderProcess, IFolderData folder)
        {
            if (ProgressChanged != null)
            {
                try
                {
                    LatestStatus = new CatalogProgressArgs(type, mailboxProcess, mailbox, folderProcess, folder);
                    ProgressChanged(this, LatestStatus);
                }
                catch (Exception e)
                {
                    OnExceptionThrowed(e);
                }
            }
        }

        private void OnProgressChanged(CatalogItemProgressType type, Process mailboxProcess, IMailboxData mailbox,
            Process folderProcess, IFolderData folder,
            Process itemProcess, IItemData item)
        {
            if (ProgressChanged != null)
            {
                try
                {
                    LatestStatus = new CatalogProgressArgs(type, mailboxProcess, mailbox, folderProcess, folder, itemProcess, item);
                    ProgressChanged(this, LatestStatus);
                }
                catch (Exception e)
                {
                    OnExceptionThrowed(e);
                }
            }
        }

        private void OnExceptionThrowed(Exception e)
        {
            if (ExceptionThrowed != null)
            {
                try
                {
                    ExceptionThrowed(this, new EventExceptionArgs(e));
                }
                catch (Exception ex)
                {

                }
            }
        }

        public void MailboxGenerateStart(IMailboxData mailbox)
        {
            // OnProgressChanged(CatalogProgressType.GRTForMailboxStart,  mailbox);
            OnProgressChanged(CatalogMailboxProgressType.Start, MailboxProgress, mailbox);

            _serviceContext.CurrentMailbox = mailbox.MailAddress;
            MailboxCacheManager.CacheManager.ReleaseCache();
        }

        public void MailboxGenerateEnd(IMailboxData mailbox, bool hasError)
        {
            if (!hasError)
            {
                MailboxCacheManager.CacheManager.ReleaseCache(true);
                OnProgressChanged(CatalogMailboxProgressType.EndWithNoError, MailboxProgress, mailbox);
            }
            else
            {
                MailboxCacheManager.CacheManager.ReleaseCache();
                OnProgressChanged(CatalogMailboxProgressType.EndWithError, MailboxProgress, mailbox);
            }
            ServiceContext.CurrentMailbox = string.Empty;
        }

        public void GenerateCatalogStart()
        {
            ServiceContext.DataAccessObj.BeginTransaction();
            OnProgressChanged(CatalogProgressType.Start);
        }

        public void GenerateCatalogEnd(bool isFinished)
        {
            ServiceContext.DataAccessObj.EndTransaction(isFinished);
            if (isFinished)
                OnProgressChanged(CatalogProgressType.EndWithNoError);
            else
                OnProgressChanged(CatalogProgressType.EndWithError);
        }

        private void GenerateItemEnd(IItemData item, IMailboxData mailboxData, Stack<IFolderData> folderStack, IFolderData folderData, bool itemHasError)
        {
            if (itemHasError)
                OnProgressChanged(CatalogItemProgressType.EndWithError, MailboxProgress, mailboxData, FolderProgress, folderData, ItemProgress, item);
            //OnItemProgressChanged(CatalogItemProgressType.EndWithError,mailboxData,folderStack,folderData, item);
            else
                OnProgressChanged(CatalogItemProgressType.EndWithNoError, MailboxProgress, mailboxData, FolderProgress, folderData, ItemProgress, item);
            // OnItemProgressChanged(CatalogItemProgressType.EndWithNoError, mailboxData, folderStack, folderData, item);
        }

        private void GenerateItemStart(IItemData item, IMailboxData mailboxData, Stack<IFolderData> folderStack, IFolderData folderData)
        {
            OnProgressChanged(CatalogItemProgressType.Start, MailboxProgress, mailboxData, FolderProgress, folderData, ItemProgress, item);
            //OnItemProgressChanged(CatalogItemProgressType.Start, mailboxData, folderStack, folderData, item);
        }

        private void GenerateFolderEnd(Stack<IFolderData> folderStack, IMailboxData mailboxData, IFolderData folderData, bool hasError)
        {
            if (!hasError)
                OnProgressChanged(CatalogFolderProgressType.EndWithNoError, MailboxProgress, mailboxData, FolderProgress, folderData);
            else
                OnProgressChanged(CatalogFolderProgressType.EndWithError, MailboxProgress, mailboxData, FolderProgress, folderData);
        }

        private void GenerateFolderStart(Stack<IFolderData> folderStack, IMailboxData mailboxData, IFolderData folderData)
        {
            OnProgressChanged(CatalogFolderProgressType.Start, MailboxProgress, mailboxData, FolderProgress, folderData);
        }
        #endregion

        #region IArcJob
        protected override void InternalRun()
        {
            throw new NotImplementedException();
        }

        public override ArcJobType JobType
        {
            get
            {
                return ArcJobType.Backup;
            }
        }

        public override string JobName
        {
            get
            {

                return _catalogJobName;
            }

            set
            {
                _catalogJobName = value;
            }
        }
        #endregion

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
            return (ICatalogDataAccess)ServiceContext.DataAccessObj;
        }

        public IDataConvert NewDataConvertInstance()
        {
            var result = CatalogFactory.Instance.NewDataConvert();
            return result;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        class MailboxInfo : IMailboxData
        {
            public MailboxInfo(string displayName, string mailboxAddress)
            {
                DisplayName = displayName;
                MailAddress = mailboxAddress;
            }

            public int ChildFolderCount
            {
                get; set;
            }

            public string DisplayName
            {
                get; set;
            }

            public string Id
            {
                get
                {
                    return RootFolderId;
                }

                set
                {
                    RootFolderId = value;
                }
            }

            public ItemKind ItemKind
            {
                get
                {
                    return ItemKind.Mailbox;
                }

                set
                {
                }
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

    class NoFilter : IFilterItem
    {
        public NoFilter()
        {

        }

        public bool IsFilterFolder(IFolderData currentFolder, IMailboxData mailbox, Stack<IFolderData> folders)
        {
            return false;
        }

        public bool IsFilterItem(IItemData item, IMailboxData mailbox, Stack<IFolderData> folders)
        {
            return false;
        }

        public bool IsFilterMailbox(IMailboxData mailbox)
        {
            return false;
        }

    }
}