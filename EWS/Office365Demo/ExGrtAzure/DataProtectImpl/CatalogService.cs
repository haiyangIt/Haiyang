using DataProtectInterface;
using EwsDataInterface;
using EwsFrame;
using EwsFrame.Cache;
using EwsServiceInterface;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security;
using DataProtectInterface.Event;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Configuration;
using System.Net.Mail;
using System.Text;
using System.Collections.Concurrent;
using EwsFrame.Util;
using LogInterface;
using EwsService.Common;

namespace DataProtectImpl
{
    /// <summary>
    /// If we need generate catalog concurrently, we need control the granularity to mailbox level.
    /// And we need use the database to record the current generate catalog job information.
    /// </summary>
    public class CatalogService : ICatalogService, ICatalogJob, IDataProtectProgress
    {
        //public DateTime LastCatalogTime { get; set; }

        private DateTime _startTime = DateTime.MinValue;
        public DateTime StartTime
        {
            get
            {
                return _startTime;
            }
            set
            {
                _startTime = value;
            }
        }

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

        private IFilterItem Filter { get; set; }

        private IServiceContext _serviceContext;
        public IServiceContext ServiceContext
        {
            get
            {
                return _serviceContext;
            }
        }

        public OrganizationAdminInfo AdminInfo { get; private set; }

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

            _serviceContext = EwsFrame.ServiceContext.NewServiceContext(AdminInfo.UserName, AdminInfo.UserPassword, AdminInfo.UserDomain, AdminInfo.OrganizationName, TaskType.Catalog);
            LatestInformation = new ConcurrentQueue<string>();
            LogFactory.LogInstance.WriteLogMsgEvent += DoEventHandler;
        }

