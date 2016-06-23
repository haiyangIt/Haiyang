using Arcserve.Office365.Exchange.DataProtect.Interface.Backup;
using Arcserve.Office365.Exchange.Thread;
using System;
using System.Collections.Generic;
using Microsoft.Exchange.WebServices.Data;
using Arcserve.Office365.Exchange.Data.Account;
using Arcserve.Office365.Exchange.Data.Increment;
using Arcserve.Office365.Exchange.EwsApi.Interface;

namespace Arcserve.Office365.Exchange.DataProtect.Impl.Backup
{
    internal class SyncBackupMailbox : BackupMailboxFlowTemplate, ITaskSyncContext<IJobProgress>, IExchangeAccess<IJobProgress>
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

        protected override Func<IFolderDataSync, bool> FuncIsFolderValid
        {
            get
            {
                return (folder) =>
                {
                    return DataFromClient.IsFolderInPlan(folder);
                };
            }
        }


        protected override Action<IMailboxDataSync> ActionUpdateMailboxSyncAndTreeToCatalog
        {
            get
            {
                return (mailbox) =>
                {
                    CatalogAccess.UpdateMailboxSyncAndTreeToCatalog(mailbox);
                };
            }
        }

        protected override Action<string> ActionDeleteFolderToCatalog { get
            {
                return (folderId) =>
                {
                    CatalogAccess.DeleteFolderToCatalog(folderId);
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

        protected override Action<IMailboxDataSync> ActionUpdateMailboxToCatalog
        {
            get
            {
                return (mailbox) =>
                {
                    CatalogAccess.UpdateMailboxToCatalog(mailbox);
                };
            }
        }

        protected override Action<IMailboxDataSync> ActionAddMailboxToCatalog
        {
            get
            {
                return (mailbox) =>
                {
                    CatalogAccess.AddMailboxesToCatalog(mailbox);
                };
            }
        }

        protected override void ForEachLoop(ICollection<IFolderDataSync> folders, ItemUADStatus itemStatus, Action<IFolderDataSync, ItemUADStatus> DoEachFolderChange)
        {
            
            foreach(var folder in folders)
            {
                DoEachFolderChange(folder, itemStatus);
            }
        }
    }
}
