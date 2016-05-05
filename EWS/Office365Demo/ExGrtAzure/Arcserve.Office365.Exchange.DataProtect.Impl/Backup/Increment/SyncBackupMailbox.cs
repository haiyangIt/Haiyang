using Arcserve.Office365.Exchange.DataProtect.Interface.Backup.Increment;
using Arcserve.Office365.Exchange.Thread;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Exchange.WebServices.Data;
using System.Threading;
using Arcserve.Office365.Exchange.EwsApi.Increment;
using Arcserve.Office365.Exchange.Data.Account;

namespace Arcserve.Office365.Exchange.DataProtect.Impl.Backup.Increment
{
    public class SyncBackupMailbox : BackupMailboxFlowTemplate, ITaskSyncContext<IJobProgress>
    {
        public IBackupQueryAsync<IJobProgress> BackupQuery { get; }
        public IEwsServiceAdapter<IJobProgress> EwsServiceAdapter { get; set; }
        public IDataFromClient<IJobProgress> DataFromClient { get; set; }
        public OrganizationAdminInfo AdminInfo { get; set; }
        public string Organization { get; }

        public CancellationToken CancelToken
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override BackupFolderFlowTemplate FolderTemplate
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override Func<string, ChangeCollection<FolderChange>> FuncGetChangedFolders
        {
            get
            {
                return (lastSyncStatus) =>
                {
                    return EwsServiceAdapter.SyncFolders(lastSyncStatus);
                };
            }
        }

        public override Func<ExchangeService> FuncGetExchangeService
        {
            get
            {
                return () =>
                {
                    return EwsServiceAdapter.GetExchangeService(MailboxInfo.MailAddress, AdminInfo);
                };
            }
        }

        public override Func<FolderChange, bool> FuncIsFolderInPlan
        {
            get
            {
                return (folderChange) =>
                {
                    return DataFromClient.IsFolderInPlan(folderChange.FolderId.UniqueId);
                };
            }
        }

        public IJobProgress Progress
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public TaskScheduler Scheduler
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override void ForEachLoop(ICollection<FolderChange> folderChanges, Action<FolderChange> DoEachFolderChange)
        {
            foreach(var folderChange in folderChanges)
            {
                DoEachFolderChange(folderChange);
            }
        }

        public void InitTaskSyncContext(ITaskSyncContext<IJobProgress> mainContext)
        {
            throw new NotImplementedException();
        }
    }
}
