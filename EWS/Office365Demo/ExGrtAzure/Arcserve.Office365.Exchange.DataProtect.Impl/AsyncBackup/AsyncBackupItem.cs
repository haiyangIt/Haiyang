using Arcserve.Office365.Exchange.Data.Increment;
using Arcserve.Office365.Exchange.Data.Mail;
using Arcserve.Office365.Exchange.DataProtect.Interface.Backup;
using Arcserve.Office365.Exchange.EwsApi.Interface;
using Arcserve.Office365.Exchange.Thread;
using Arcserve.Office365.Exchange.Util;
using Arcserve.Office365.Exchange.Util.Setting;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EwsWSData = Microsoft.Exchange.WebServices.Data;

namespace Arcserve.Office365.Exchange.DataProtect.Impl.AsyncBackup
{
    public class AsyncBackupItem : BackupItemAsyncFlow, ITaskSyncContext<IJobProgress>, IExchangeAccess<IJobProgress>
    {
        CollectionPatitionUtil<IItemDataSync> _batchItemUtil;
        public AsyncBackupItem()
        {
            _batchItemUtil = new CollectionPatitionUtil<IItemDataSync>((item) =>
            {
                return item.Size;
            });
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
        public IDataConvert DataConvert
        {
            get; set;
        }


        public IJobProgress Progress
        {
            get; set;
        }

        public CancellationToken CancelToken
        {
            get; set;
        }

        public TaskScheduler Scheduler
        {
            get; set;
        }

        public bool IsRewriteDataIfReadFlagChanged
        {
            get
            {
                return CloudConfig.Instance.IsRewriteDataIfReadFlagChanged;
            }
        }

        private static int _loadPropertyMaxCount = CloudConfig.Instance.BatchLoadPropertyItemCount;

        private Dictionary<ItemClass, List<ItemChange>> _dicItemAdds = new Dictionary<ItemClass, List<ItemChange>>();
        private Dictionary<ItemClass, List<ItemChange>> _dicItemUpdates = new Dictionary<ItemClass, List<ItemChange>>();
        private List<ItemChange> _itemDeletes = new List<ItemChange>();
        private List<ItemChange> _dicItemReadChangs = new List<ItemChange>();

        public FolderTree FolderTree { get; set; }
        public IFolderDataSync ParentFolder { get; set; }

        public void InitTaskSyncContext(ITaskSyncContext<IJobProgress> mainContext)
        {
            this.CloneSyncContext(mainContext);
        }

        public async System.Threading.Tasks.Task LoadPropertyForItems(IEnumerable<Item> items, ItemClass itemClass)
        {
            await EwsServiceAdapter.LoadItemsPropertiesAsync(items, itemClass);
        }

        public async Task<int> WriteItemsToStorage(IEnumerable<IItemDataSync> items)
        {
            return await EwsServiceAdapter.ExportItemsAsync(items, CatalogAccess);
        }

        public async System.Threading.Tasks.Task AddItemsToCatalog(IEnumerable<IItemDataSync> items)
        {
            await CatalogAccess.AddItemsToCatalogAsync(items);
        }

        public async System.Threading.Tasks.Task DeleteItemsToCatalog(IEnumerable<string> itemIds)
        {
            await CatalogAccess.DeleteItemsToCatalogAsync(itemIds);
        }

        public async System.Threading.Tasks.Task UpdateItemsToCatalog(IEnumerable<IItemDataSync> items)
        {
            await CatalogAccess.UpdateItemsToCatalogAsync(items);
        }

        public async System.Threading.Tasks.Task AddItemToCatalog(IItemDataSync item)
        {
            await CatalogAccess.AddItemToCatalogAsync(item);
        }

        public async Task<IEnumerable<IItemDataSync>> GetItemsFromCatalog(IEnumerable<string> itemIds)
        {
            return await CatalogAccess.GetItemsFromLatestCatalogAsync(itemIds);
        }

        public async Task<IEnumerable<IItemDataSync>> GetItemsByParentFolderIdFromCatalog(string parentFolderId)
        {
            return await CatalogAccess.GetItemsByParentFolderIdFromCatalogAsync(parentFolderId);
        }

        public IEnumerable<IItemDataSync> RemoveInvalidItem(IEnumerable<IItemDataSync> items)
        {
            var result = new List<IItemDataSync>(items.Count());
            foreach (var item in items)
            {
                if (DataFromClient.IsItemValid(item, ParentFolder))
                {
                    result.Add(item);
                }
            }
            return result;
        }

        public bool IsItemChangeValid(string itemId)
        {
            return DataFromClient.IsItemValid(itemId, ParentFolder);
        }

        public bool IsItemValid(IItemDataSync item)
        {
            return DataFromClient.IsItemValid(item, ParentFolder);
        }

        public async System.Threading.Tasks.Task DealItem(ItemChange itemChange)
        {
            if (IsItemChangeValid(itemChange.ItemId.UniqueId))
            {
                switch (itemChange.ChangeType)
                {
                    case ChangeType.Create:
                        await DealAdded(itemChange);
                        break;
                    case Microsoft.Exchange.WebServices.Data.ChangeType.Update:
                        await DealUpdate(itemChange);
                        break;
                    case Microsoft.Exchange.WebServices.Data.ChangeType.ReadFlagChange:
                        await DealReadFlagChanged(itemChange);
                        break;
                    case Microsoft.Exchange.WebServices.Data.ChangeType.Delete:
                        await DealDelete(itemChange);
                        break;
                }
            }
        }

        private async System.Threading.Tasks.Task DealAdded(ItemChange itemChange)
        {
            ICollection<ItemChange> items = null;
            ItemClass itemClass = (ItemClass)(int)this.ParentFolder.FolderType.GetFolderClass();
            if (CheckCanBatchAdded(itemChange, itemClass, out items))
            {
                await BatchAddedItems(items, itemClass);
            }
        }

        private async System.Threading.Tasks.Task DealReadFlagChanged(ItemChange itemChange)
        {
            ICollection<ItemChange> items = null;
            ItemClass itemClass = (ItemClass)(int)ParentFolder.FolderType.GetFolderClass();
            if (CheckCanBatchReadChange(itemChange, itemClass, out items))
            {
                await BatchChangeRead(items);

            }
        }

        private async System.Threading.Tasks.Task DealUpdate(ItemChange itemChange)
        {
            ICollection<ItemChange> items = null;
            ItemClass itemClass = (ItemClass)(int)ParentFolder.FolderType.GetFolderClass();
            if (CheckCanBatchUpdate(itemChange, itemClass, out items))
            {
                await BatchUpdate(items, itemClass);
            }
        }

        private async System.Threading.Tasks.Task DealDelete(ItemChange itemChange)
        {
            ICollection<ItemChange> items = null;
            if (CheckCanBatchDelete(itemChange, out items))
            {
                await BatchDeleteItems(items);
            }
        }

        public async System.Threading.Tasks.Task DealFinish(HashSet<string> dealedItemIds)
        {
            var tasks = new List<System.Threading.Tasks.Task>();
            var addItems = GetLeftBatchAdded();
            foreach (var itemKeyValue in addItems)
            {
                tasks.Add(BatchAddedItems(itemKeyValue.Value, itemKeyValue.Key));
            }
            var udpateItems = GetLeftBatchUpdated();
            foreach (var itemKeyValue in udpateItems)
            {
                tasks.Add(BatchUpdate(itemKeyValue.Value, itemKeyValue.Key));
            }

            var readChangeItems = GetLeftBatchReadChanged();
            tasks.Add(BatchChangeRead(readChangeItems));

            var deleteItems = GetLeftBatchDeleted();

            tasks.Add(BatchDeleteItems(deleteItems));

            var writeToStorageItems = GetLeftWriteToStorageItems();
            var pCounter = PerformanceCounter.Start();
            foreach (var itemPartition in writeToStorageItems)
            {
                tasks.Add(WriteItemsToStorage(itemPartition));
            }
            await System.Threading.Tasks.Task.WhenAll(tasks.ToArray());
        }

        protected async System.Threading.Tasks.Task BatchAddedItems(ICollection<EwsWSData.ItemChange> batchItems, ItemClass itemClass)
        {
            var items = from item in batchItems select item.Item;

            Progress.Report("      Items add {0} Start.", batchItems.Count());
            await LoadPropertyForItems(items, itemClass);
            IEnumerable<IItemDataSync> itemDatas = DataConvert.Convert(items, ParentFolder);
            itemDatas = RemoveInvalidItem(itemDatas);

            var taskAdd = AddItemsToCatalog(itemDatas);
            var taskWrite = BatchWriteItemToStorage(itemDatas);

            var itemCount = itemDatas.Count();
            await System.Threading.Tasks.Task.WhenAll(taskAdd, taskWrite);
        }
        protected async System.Threading.Tasks.Task BatchDeleteItems(ICollection<EwsWSData.ItemChange> batchItems)
        {
            var itemIds = from item in batchItems select item.ItemId.UniqueId;
            await DeleteItemsToCatalog(itemIds);
        }
        protected async System.Threading.Tasks.Task BatchUpdate(ICollection<EwsWSData.ItemChange> batchItems, ItemClass itemClass)
        {
            var items = from item in batchItems select item.Item;
            var pCounter = PerformanceCounter.Start();
            Progress.Report("      Items update {0} Start.", batchItems.Count());
            await LoadPropertyForItems(items, itemClass);
            IEnumerable<IItemDataSync> itemDatas = DataConvert.Convert(items, ParentFolder);

            itemDatas = RemoveInvalidItem(itemDatas);
            var taskUpdate = UpdateItemsToCatalog(itemDatas);
            var taskWrite = BatchWriteItemToStorage(itemDatas);
            var itemCount = itemDatas.Count();
            await System.Threading.Tasks.Task.WhenAll(taskUpdate, taskWrite);
            Progress.Report("      Items update {0} End, total count {0}, total time {1}s.", itemCount, pCounter.EndBySecond());
        }
        protected async System.Threading.Tasks.Task BatchChangeRead(ICollection<EwsWSData.ItemChange> batchItems)
        {
            if (batchItems == null || batchItems.Count == 0)
                return;
            var pCounter = PerformanceCounter.Start();
            Progress.Report("      Items readChange {0} Start.", batchItems.Count());

            var items = from item in batchItems select item.Item;
            var ids = from item in batchItems select item.ItemId.UniqueId;
            var itemToIsRead = new Dictionary<string, bool>(batchItems.Count);
            foreach (var itemChange in batchItems)
            {
                itemToIsRead.Add(itemChange.ItemId.UniqueId, itemChange.IsRead);
            }
            var itemDatas = await GetItemsFromCatalog(ids);
            itemDatas = RemoveInvalidItem(itemDatas);

            foreach (var item in itemDatas)
            {
                item.IsRead = itemToIsRead[item.ItemId];
            }
            var taskAdd = AddItemsToCatalog(itemDatas);
            System.Threading.Tasks.Task taskWrite = null;
            if (IsRewriteDataIfReadFlagChanged)
            {
                taskWrite = BatchWriteItemToStorage(itemDatas);
                await System.Threading.Tasks.Task.WhenAll(taskAdd, taskWrite);
            }
            else
                await taskAdd;
            var itemCount = itemDatas.Count();
            Progress.Report("      Items readChange {0} End, total count {0}, total time {1}s.", itemCount, pCounter.EndBySecond());
        }

        protected async System.Threading.Tasks.Task BatchWriteItemToStorage(IEnumerable<IItemDataSync> items)
        {
            List<System.Threading.Tasks.Task> tasks = new List<System.Threading.Tasks.Task>();
            foreach (var item in items)
            {
                IEnumerable<IEnumerable<IItemDataSync>> result;
                if (CheckCanWriteToStorage(item, out result))
                {
                    foreach (var itemPartition in result)
                    {
                        var task = WriteItemsToStorage(itemPartition);
                        tasks.Add(task);
                    }
                }
            }
            await System.Threading.Tasks.Task.WhenAll(tasks.ToArray());
        }

        protected bool CheckCanBatchReadChange(ItemChange itemChange, ItemClass itemClass, out ICollection<ItemChange> batchItems)
        {
            List<ItemChange> result = null;
            bool isGet = false;
            using (_dicItemReadChangs.LockWhile(() =>
            {
                _dicItemReadChangs.Add(itemChange);
                if (_dicItemReadChangs.Count >= _loadPropertyMaxCount)
                {
                    result = new List<ItemChange>(_dicItemReadChangs);
                    _dicItemReadChangs.Clear();
                }
            }))
            {

            }
            batchItems = result;
            return isGet;
        }

        protected bool CheckCanBatchUpdate(ItemChange itemChange, ItemClass itemClass, out ICollection<ItemChange> batchItems)
        {
            return CheckCanBatch(itemChange, itemClass, _dicItemUpdates, out batchItems);
        }


        protected bool CheckCanWriteToStorage(IItemDataSync item, out IEnumerable<IEnumerable<IItemDataSync>> items)
        {
            return _batchItemUtil.AddItem(item, out items);
        }
        protected bool CheckCanBatchDelete(ItemChange itemChange, out ICollection<ItemChange> batchItems)
        {
            List<ItemChange> result = null;
            bool isGet = false;
            using (_itemDeletes.LockWhile(() =>
            {
                _itemDeletes.Add(itemChange);
                if (_itemDeletes.Count >= _loadPropertyMaxCount)
                {
                    result = new List<ItemChange>(_itemDeletes);
                    _itemDeletes.Clear();
                }
            }))
            {

            }
            batchItems = result;
            return isGet;
        }
        private static bool CheckCanBatch(ItemChange itemChange, ItemClass itemClass, Dictionary<ItemClass, List<ItemChange>> itemChanges, out ICollection<ItemChange> batchItems)
        {
            List<ItemChange> outPut = null;
            bool isGet = false;
            using (itemChanges.LockWhile(() =>
            {
                List<ItemChange> result;
                if (!itemChanges.TryGetValue(itemClass, out result))
                {
                    result = new List<ItemChange>(_loadPropertyMaxCount);
                    itemChanges.Add(itemClass, result);
                }
                result.Add(itemChange);
                if (result.Count >= _loadPropertyMaxCount)
                {
                    outPut = result;
                    result = new List<ItemChange>(_loadPropertyMaxCount);
                    itemChanges[itemClass] = result;

                    isGet = true;
                }
            }))
            { }

            batchItems = outPut;
            return isGet;
        }
        protected bool CheckCanBatchAdded(ItemChange itemChange, ItemClass itemClass, out ICollection<ItemChange> batchItems)
        {
            return CheckCanBatch(itemChange, itemClass, _dicItemAdds, out batchItems);
        }

        protected Dictionary<ItemClass, List<ItemChange>> GetLeftBatchAdded()
        {
            var result = new Dictionary<ItemClass, List<ItemChange>>(_dicItemAdds);
            _dicItemAdds.Clear();
            return result;
        }

        protected Dictionary<ItemClass, List<ItemChange>> GetLeftBatchUpdated()
        {
            var result = new Dictionary<ItemClass, List<ItemChange>>(_dicItemUpdates);
            _dicItemUpdates.Clear();
            return result;
        }

        protected List<ItemChange> GetLeftBatchReadChanged()
        {
            return _dicItemReadChangs;
        }

        protected List<ItemChange> GetLeftBatchDeleted()
        {
            return _itemDeletes;
        }

        protected IEnumerable<IEnumerable<IItemDataSync>> GetLeftWriteToStorageItems()
        {
            return _batchItemUtil.AddComplete();
        }
    }
}
