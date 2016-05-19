using Arcserve.Office365.Exchange.DataProtect.Interface.Backup.Increment;
using Arcserve.Office365.Exchange.Thread;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arcserve.Office365.Exchange.EwsApi.Increment;
using System.Threading;
using Microsoft.Exchange.WebServices.Data;
using Arcserve.Office365.Exchange.Data.Increment;

namespace Arcserve.Office365.Exchange.DataProtect.Impl.Backup.Increment
{
    public class SyncBackupItem : BackupItemFlow, ITaskSyncContext<IJobProgress>, IExchangeAccess<IJobProgress>
    {
        public override Action<IEnumerable<IItemDataSync>> ActionAddItemsToCatalog
        {
            get
            {
                return (items) =>
                {
                    CatalogAccess.AddItemsToCatalog(items);
                };
            }
        }

        //public override Action<IEnumerable<Item>> ActionDeleteItemsToCatalog
        //{
        //    get
        //    {
        //        return (items) =>
        //        {
        //            CatalogAccess.DeleteItemsToCatalog(items);
        //        };
        //    }
        //}
        

        //public override Action<IEnumerable<Item>> ActionUpdateItemsToCatalog
        //{
        //    get
        //    {
        //        return (items) =>
        //        {
        //            CatalogAccess.UpdateItems(items);
        //        };
        //    }
        //}

        //public override Action<IEnumerable<Item>> ActionUpdateReadFlagItemsToCatalog
        //{
        //    get
        //    {
        //        return (itemsWithReadFlagChange) =>
        //        {
        //            CatalogAccess.UpdateReadFlagItems(itemsWithReadFlagChange);
        //        };
        //    }
        //}

        public override Action<IEnumerable<Item>> ActionWriteItemsToStorage
        {
            get
            {
                return (items) =>
                {
                    EwsServiceAdapter.ExportItems(items, CatalogAccess.WriteItemsToStorage);
                };
            }
        }

        public CancellationToken CancelToken
        {
            get; set;
        }

        public ICatalogAccess<IJobProgress> CatalogAccess
        {
            get; set;
        }

        public IDataFromClient<IJobProgress> DataFromClient
        {
            get; set;
        }

        public IEwsServiceAdapter<IJobProgress> EwsServiceAdapter
        {
            get; set;
        }

        public override Action<IEnumerable<Item>> ActionLoadPropertyForItems
        {
            get
            {
                return (items) =>
                {
                    EwsServiceAdapter.LoadItemsProperties(items);
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

        protected override bool IsRewriteDataIfReadFlagChanged
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override IDataConvert DataConvert
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override Action<IItemDataSync> ActionAddItemToCatalog
        {
            get
            {
                return (item) =>
                {
                    CatalogAccess.AddItemToCatalog(item);
                };
            }
        }

        public override Func<IEnumerable<string>, IEnumerable<IItemDataSync>> FuncGetItemsFromCatalog
        {
            get
            {
                return (itemIds) =>
                {
                    return CatalogAccess.GetItemsFromLatestCatalog(itemIds);
                };
            }
        }

        public override Func<string, IEnumerable<IItemDataSync>> FuncGetItemsByParentFolderIdFromCatalog
        {
            get
            {
                return (parentFolderId) =>
                {
                    return CatalogAccess.GetItemsByParentFolderIdFromCatalog(parentFolderId);
                };
            }
        }

        public void InitTaskSyncContext(ITaskSyncContext<IJobProgress> mainContext)
        {
            this.CloneSyncContext(mainContext);
        }

        protected override bool CheckCanBatchAdded(ItemChange itemChange, out ICollection<ItemChange> batchItems)
        {
            throw new NotImplementedException();
        }

        protected override bool CheckCanBatchDelete(ItemChange itemChange, out ICollection<ItemChange> batchItems)
        {
            throw new NotImplementedException();
        }

        protected override bool CheckCanBatchReadChange(ItemChange itemChange, out ICollection<ItemChange> batchItems)
        {
            throw new NotImplementedException();
        }

        protected override bool CheckCanBatchUpdate(ItemChange itemChange, out ICollection<ItemChange> batchItems)
        {
            throw new NotImplementedException();
        }
    }
}