        private void DoEventHandler(object serder, string msg)
        {
            while (LatestInformation.Count > 10)
            {
                string result;
                LatestInformation.TryDequeue(out result);
            }
            LatestInformation.Enqueue(msg);
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

        private IEwsAdapter EwsAdapter { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public void GenerateCatalog()
        {
            ThreadData.Information = (-1).ToString("D8");
            StartTime = DateTime.Now;
            if (Filter == null)
            {
                Filter = new NoFilter();
            }

            GenerateCatalogStart();
            bool isFinished = false;
            try
            {
                IDataConvert dataConvert = NewDataConvertInstance();
                dataConvert.StartTime = StartTime;
                dataConvert.OrganizationName = AdminInfo.OrganizationName;
                EwsAdapter = CatalogFactory.Instance.NewEwsAdapter();
                EwsAdapter.DataConvert = dataConvert;

                ICatalogDataAccess dataAccess = NewDataAccessInstance();
                dataAccess.OtherObj = EwsAdapter;

                OnProgressChanged(CatalogProgressType.GetAllMailboxStart);
                List<IMailboxData> allUserMailbox = GetAllUserMailboxFromFilter();
                OnProgressChanged(CatalogProgressType.GetAllMailboxEnd, new Process(-1, allUserMailbox.Count));

                int mailboxIndex = 0;
                int totalMailboxCount = allUserMailbox.Count;
                Stack<IFolderData> folderStack = new Stack<IFolderData>(4);
                foreach (IMailboxData userMailbox in allUserMailbox)
                {
                    mailboxIndex++;
                    MailboxGenerateStart(userMailbox, mailboxIndex, totalMailboxCount);

                    if (Filter.IsFilterMailbox(userMailbox))
                    {
                        OnMailboxProgressChanged(CatalogMailboxProgressType.SkipMailbox, userMailbox);
                        continue;
                    }

                    bool hasError = true;
                    try
                    {
                        OnMailboxProgressChanged(CatalogMailboxProgressType.ConnectMailboxStart, userMailbox);
                        ServiceContext.CurrentMailbox = userMailbox.MailAddress;
                        EwsAdapter.ConnectMailbox(ServiceContext.Argument, userMailbox.MailAddress);
                        OnMailboxProgressChanged(CatalogMailboxProgressType.ConnectMailboxEnd, userMailbox);


                        OnMailboxProgressChanged(CatalogMailboxProgressType.GetRootFolderStart, userMailbox);
                        IFolderData rootFolder = EwsAdapter.GetRootFolder();

                        OnMailboxProgressChanged(CatalogMailboxProgressType.GetRootFolderEnd, userMailbox);

                        userMailbox.RootFolderId = rootFolder.Id;
                        IMailboxData mailboxData = dataConvert.Convert(userMailbox);

                        IFolderData rootFolderData = rootFolder;
                        //dataAccess.SaveFolder(rootFolderData, mailboxData, null); // root folder don't need save.

                        folderStack.Push(rootFolderData);
                        GenerateEachFolderCatalog(mailboxData,
                            rootFolderData,
                            dataAccess,
                            dataConvert,
                            folderStack);
                        folderStack.Pop();

                        //mailboxData.ChildFolderCount = rootFolderData.ChildFolderCount;
                        //dataAccess.UpdateMailboxChildFolderCount(mailboxData, StartTime);

                        OnMailboxProgressChanged(CatalogMailboxProgressType.SaveMailboxStart, userMailbox);
                        mailboxData.ChildFolderCount = rootFolderData.ChildFolderCount;
                        dataAccess.SaveMailbox(mailboxData);
                        OnMailboxProgressChanged(CatalogMailboxProgressType.SaveMailboxEnd, userMailbox);

                        hasError = false;
                    }

                    finally
                    {
                        ServiceContext.DataAccessObj.SaveChanges();
                        MailboxGenerateEnd(userMailbox, hasError, mailboxIndex, totalMailboxCount);
                    }
                }
                ICatalogJob job = dataConvert.Convert(this);
                dataAccess.SaveCatalogJob(job);

                isFinished = true;
                //SendMail("complete success", null);
            }
            catch (Exception e)
            {
                OnExceptionThrowed(e);

                System.Diagnostics.Trace.TraceError(e.GetExceptionDetail());
                var timeSpan = DateTime.Now - StartTime;
                LogFactory.LogInstance.WriteLog(LogInterface.LogLevel.INFO, "Job failed",
                    "Total {0} folders {1} items's size {2} bytes actual size {3} bytes cost {4} minutes",
                    AllDealedFolderIndex, AllDealedItemIndex, MailboxSize, ActualSize, timeSpan.TotalMinutes);

                LogFactory.LogInstance.WriteException(LogInterface.LogLevel.ERR, "InCompleted", e, e.Message);

                //SendMail("complete with error", e);
                throw new CatalogException("Catalog Failure, please view inner Exception.", e);
            }
            finally
            {
                ServiceContext.DataAccessObj.SaveChanges();
                var timeSpan = DateTime.Now - StartTime;
                LogFactory.LogInstance.WriteLog(LogInterface.LogLevel.INFO, "Job completed",
                    "Total {0} folders {1} items's size {2} bytes actual size {3} bytes cost {4} minutes",
                    AllDealedFolderIndex, AllDealedItemIndex, MailboxSize, ActualSize, timeSpan.TotalMinutes);
                _serviceContext.DataAccessObj.Dispose();
                GenerateCatalogEnd(isFinished);
            }
           
        }

        private void SendMail(string message, Exception e)
        {
            var client = Config.MailConfigInstance.Client();
            System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage();

            msg.To.Add("haiyang.ling@arcserve.com");

            msg.From = new MailAddress(Config.MailConfigInstance.Sender);
            msg.Subject = "Catalog completed";

            msg.Body = e != null ? GetExceptionString("Catalog failed", e, e.Message) : message;

            try
            {
                client.Send(msg);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.GetExceptionDetail());
            }
        }

        public static string GetExceptionString(string message, Exception exception, string exMsg)
        {
            StringBuilder sb = new StringBuilder();
            var curEx = exception;
            while (curEx != null)
            {
                if (curEx is AggregateException)
                {
                    sb.AppendLine(GetAggrateException(message, curEx as AggregateException, exMsg));
                }
                else
                {
                    sb.AppendLine(string.Join(blank, DateTime.Now.ToString("yyyyMMddHHmmss"),
                        message,
                        curEx.Message,
                        curEx.StackTrace));

                    curEx = curEx.InnerException;
                }
            }
            sb.AppendLine();
            return sb.ToString();
        }

        private const string blank = "  ";
        internal static string GetAggrateException(string message, AggregateException ex, string exMsg)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Join(blank, DateTime.Now.ToString("yyyyMMddHHmmss"),
                    message,
                    ex.Message,
                    ex.StackTrace));

