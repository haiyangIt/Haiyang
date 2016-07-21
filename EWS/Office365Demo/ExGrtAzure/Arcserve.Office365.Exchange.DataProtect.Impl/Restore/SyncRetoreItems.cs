using Arcserve.Office365.Exchange.DataProtect.Interface.Restore;
using Arcserve.Office365.Exchange.EwsApi.Interface;
using Arcserve.Office365.Exchange.Thread;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arcserve.Office365.Exchange.Data.Increment;
using Arcserve.Office365.Exchange.Util.Setting;
using Arcserve.Office365.Exchange.Util;

namespace Arcserve.Office365.Exchange.DataProtect.Impl.Restore
{
    public class SyncRetoreItems : RestoreItemsFlowTemplate, ITaskSyncContext<IJobProgress>, IExchangeAccessForRestore<IJobProgress>
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

        private CollectionPatitionUtil<ImportItemStatus> _batchItemUtil = new CollectionPatitionUtil<ImportItemStatus>((item) =>
        {
            return item.Item.Size;
        });

        private HashSet<string> notExistItemsInExchange = new HashSet<string>();

        private List<IItemDataSync> existItems = new List<IItemDataSync>();
        private List<IItemDataSync> notExistItems = new List<IItemDataSync>();

        public override void Init()
        {
            IEnumerable<string> notExistItems = RestoreToPosition.GetNotExistItems(FolderForRestore, Folder);
            if(notExistItems != null)
            {
                foreach(var item in notExistItems)
                {
                    notExistItemsInExchange.Add(item);
                }
            }
        }

        protected override void AnalysisItems(IEnumerable<IItemDataSync> items)
        {
            foreach (var item in items)
            {
                if (notExistItemsInExchange.Contains(item.ItemId))
                {
                    notExistItems.Add(item);
                }
                else
                {
                    existItems.Add(item);
                }
            }
        }

        protected override void RestoreItems(bool isForceRestore)
        {
            bool isRestoreExistItems = existItems.Count > CloudConfig.Instance.BatchExportImportMaxAddCount || isForceRestore;
            bool isRestoreNotExistItems = notExistItems.Count > CloudConfig.Instance.BatchExportImportMaxAddCount || isForceRestore;

            List<IEnumerable<ImportItemStatus>> partitions = new List<IEnumerable<ImportItemStatus>>();
            if (isRestoreExistItems)
            {
                if (RestoreToPosition.ImportExistItems())
                {
                    foreach(var item in existItems)
                    {
                        IEnumerable<IEnumerable<ImportItemStatus>> partitionResult = null;
                        if(_batchItemUtil.AddItem(new ImportItemStatus(item, true), out partitionResult))
                        {
                            partitions.AddRange(partitionResult);
                        }
                    }
                }
            }

            if (isRestoreNotExistItems)
            {
                if(RestoreToPosition.ImportNotExistItems())
                {
                    foreach(var item in notExistItems)
                    {
                        IEnumerable<IEnumerable<ImportItemStatus>> partitionResult = null;
                        if (_batchItemUtil.AddItem(new ImportItemStatus(item, false), out partitionResult))
                        {
                            partitions.AddRange(partitionResult);
                        }
                    }
                }
            }

            ForEachPartition(partitions, RestoreEachPartition);
        }

        protected virtual void ForEachPartition(IEnumerable<IEnumerable<ImportItemStatus>> partitions, Action<IEnumerable<ImportItemStatus>> actionDoEachPartition)
        {
            foreach(var part in partitions)
            {
                actionDoEachPartition(part);
            }
        }

        protected virtual void RestoreEachPartition(IEnumerable<ImportItemStatus> partition)
        {
            RestoreToPosition.ImportItems(partition, CatalogAccess, FolderForRestore);
        }
    }
}
