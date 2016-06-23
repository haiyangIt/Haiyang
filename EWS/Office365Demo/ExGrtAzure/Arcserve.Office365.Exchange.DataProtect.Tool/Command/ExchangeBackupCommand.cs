using Arcserve.Office365.Exchange.Data;
using Arcserve.Office365.Exchange.Data.Account;
using Arcserve.Office365.Exchange.Data.Increment;
using Arcserve.Office365.Exchange.Data.Mail;
using Arcserve.Office365.Exchange.DataProtect.Impl;
using Arcserve.Office365.Exchange.DataProtect.Impl.Backup;
using Arcserve.Office365.Exchange.DataProtect.Interface.Backup;
using Arcserve.Office365.Exchange.EwsApi.Impl.Increment;
using Arcserve.Office365.Exchange.EwsApi.Interface;
using Arcserve.Office365.Exchange.Log;
using Arcserve.Office365.Exchange.Manager;
using Arcserve.Office365.Exchange.StorageAccess.MountSession;
using Arcserve.Office365.Exchange.StorageAccess.MountSession.EF.Data;
using Arcserve.Office365.Exchange.Thread;
using Arcserve.Office365.Exchange.Tool.Framework;
using Arcserve.Office365.Exchange.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Arcserve.Office365.Exchange.DataProtect.Tool.Result;
using Arcserve.Office365.Exchange.DataProtect.Tool.Resource;
using System.IO;

namespace Arcserve.Office365.Exchange.Tool
{
    /// <summary>
    /// Arcserve.Office365.Exchange.Tool.exe -JobType:ExchangeBackup -AdminUserName:user -AdminPassword:psw -Mailboxes:user@consto.com -WorkFolder:"E:\abc" -ItemCount:4
    /// </summary>
    [ArcserveCommand("ExchangeBackup")]
    public class ExchangeBackupCommand : ArcServeCommand
    {
        public ExchangeBackupCommand(CommandArgs args) : base(args) { }

        [CommandArgument("AdminUserName", "Please specify the job type. like: -AdminUserName:user.", true)]
        public ArgInfo AdminUserName { get; set; }

        [CommandArgument("AdminPassword", "Please specify the job type. like: -AdminPassword:user.", true)]
        public ArgInfo AdminPassword { get; set; }

        [CommandArgument("Mailboxes", "Please specify the job type. like: -Mailboxes:user@arcserve.com.", true)]
        public ArgInfo Mailboxes { get; set; }

        [CommandArgument("FolderCount")]
        public ArgInfo FolderCount { get; set; }

        [CommandArgument("ItemCount")]
        public ArgInfo ItemCount { get; set; }

        [CommandArgument("WorkFolder", "Please specify the work folder, like:-WorkFolder:E:\\myFolder", true)]
        public ArgInfo WorkFolder { get; set; }

        [CommandArgument("IsFull", "Please specify backup type, like:-IsFull:0", true)]
        public ArgInfo IsFull { get; set; }

        [CommandArgument("CurrentCatalogFolder", "Please specify current catalog folder, like:-CurrentCatalogFolder:E:\\myFolder", true)]
        public ArgInfo CurrentCatalogFolder { get; set; }

        [CommandArgument("LastCatalogFolder", "Please specify last catalog folder, like:-LastCatalogFolder:E:\\myFolder", false)]
        public ArgInfo LastCatalogFolder { get; set; }

        private int? _FolderCount
        {
            get
            {
                int result = 0;
                if (FolderCount != null && int.TryParse(FolderCount.Value, out result))
                {
                    return result;
                }
                return null;
            }
        }

        private int? _ItemCountInEachFolder
        {
            get
            {
                int result = 0;
                if (ItemCount != null && int.TryParse(ItemCount.Value, out result))
                {
                    return result;
                }
                return null;
            }
        }

        public static string CommandName
        {
            get
            {
                return "ExchangeBackup";
            }
        }

        private static char[] mailboxSplit = ";".ToCharArray();
        public List<string> MailboxList
        {
            get
            {
                var result = new List<string>(Mailboxes.Value.Split(mailboxSplit, StringSplitOptions.RemoveEmptyEntries));
                return result;
            }
        }

        protected override ResultBase DoExcute()
        {

            Backup();
            return new ExchangeBackupResult();
        }

