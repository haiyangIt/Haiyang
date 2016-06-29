﻿using Arcserve.Office365.Exchange.DataProtect.Interface.Backup;
using Arcserve.Office365.Exchange.Thread;
using System;
using Arcserve.Office365.Exchange.Data.Increment;
using Microsoft.Exchange.WebServices.Data;
using Arcserve.Office365.Exchange.EwsApi.Interface;

namespace Arcserve.Office365.Exchange.DataProtect.Impl.Backup
{
    internal class SyncBackupFolder : BackupFolderFlowTemplate, ITaskSyncContext<IJobProgress>, IExchangeAccess<IJobProgress>
    {
        public ICatalogAccess<IJobProgress> CatalogAccess { get; set; }
        public IEwsServiceAdapter<IJobProgress> EwsServiceAdapter { get; set; }
        public IDataFromBackup<IJobProgress> DataFromClient { get; set; }


        protected override Func<string, string, ChangeCollection<ItemChange>> FuncGetChangedItems
        {
            get
            {
                return (folderId, lastSyncStatus) =>
                {
                    return EwsServiceAdapter.SyncItems(folderId, lastSyncStatus);
                };
            }
        }

        protected override Func<BackupItemFlow> FuncNewBackupItem
        {
            get
            {
                return () =>
                {
                    var result = new SyncBackupItem();
                    result.CloneSyncContext(this);
                    result.CloneExchangeAccess(this);
                    return result;
                };
            }
        }

        protected override Action<IFolderDataSync> ActionAddFolderToCatalog
        {
            get
            {
                return (folder) =>
                {
                    CatalogAccess.AddFolderToCatalog(folder);
                };
            }
        }

        protected override Action<IFolderDataSync> ActionUpdateFolderToCatalog { get
            {
                return (folder) =>
                {
                    CatalogAccess.UpdateFolderToCatalog(folder);
                };
            }
        }
        protected override Action<IFolderDataSync> ActionUpdateFolderStatusToCatalog { get
            {
                return (folder) =>
                {
                    CatalogAccess.UpdateFolderSyncStatusToCatalog(folder);
                };
            }
        }

        public IDataConvert DataConvert
        {
            get; set;
        }
    }
}
