using Arcserve.Office365.Exchange.DataProtect.Interface.Restore;
using Arcserve.Office365.Exchange.Thread;
using System;
using System.Collections.Generic;
using Arcserve.Office365.Exchange.Data.Increment;
using Arcserve.Office365.Exchange.EwsApi.Interface;
using Arcserve.Office365.Exchange.Data.Account;

namespace Arcserve.Office365.Exchange.DataProtect.Impl.Restore
{
    public class SyncRestoreMailbox : RestoreMailboxFlowTemplate, ITaskSyncContext<IJobProgress>, IExchangeAccessForRestore<IJobProgress>
    {
        public ICatalogAccessForRestore<IJobProgress> CatalogAccess
        {
            get; set;
        }

        public IDataFromClientForRestore<IJobProgress> DataFromClient
        {
            get; set;
        }

        public IEwsServiceAdapter<IJobProgress> EwsServiceAdapter
        {
            get; set;
        }

        public OrganizationAdminInfo AdminInfo { get; set; }

        protected override IRestoreToPosition<IJobProgress> NewRestoreToPosition()
        {
            throw new NotImplementedException();
        }

        protected override void ConnectExchangeService()
        {
            RestoreToPosition.ConnectExchangeService();
        }

        protected override void ForEachFolder(IEnumerable<IFolderDataSync> folders, FolderTree folderTree, Action<IFolderDataSync, FolderTree> actionDoFolder)
        {
            foreach(var folder in folders)
            {
                actionDoFolder(folder, folderTree);
            }
        }

        protected override IEnumerable<IFolderDataSync> GetFoldersFromPlan(IMailboxDataSync mailboxInfo, Func<IMailboxDataSync, IEnumerable<IFolderDataSync>> funcGetFolderFromCatalog)
        {
            return DataFromClient.GetAllFoldersFromPlan(mailboxInfo, funcGetFolderFromCatalog);
        }

        protected override IEnumerable<IFolderDataSync> GetFoldersFromCatalog(IMailboxDataSync mailboxInfo)
        {
            return CatalogAccess.GetFoldersFromCatalog(mailboxInfo);
        }

        protected override RestoreFolderFlowTemplate NewRestoreFolderTemplate()
        {
            var result = new SyncRestoreFolder();
            result.CloneSyncContext(this);
            result.CloneExchangeAccess(this);
            return result;
        }
    }
}
