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
        public OrganizationAdminInfo AdminInfo { get; internal set; }
        public string Organization { get; internal set; }


        protected override Func<BackupFolderFlowTemplate> FuncNewFolderTemplate
        {
            get
            {
                return ()=>
                {
                    var result = new SyncBackupFolder();
                    result.CloneSyncContext(this);
                    result.CloneExchangeAccess(this);
                    return result;
                };
            }
        }

        protected override Func<string, ChangeCollection<FolderChange>> FuncGetChangedFolders
        {
            get
            {
                return (lastSyncStatus) =>
                {
                    return EwsServiceAdapter.SyncFolderHierarchy(lastSyncStatus);
                };
            }
        }

        protected override Action ActionConnectExchangeService
        {
            get
            {
                return () =>
                {
                    EwsServiceAdapter.GetExchangeService(MailboxInfo.MailAddress, AdminInfo);
                };
            }
        }



        protected override Func<IMailboxDataSync, IEnumerable<IFolderDataSync>> FuncGetFoldersInLastCatalog
        {
            get
            {
                return (mailboxData) =>
                {
                    return CatalogAccess.GetFoldersFromLatestCatalog(mailboxData);
                };
            }
        }


        protected override Func<string, bool> FuncIsFolderInPlan
        {
            get
            {
                return (folderId) =>
                {
                    return DataFromClient.IsFolderInPlan(folderId);
                };
            }
        }


        protected override Action<IMailboxDataSync> ActionUpdateMailbox
        {
            get
            {
                return (mailbox) =>
                {
                    CatalogAccess.UpdateMailbox(mailbox);
                };
            }
        }

        protected override Func<IEnumerable<IFolderDataSync>, IEnumerable<IFolderDataSync>, FolderTree> FuncGetFolderTrees
        {
            get
            {
                return (array1, array2) =>
                {
                    FolderTree result = new FolderTree();
                    foreach (var f in array1)
                    {
                        result.AddNode(f);
                    }
                    foreach (var f in array2)
                    {
                        result.AddNode(f);
                    }
                    result.AddComplete();
                    return result;
                };
            }
        }

        protected override Action<Folder> ActionLoadFolderProperties
        {
            get
            {
                return (folder) =>
                {
                    EwsServiceAdapter.LoadFolderProperties(folder);
                };
            }
        }

        public override IDataConvert DataConvert
        {
            get; set;
        }

        protected override Func<string, bool> FuncIsFolderClassValid
        {
            get
            {
                return (folderClass) =>
                {
                    return DataFromClient.IsFolderClassValid(folderClass);
                };
            }
        }


        protected override void ForEachLoop(ICollection<IFolderDataSync> folders, Action<IFolderDataSync> DoEachFolderChange)
        {
            
            foreach(var folder in folders)
            {
                DoEachFolderChange(folder);
            }
        }
    }
}