        private void Backup()
        {
            try
            {
                if(IsFull.Value != "1")
                {
                    var lastCatalogFolder = LastCatalogFolder.Value;
                    if(string.IsNullOrEmpty(lastCatalogFolder) || !Directory.Exists(lastCatalogFolder))
                    {
                        throw new ArgumentException(string.Format("last catalog folder is not right {0}.", lastCatalogFolder));
                    }

                    LogFactory.LogInstance.WriteLog(LogLevel.INFO, string.Format("will start increment backup."));
                }
                else
                {
                    LogFactory.LogInstance.WriteLog(LogLevel.INFO, string.Format("will start full backup."));
                }

                using (var catalogAccess = new CatalogAccess(CurrentCatalogFolder.Value, LastCatalogFolder.Value, WorkFolder.Value, AdminUserName.Value.GetOrganization()))
                {
                    var taskSyncContextBase = DataProtectFactory.Instance.NewDefaultTaskSyncContext();
                    catalogAccess.InitTaskSyncContext(taskSyncContextBase);
                    var dataFromClient = new DataFromClient(_FolderCount, _ItemCountInEachFolder, MailboxList);
                    dataFromClient.InitTaskSyncContext(taskSyncContextBase);
                    var ewsServiceAdapter = EwsFactory.Instance.NewEwsAdapter();
                    ewsServiceAdapter.InitTaskSyncContext(taskSyncContextBase);
                    var dataConvert = new DataConvert();
                    var adminInfo = new Arcserve.Office365.Exchange.Data.Account.OrganizationAdminInfo()
                    {
                        OrganizationName = AdminUserName.Value.GetOrganization(),
                        UserName = AdminUserName.Value,
                        UserPassword = AdminPassword.Value
                    };

                    using (var backupFlow = DataProtectFactory.Instance.NewBackupInstance(catalogAccess, ewsServiceAdapter, dataFromClient, dataConvert, adminInfo))
                    {
                        backupFlow.InitTaskSyncContext(taskSyncContextBase);
                        backupFlow.BackupSync();
                    }
                }
            }
            finally
            {

            }
        }

        protected override ResultBase GetErrorResultBase(Exception e)
        {
            return new ExchangeBackupResult(e.Message);
        }

        protected override ResultBase GetInvalidUserPsw()
        {
            return new ExchangeBackupResult(ResMessage.UserNamePswInvalid);
        }
    }

    public class DataFromClient : IDataFromClient<IJobProgress>
    {
        public DataFromClient(int? folderCount, int? itemCount, List<string> mailboxes)
        {
            FolderCount = folderCount;
            ItemCount = itemCount;
            Mailboxes = mailboxes;
        }

        private List<string> Mailboxes;
        private int? ItemCount = null;
        public CancellationToken CancelToken
        {
            get; set;
        }

        public IJobProgress Progress
        {
            get; set;
        }

        public TaskScheduler Scheduler
        {
            get; set;
        }

        public ICollection<IMailboxDataSync> GetAllMailboxes()
        {
            var result = new List<IMailboxDataSync>(Mailboxes.Count);
            foreach (var mailbox in Mailboxes)
            {
                result.Add(new MailboxDataSyncBase() { MailAddress = mailbox });
            }
            return result;
        }

        public Task<ICollection<IMailboxDataSync>> GetAllMailboxesAsync()
        {
            throw new NotImplementedException();
        }


        public ICatalogJob GetLatestCatalogJob()
        {
            return null;
        }

        public Task<ICatalogJob> GetLatestCatalogJobAsync()
        {
            throw new NotImplementedException();
        }

        public void InitTaskSyncContext(ITaskSyncContext<IJobProgress> mainContext)
        {
            this.CloneSyncContext(mainContext);
        }

        public bool IsFolderClassValid(string folderClass)
        {
            return FolderClassUtil.IsFolderValid(folderClass);
        }

        private int? FolderCount = null;
        private int _dealtFolderCount = 0;
        public bool IsFolderInPlan(string uniqueFolderId)
        {
            if (FolderCount.HasValue)
            {
                _dealtFolderCount++;
                if (_dealtFolderCount <= FolderCount)
                    return true;
                return false;
            }
            else
            {
                return true;
            }
        }

        public Task<bool> IsFolderInPlanAsync(string uniqueFolderId)
        {
            throw new NotImplementedException();
        }

        public bool IsItemValid(IItemDataSync item, IFolderDataSync parentFolder)
        {
            return IsItemValid(item.ItemId, parentFolder.FolderId);
        }

        Dictionary<string, HashSet<string>> _folderItemCount = new Dictionary<string, HashSet<string>>();
        private bool IsItemValid(string itemId, string parentId)
        {
            bool returnResult = false;
            if (ItemCount.HasValue)
            {
                using (_folderItemCount.LockWhile(() =>
                {
                    HashSet<string> result = null;
                    if (!_folderItemCount.TryGetValue(parentId, out result))
                    {
                        result = new HashSet<string>();
                        _folderItemCount.Add(parentId, result);
                    }

                    if (result.Count > ItemCount)
                    {
                        returnResult = false;
                    }
                    else
                        returnResult = true;
                    if (!result.Contains(itemId))
                        result.Add(itemId);
                }))
                { }
                return returnResult;
            }
            else
            {
                return true;
            }
        }

        public bool IsItemValid(string itemChangeId, IFolderDataSync parentFolder)
        {
            return IsItemValid(itemChangeId, parentFolder.FolderId);
        }

        public bool IsFolderInPlan(IFolderDataSync folder)
        {
            return true;
        }
    }
}
