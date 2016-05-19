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
using Arcserve.Office365.Exchange.Data.Increment;
using Arcserve.Office365.Exchange.Util;

namespace Arcserve.Office365.Exchange.DataProtect.Impl.Backup.Increment
{
    public class SyncBackupMailbox : BackupMailboxFlowTemplate, ITaskSyncContext<IJobProgress>, IExchangeAccess<IJobProgress>
    {
        public ICatalogAccess<IJobProgress> CatalogAccess { get; set; }
        public IEwsServiceAdapter<IJobProgress> EwsServiceAdapter { get; set; }
        public IDataFromClient<IJobProgress> DataFromClient { get; set; }
        public OrganizationAdminInfo AdminInfo { get; set; }
        public string Organization { get; }

        public CancellationToken CancelToken
        {
            get; set;
        }

        public override Func<BackupFolderFlowTemplate> FuncNewFolderTemplate
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
                    return EwsServiceAdapter.SyncFolderHierarchy(lastSyncStatus);
                };
            }
        }

        public override Action ActionConnectExchangeService
        {
            get
            {
                return () =>
                {
                    EwsServiceAdapter.GetExchangeService(MailboxInfo.MailAddress, AdminInfo);
                };
            }
        }


        public IJobProgress Progress
        {
            get; set;
        }

        public TaskScheduler Scheduler
        {
            get; set;
        }

        public override Func<IMailboxDataSync, IEnumerable<IFolderDataSync>> FuncGetFoldersInLastCatalog
        {
            get
            {
                return (mailboxData) =>
                {
                    return CatalogAccess.GetFoldersFromLatestCatalog(mailboxData);
                };
            }
        }

        //public override Func<IMailboxDataSync, IEnumerable<IFolderDataSync>> FuncGetFoldersFromClient
        //{
        //    get
        //    {
        //        return (mailboxData) =>
        //        {
        //            return DataFromClient.GetFolders(mailboxData);
        //        };
        //    }
        //}
        

        public override Func<string, bool> FuncIsFolderInPlan
        {
            get
            {
                return (folderId) =>
                {
                    return DataFromClient.IsFolderInPlan(folderId);
                };
            }
        }
        

        public override Action<IMailboxDataSync> ActionUpdateMailbox
        {
            get
            {
                return (mailbox) =>
                {
                    CatalogAccess.UpdateMailbox(mailbox);
                };
            }
        }

        public override Func<IEnumerable<IFolderDataSync>, IEnumerable<FolderChange>, TreeNode<IFolderDataSync>> FuncGetFolderTrees
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void InitTaskSyncContext(ITaskSyncContext<IJobProgress> mainContext)
        {
            this.CloneSyncContext(mainContext);
        }

        public override void ForEachLoop(ICollection<IFolderDataSync> folders, Action<IFolderDataSync> DoEachFolderChange)
        {
            foreach(var folder in folders)
            {
                DoEachFolderChange(folder);
            }
        }

        public override void ForEachLoop(ICollection<FolderChange> folderChanges, Dictionary<string, IFolderDataSync> folderDic, Action<FolderChange, Dictionary<string, IFolderDataSync>> DoEachFolderChange)
        {
            foreach(var folder in folderChanges)
            {
                DoEachFolderChange(folder, folderDic);
            }
        }
    }
}