            foreach (var innerEx in ex.Flatten().InnerExceptions)
            {
                sb.AppendLine(GetExceptionString(message, ex, exMsg));
            }
            return sb.ToString();
        }


        private int _maxConcurrentItemNumber = 0;
        private int MaxConcurrentItemNumber
        {
            get
            {
                if (_maxConcurrentItemNumber == 0)
                {
                    _maxConcurrentItemNumber = 5;
                    int result = 0;
                    if (int.TryParse(ConfigurationManager.AppSettings["MaxConcurrentItemNumber"], out result))
                    {
                        _maxConcurrentItemNumber = result > 0 ? result : 5;
                    }
                    LogFactory.LogInstance.WriteLog(LogInterface.LogLevel.INFO, string.Format("Parralle count is {0}", _maxConcurrentItemNumber));
                }
                return _maxConcurrentItemNumber;
            }
        }

        public string CurrentMailbox
        {
            get; set;
        }

        public string CurrentFolder
        {
            get; set;
        }

        public string CurrentItem
        {
            get; set;
        }

        public ConcurrentQueue<string> LatestInformation
        {
            get; set;
        }

        public string CurrentStatus
        {
            get; set;
        }

        public string MailboxPercent
        {
            get; set;
        }

        public string FolderPercent
        {
            get; set;
        }

        public string ItemPercent
        {
            get; set;
        }

        private DateTime _endTime = DateTime.MinValue;
        public DateTime EndTime
        {
            get
            {
                return _endTime;
            }
            set
            {
                _endTime = value;
            }
        }

        private long AllDealedItemIndex = 0;
        
        private long AllDealedFolderIndex = 0;

        private long AllItemIndex = 0;

        private long ActualSize = 0;
        private long MailboxSize = 0;

        private object _lockObj = new object();

        private ConcurrentBag<string> FailureItems = new ConcurrentBag<string>();

        private void GenerateEachFolderCatalog(IMailboxData mailboxData,
           IFolderData folderData,
           ICatalogDataAccess dataAccess,
           IDataConvert dataConvert,
           Stack<IFolderData> folderStack,
           int level = 0)
        {
            var foldeHyPath = string.Join(" | ", (from f in folderStack select ((IFolderDataBase)f).DisplayName).Reverse().AsEnumerable());
            CurrentFolder = foldeHyPath;
            GenerateFolderStart(folderStack, mailboxData, folderData);
            bool hasError = true;
            try
            {
                if (level != 0) // root folder don't need save items;
                {
                    OnFolderProgressChanged(CatalogFolderProgressType.ChildItemStart, mailboxData, folderStack, folderData);
                    if (folderData.ChildItemCountInEx > 0)
                    {
                        OnFolderProgressChanged(CatalogFolderProgressType.GetChildItemStart, mailboxData, folderStack, folderData);
                        List<IItemData> folderItems = EwsAdapter.GetFolderItems(folderData);
                        int itemCount = folderItems.Count;
                        OnFolderProgressChanged(CatalogFolderProgressType.GetChildItemsEnd, mailboxData, folderStack, folderData, new Process(-1, itemCount));

                        if (itemCount > 0)
                        {
                            int itemDealedCount = 0;
                            Parallel.ForEach(source: folderItems,
                                parallelOptions: new ParallelOptions() { MaxDegreeOfParallelism = MaxConcurrentItemNumber },
                                localInit: () =>
                                {
                                    return 0;
                                },
                                body: (item, state, index, localValue) =>
                             {
                                 var dealedCountTemp = 0;

                                 using (_lockObj.LockWhile(() =>
                                 {
                                     itemDealedCount++;
                                     AllItemIndex++;
                                     dealedCountTemp = itemDealedCount;
                                     ThreadData.Information = AllItemIndex.ToString("D8");
                                 }))
                                 { };

                                 CurrentFolder = foldeHyPath;
                                 ItemPercent = string.Format("{0}/{1}", itemDealedCount, itemCount);
                                 CurrentItem = item.DisplayName;
                                 //LogFactory.LogInstance.WriteLog(LogInterface.LogLevel.INFO, string.Format("{1} Item {0} start.", item.Subject, index));
                                 DateTime itemStartTime = DateTime.Now;
                                 do
                                 {
                                     bool itemHasError = true;
                                     try
                                     {
                                         if (Filter.IsFilterItem(item, mailboxData, folderStack))
                                         {
                                             OnItemProgressChanged(CatalogItemProgressType.SkipItem, mailboxData, folderStack, folderData, item);
                                             break;
                                         }

                                         if(item.SizeInEx > ExportUploadHelper.MaxSupportItemSize)
                                         {
                                             LogFactory.LogInstance.WriteLog(LogLevel.WARN, "Item out of max size.", "{0} item {1} out of max size {2}", dealedCountTemp, item.DisplayName, ExportUploadHelper.MaxSupportItemSize);
                                             break;
                                         }

                                         Interlocked.Increment(ref AllDealedItemIndex);
                                         Interlocked.Add(ref MailboxSize, item.SizeInEx);

                                         OnFolderProgressChanged(CatalogFolderProgressType.ProcessingItemStart, mailboxData, folderStack, folderData, new Process((int)dealedCountTemp, itemCount), item);
                                         GenerateItemStart(item, mailboxData, folderStack, folderData);

                                         OnItemProgressChanged(CatalogItemProgressType.SaveItemStart, mailboxData, folderStack, folderData, item);
                                         dataAccess.SaveItem(item, mailboxData, folderData);

                                         using (_lockObj.LockWhile(() =>
                                         {
                                             folderData.ChildItemCount++;
                                         }))
                                         { };

                                         OnItemProgressChanged(CatalogItemProgressType.SaveItemEnd, mailboxData, folderStack, folderData, item);

                                         var itemIsNew = EwsAdapter.IsItemNew(item, DateTime.MinValue, StartTime) && !dataAccess.IsItemContentExist(item.Id);
                                         if (itemIsNew)
                                         {
                                             OnItemProgressChanged(CatalogItemProgressType.SaveItemContentStart, mailboxData, folderStack, folderData, item);
                                             dataAccess.SaveItemContent(item, mailboxData.MailAddress, StartTime, true, !itemIsNew);
                                             OnItemProgressChanged(CatalogItemProgressType.SaveItemContentEnd, mailboxData, folderStack, folderData, item);
                                             Interlocked.Add(ref ActualSize, item.ActualSize);
                                         }
                                         else
                                         {
                                             OnItemProgressChanged(CatalogItemProgressType.SaveItemContentEndForExist, mailboxData, folderStack, folderData, item);
                                         }
                                         itemHasError = false;

                                     }
                                     catch (Exception ex)
                                     {
                                         System.Diagnostics.Trace.TraceError(ex.GetExceptionDetail());
                                         var itemFailedMsg = string.Format("Item {0} ItemId:{2} in {1} can't export.", item.DisplayName, foldeHyPath, item.Id);
                                         FailureItems.Add(itemFailedMsg);
                                         LogFactory.LogInstance.WriteException(LogInterface.LogLevel.ERR, itemFailedMsg
                                             , ex, ex.Message);
                                     }
                                     finally
                                     {
                                         GenerateItemEnd(item, mailboxData, folderStack, folderData, itemHasError);
                                         if (itemHasError)
                                             OnFolderProgressChanged(CatalogFolderProgressType.ProcessingItemEndWithError, mailboxData, folderStack, folderData, new Process((int)dealedCountTemp, itemCount), item);
                                         else
                                             OnFolderProgressChanged(CatalogFolderProgressType.ProcessingItemEndNoError, mailboxData, folderStack, folderData, new Process((int)dealedCountTemp, itemCount), item);
                                         LogFactory.LogInstance.WriteLog(LogInterface.LogLevel.DEBUG, string.Format("{0} item {1} size {2}b end", dealedCountTemp, item.DisplayName, item.SizeInEx),
                                             "TotalTime:{0}", (DateTime.Now - itemStartTime).TotalSeconds);
                                     }
                                 } while (false);

                                 return localValue;
                             }, localFinally: (localValue) =>
                             {
                             });

                            //foreach (var item in folderItems)
                            //{
                            //    itemIndex++;
                            //    DateTime itemStartTime = DateTime.Now;
                            //    IItemData itemData = dataConvert.Convert(item);
                            //    if (Filter.IsFilterItem(itemData, mailboxData, folderStack))
                            //    {
                            //        OnItemProgressChanged(CatalogItemProgressType.SkipItem, mailboxData, folderStack, folderData, itemData);
                            //        continue;
                            //    }

                            //    bool itemHasError = true;


                            //    OnFolderProgressChanged(CatalogFolderProgressType.ProcessingItemStart, mailboxData, folderStack, folderData, new Process(itemIndex, itemCount), itemData);
                            //    GenerateItemStart(itemData, mailboxData, folderStack, folderData);
                            //    try
                            //    {
                            //        OnItemProgressChanged(CatalogItemProgressType.SaveItemStart, mailboxData, folderStack, folderData, itemData);
                            //        dataAccess.SaveItem(itemData, mailboxData, folderData);
                            //        folderData.ChildItemCount++;
                            //        OnItemProgressChanged(CatalogItemProgressType.SaveItemEnd, mailboxData, folderStack, folderData, itemData);

                            //        var itemIsNew = itemOperator.IsItemNew(item, DateTime.MinValue, StartTime);
                            //        if (itemIsNew)
                            //        {
                            //            OnItemProgressChanged(CatalogItemProgressType.SaveItemContentStart, mailboxData, folderStack, folderData, itemData);
                            //            dataAccess.SaveItemContent(itemData, mailboxData.MailAddress, StartTime, true, !itemIsNew);
                            //            OnItemProgressChanged(CatalogItemProgressType.SaveItemContentEnd, mailboxData, folderStack, folderData, itemData);
                            //        }
                            //        else
                            //        {
                            //            OnItemProgressChanged(CatalogItemProgressType.SaveItemContentEndForExist, mailboxData, folderStack, folderData, itemData);
                            //        }
                            //        itemHasError = false;
                            //    }
                            //    finally
                            //    {
                            //        GenerateItemEnd(itemData, mailboxData, folderStack, folderData, itemHasError);
                            //        if (itemHasError)
                            //            OnFolderProgressChanged(CatalogFolderProgressType.ProcessingItemEndWithError, mailboxData, folderStack, folderData, new Process(itemIndex, itemCount), itemData);
                            //        else
                            //            OnFolderProgressChanged(CatalogFolderProgressType.ProcessingItemEndNoError, mailboxData, folderStack, folderData, new Process(itemIndex, itemCount), itemData);

                            //        LogFactory.LogInstance.WriteLog(LogInterface.LogLevel.DEBUG, string.Format("{0} item end", itemIndex),
                            //            "ThreadId:{0}, TaskId:{1}, TotalTime:{2}", Thread.CurrentThread.ManagedThreadId, System.Threading.Tasks.Task.CurrentId, (DateTime.Now - itemStartTime).TotalSeconds);
                            //    }
                            //}
                        }
                        else
                        {
                            OnFolderProgressChanged(CatalogFolderProgressType.NoChildItem, mailboxData, folderStack, folderData);
                        }

                        ServiceContext.DataAccessObj.SaveChanges();
                    }
                    OnFolderProgressChanged(CatalogFolderProgressType.ChildItemEnd, mailboxData, folderStack, folderData);
                }

                OnFolderProgressChanged(CatalogFolderProgressType.ChildFolderStart, mailboxData, folderStack, folderData);
                if (folderData.ChildFolderCountInEx > 0)
                {
                    OnFolderProgressChanged(CatalogFolderProgressType.GetChildFoldersStart, mailboxData, folderStack, folderData);
                    List<IFolderData> childFolders = EwsAdapter.GetChildFolder(folderData.FolderId);
                    int childFolderIndex = 0;
                    int childFolderCount = childFolders.Count;

                    CurrentFolder = foldeHyPath;

                    LogFactory.LogInstance.WriteLog(LogInterface.LogLevel.INFO, string.Format("{0} has {1} folders:{2}.",
                        ((IFolderDataBase)folderData).DisplayName,
                        childFolderCount,
                        CurrentFolder));


                    OnFolderProgressChanged(CatalogFolderProgressType.GetChildFoldersEnd, mailboxData, folderStack, folderData, null, null, new Process(-1, childFolderCount));

                    foreach (var childFolder in childFolders)
                    {
                        childFolderIndex++;

                        FolderPercent = string.Format("{0}/{1}", childFolderIndex, childFolders.Count);

                        IFolderData childFolderData = childFolder;
                        if (!EwsAdapter.IsFolderNeedGenerateCatalog(childFolder))
                        {
                            OnFolderProgressChanged(CatalogFolderProgressType.ChildFolderSkip, mailboxData, folderStack, folderData, null, null, new Process(childFolderIndex, childFolderCount));
                            continue;
                        }

                        if (Filter.IsFilterFolder(childFolderData, mailboxData, folderStack))
                        {
                            OnFolderProgressChanged(CatalogFolderProgressType.ChildFolderSkip, mailboxData, folderStack, folderData, null, null, new Process(childFolderIndex, childFolderCount));
                            continue;
                        }

                        Interlocked.Increment(ref AllDealedFolderIndex);


                        folderStack.Push(childFolderData);
                        GenerateEachFolderCatalog(mailboxData, childFolderData, dataAccess, dataConvert, folderStack, level + 1);
                        folderStack.Pop();
                        //dataAccess.UpdateFolderChildFolderItemCount(childFolderData, StartTime);
                        //ServiceContext.DataAccessObj.SaveChanges();

                        OnFolderProgressChanged(CatalogFolderProgressType.SaveFolderStart, mailboxData, folderStack, folderData, null, null, new Process(childFolderIndex, childFolderCount));
                        dataAccess.SaveFolder(childFolderData, mailboxData, folderData);
                        ServiceContext.DataAccessObj.SaveChanges();
                        OnFolderProgressChanged(CatalogFolderProgressType.SaveFolderEnd, mailboxData, folderStack, folderData, null, null, new Process(childFolderIndex, childFolderCount));

                        folderData.ChildFolderCount++;
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
                ServiceContext.DataAccessObj.SaveChanges();
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
            if (ProgressChanged != null)
            {
                try
                {
                    ProgressChanged(this, new CatalogProgressArgs(type, mailboxProcess, mailbox));
                }
                catch (Exception e)
                {
                    System.Diagnostics.Trace.TraceError(e.GetExceptionDetail());
                    OnExceptionThrowed(e);
                }
            }
        }

        private void OnMailboxProgressChanged(CatalogMailboxProgressType type, IMailboxData mailbox = null)
        {
            if (MailboxProgressChanged != null)
            {
                try
                {
                    MailboxProgressChanged(this, new CatalogMailboxArgs(type, mailbox));
                }
                catch (Exception e)
                {
                    System.Diagnostics.Trace.TraceError(e.GetExceptionDetail());
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
                    System.Diagnostics.Trace.TraceError(e.GetExceptionDetail());
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
                    ItemProgressChanged(this, new CatalogItemArgs(type, mailbox, folderStack, folder, currentItem));
                }
                catch (Exception e)
                {
                    System.Diagnostics.Trace.TraceError(e.GetExceptionDetail());
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
                    System.Diagnostics.Trace.TraceError(ex.GetExceptionDetail());
                }
            }
        }

        public void MailboxGenerateStart(IMailboxData mailbox, int mailboxIndex, int mailboxCount)
        {
            OnProgressChanged(CatalogProgressType.GRTForMailboxStart, new Process(mailboxIndex, mailboxCount), mailbox);
            OnMailboxProgressChanged(CatalogMailboxProgressType.Start, mailbox);

            _serviceContext.CurrentMailbox = mailbox.MailAddress;

            CurrentMailbox = mailbox.MailAddress;
            MailboxPercent = string.Format("{0}/{1}", mailboxIndex, mailboxCount);

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
            ServiceContext.CurrentMailbox = string.Empty;

        }

        public void GenerateCatalogStart()
        {
            ServiceContext.DataAccessObj.BeginTransaction();
            LogFactory.LogInstance.WriteLog(LogInterface.LogLevel.INFO, string.Format("{0} Catalog Start", CatalogJobName));
            OnProgressChanged(CatalogProgressType.Start);
        }

        public void GenerateCatalogEnd(bool isFinished)
        {
            ServiceContext.DataAccessObj.EndTransaction(isFinished);
            if (isFinished)
                OnProgressChanged(CatalogProgressType.EndWithNoError);
            else
                OnProgressChanged(CatalogProgressType.EndWithError);

            if (FailureItems.Count > 0)
            {
                LogFactory.LogInstance.WriteLog(LogInterface.LogLevel.ERR, string.Format("Following items [{0}] export failure.", string.Join("][", (from i in FailureItems select i).AsEnumerable())));
            }

            LogFactory.LogInstance.WriteLog(LogInterface.LogLevel.INFO, string.Format("{0} Catalog End", CatalogJobName));
            EndTime = DateTime.Now;

            LogFactory.LogInstance.WriteLogMsgEvent -= DoEventHandler;
        }

        private void GenerateItemEnd(IItemData item, IMailboxData mailboxData, Stack<IFolderData> folderStack, IFolderData folderData, bool itemHasError)
        {
            if (itemHasError)
                OnItemProgressChanged(CatalogItemProgressType.EndWithError, mailboxData, folderStack, folderData, item);
            else
                OnItemProgressChanged(CatalogItemProgressType.EndWithNoError, mailboxData, folderStack, folderData, item);
        }

        private void GenerateItemStart(IItemData item, IMailboxData mailboxData, Stack<IFolderData> folderStack, IFolderData folderData)
        {
            OnItemProgressChanged(CatalogItemProgressType.Start, mailboxData, folderStack, folderData, item);
        }

        private void GenerateFolderEnd(Stack<IFolderData> folderStack, IMailboxData mailboxData, IFolderData folderData, bool hasError)
        {
            LogFactory.LogInstance.WriteLog(LogInterface.LogLevel.INFO, string.Format("{0} folder end.", ((IFolderDataBase)folderData).DisplayName));
            if (!hasError)
                OnFolderProgressChanged(CatalogFolderProgressType.EndWithNoError, mailboxData, folderStack, folderData);
            else
                OnFolderProgressChanged(CatalogFolderProgressType.EndWithError, mailboxData, folderStack, folderData);
        }

        private void GenerateFolderStart(Stack<IFolderData> folderStack, IMailboxData mailboxData, IFolderData folderData)
        {
            LogFactory.LogInstance.WriteLog(LogInterface.LogLevel.INFO, string.Format("{0} folder start.", ((IFolderDataBase)folderData).DisplayName));
            OnFolderProgressChanged(CatalogFolderProgressType.Start, mailboxData, folderStack, folderData);

            //FolderPercent = "";
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
        

        public virtual ICatalogDataAccess NewDataAccessInstance()
        {
            return (ICatalogDataAccess)ServiceContext.DataAccessObj;
        }

        public IDataConvert NewDataConvertInstance()
        {
            var result = CatalogFactory.Instance.NewDataConvert();
            return result;
        }

        public List<IFolderData> GetFolder(string mailbox, string parentId, bool containRootFolder)
        {
            ServiceContext.CurrentMailbox = mailbox;
            if(EwsAdapter == null)
            {
                EwsAdapter = CatalogFactory.Instance.NewEwsAdapter();
            }
            EwsAdapter.ConnectMailbox(ServiceContext.Argument, mailbox);
            IDataConvert dataConvert = CatalogFactory.Instance.NewDataConvert();
            dataConvert.OrganizationName = AdminInfo.OrganizationName;
            dataConvert.StartTime = DateTime.Now;

            EwsAdapter.DataConvert = dataConvert;

            List<IFolderData> folders = null;
            if (string.IsNullOrEmpty(parentId) || parentId == "0")
            {
                var rootFolder = EwsAdapter.GetRootFolder();
                if (containRootFolder)
                {
                    folders = new List<IFolderData>() { rootFolder };
                }
                else
                {
                    folders = EwsAdapter.GetChildFolder(rootFolder.FolderId);
                }
            }
            else
            {
                folders = EwsAdapter.GetChildFolder(parentId);
            }

            List<IFolderData> folderdatas = new List<IFolderData>(folders.Count);
            

            foreach (var folder in folders)
            {
                if (EwsAdapter.IsFolderNeedGenerateCatalog(folder))
                {
                    folderdatas.Add(folder);
                }
            }
            return folderdatas;
        }

        public void GenerateCatalog(IFilterItem filter)
        {
            Filter = filter;
            GenerateCatalog();
        }

        public void Dispose()
        {
            if (_serviceContext != null)
            {
                _serviceContext.Dispose();
                _serviceContext = null;
            }
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