using Arcserve.Office365.Exchange.DataProtect.Interface.Backup.Increment;
using Arcserve.Office365.Exchange.Thread;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Arcserve.Office365.Exchange.Data.Increment;
using Microsoft.Exchange.WebServices.Data;
using Arcserve.Office365.Exchange.EwsApi.Increment;

namespace Arcserve.Office365.Exchange.DataProtect.Impl.Backup.Increment
{
    public class SyncBackupFolder : BackupFolderFlowTemplate, ITaskSyncContext<IJobProgress>, IExchangeAccess<IJobProgress>
    {
        public ICatalogAccess<IJobProgress> CatalogAccess { get; set; }
        public IEwsServiceAdapter<IJobProgress> EwsServiceAdapter { get; set; }
        public IDataFromClient<IJobProgress> DataFromClient { get; set; }


        //public override Action<FolderChange> ActionDeleteFolderToCatalog
        //{
        //    get
        //    {
        //        return (folderChange) =>
        //        {
        //            CatalogAccess.DeleteFolderToCatalog(folderChange.Folder);
        //        };
        //    }
        //}



        //public override Action<FolderId, string> ActionUpdateFolderSyncToCatalog
        //{
        //    get
        //    {
        //        return (folderId, newSyncStatus) =>
        //        {
        //            CatalogAccess.UpdateFolderSyncStatus(folderId, newSyncStatus);
        //        };
        //    }
        //}



        public override ProtectConfig BackupConfig
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public CancellationToken CancelToken
        {
            get; set;
        }

        public override Func<FolderId, string, ChangeCollection<ItemChange>> FuncGetChangedItems
        {
            get
            {
                return (folderId, lastSyncStatus) =>
                {
                    return EwsServiceAdapter.SyncItems(folderId, lastSyncStatus);
                };
            }
        }


        //public override Func<FolderChange, string> ActionAddFolderToCatalog
        //{
        //    get
        //    {
        //        return (folderChange) =>
        //        {
        //            return CatalogAccess.UpdateFolderByChangeType(folderChange);
        //        };
        //    }
        //}

        public override Action<Folder> ActionLoadFolderProperties
        {
            get
            {
                return (folder) =>
                {
                    EwsServiceAdapter.LoadFolderProperties(folder);
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

        //public override Func<string, string> FuncGetFolderLastSyncStatusInCatalog
        //{
        //    get
        //    {
        //        return (folderId) =>
        //        {
        //            return CatalogAccess.GetFolderLastSyncStatusFromLatestCatalog(folderId);
        //        };
        //    }
        //}

        public override Func<BackupItemFlow> FuncNewBackupItem
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override Action<IFolderDataSync> ActionAddFolderToCatalog
        {
            get
            {
                return (folder) =>
                {
                    CatalogAccess.AddFolderToCatalog(folder);
                };
            }
        }

        public override IDataConvert DataConvert
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

    }
}
