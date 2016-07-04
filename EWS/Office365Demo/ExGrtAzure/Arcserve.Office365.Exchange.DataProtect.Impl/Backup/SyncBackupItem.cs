using Arcserve.Office365.Exchange.DataProtect.Interface.Backup;
using Arcserve.Office365.Exchange.Thread;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Exchange.WebServices.Data;
using Arcserve.Office365.Exchange.Data.Increment;
using Arcserve.Office365.Exchange.Data.Mail;
using Arcserve.Office365.Exchange.Util.Setting;
using Arcserve.Office365.Exchange.Util;
using Arcserve.Office365.Exchange.EwsApi.Interface;

namespace Arcserve.Office365.Exchange.DataProtect.Impl.Backup
{
    internal class SyncBackupItem : BackupItemFlow, ITaskSyncContext<IJobProgress>, IExchangeAccess<IJobProgress>
    {
        CollectionPatitionUtil<IItemDataSync> _batchItemUtil;
        public SyncBackupItem()
        {
            _batchItemUtil = new CollectionPatitionUtil<IItemDataSync>((item) =>
            {
                return item.Size;
            });
        }

        protected override Action<IEnumerable<IItemDataSync>> ActionAddItemsToCatalog
        {
            get
            {
                return (items) =>
                {
                    CatalogAccess.AddItemsToCatalog(items);
                };
            }
        }

        protected override Action<IEnumerable<string>> ActionDeleteItemsToCatalog
        {
            get
            {
                return (itemIds) =>
                {
                    CatalogAccess.DeleteItemsToCatalog(itemIds);
                };
            }
        }
        protected override Action<IEnumerable<IItemDataSync>> ActionUpdateItemsToCatalog
        {
            get
            {
                return (items) =>
                {
                    CatalogAccess.UpdateItemsToCatalog(items);
                };
            }
        }


        protected override Func<IEnumerable<IItemDataSync>, int> FuncWriteItemsToStorage
        {
            get
            {
                return (items) =>
                {
                    return EwsServiceAdapter.ExportItems(items, CatalogAccess);
                };
            }
        }



        public ICatalogAccess<IJobProgress> CatalogAccess
        {
            get; set;
        }

        public IDataFromBackup<IJobProgress> DataFromClient
        {
            get; set;
        }

        public IEwsServiceAdapter<IJobProgress> EwsServiceAdapter
        {
            get; set;
        }

        protected override Action<IEnumerable<Item>, ItemClass> ActionLoadPropertyForItems
        {
            get
            {
                return (items, itemClass) =>
                {
                    EwsServiceAdapter.LoadItemsProperties(items, itemClass);
                };
            }
        }

        protected override bool IsRewriteDataIfReadFlagChanged
        {
            get
            {
                return CloudConfig.Instance.IsRewriteDataIfReadFlagChanged;
            }
        }

        public override IDataConvert DataConvert
        {
            get; set;
        }

        protected override Action<IItemDataSync> ActionAddItemToCatalog
        {
            get
            {
                return (item) =>
                {
                    CatalogAccess.AddItemToCatalog(item);
                };
            }
        }

        protected override Func<IEnumerable<string>, IEnumerable<IItemDataSync>> FuncGetItemsFromCatalog
        {
            get
            {
                return (itemIds) =>
                {
                    return CatalogAccess.GetItemsFromLatestCatalog(itemIds);
                };
            }
        }

        protected override Func<string, IEnumerable<IItemDataSync>> FuncGetItemsByParentFolderIdFromCatalog
        {
            get
            {
                return (parentFolderId) =>
                {
                    return CatalogAccess.GetItemsByParentFolderIdFromCatalog(parentFolderId);
                };
            }
        }

        protected override Func<IEnumerable<IItemDataSync>, IEnumerable<IItemDataSync>> FuncRemoveInvalidItem
        {
            get
            {
                return (items) =>
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
                };
            }
        }

        protected override Func<string, bool> FuncIsItemChangeValid
        {
            get
            {
                return (itemChangeId) =>
                {
                    return DataFromClient.IsItemValid(itemChangeId, ParentFolder);
                };
            }
        }

        protected override Func<IItemDataSync, bool> FuncIsItemValid
        {
            get
            {
                return (item) =>
                {
                    return DataFromClient.IsItemValid(item, ParentFolder);
                };
            }
        }

        private static int _loadPropertyMaxCount = CloudConfig.Instance.BatchLoadPropertyItemCount;

        private Dictionary<ItemClass, List<ItemChange>> _dicItemAdds = new Dictionary<ItemClass, List<ItemChange>>();
        private Dictionary<ItemClass, List<ItemChange>> _dicItemUpdates = new Dictionary<ItemClass, List<ItemChange>>();

        protected override bool CheckCanBatchAdded(ItemChange itemChange, ItemClass itemClass, out ICollection<ItemChange> batchItems)
        {
            return CheckCanBatch(itemChange, itemClass, _dicItemAdds, out batchItems);
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

        private List<ItemChange> _itemDeletes = new List<ItemChange>();
        protected override bool CheckCanBatchDelete(ItemChange itemChange, out ICollection<ItemChange> batchItems)
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

        private List<ItemChange> _dicItemReadChangs = new List<ItemChange>();
        protected override bool CheckCanBatchReadChange(ItemChange itemChange, ItemClass itemClass, out ICollection<ItemChange> batchItems)
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

        protected override bool CheckCanBatchUpdate(ItemChange itemChange, ItemClass itemClass, out ICollection<ItemChange> batchItems)
        {
            return CheckCanBatch(itemChange, itemClass, _dicItemUpdates, out batchItems);
        }


        protected override bool CheckCanWriteToStorage(IItemDataSync item, out IEnumerable<IEnumerable<IItemDataSync>> items)
        {
            return _batchItemUtil.AddItem(item, out items);
        }

        protected override Dictionary<ItemClass, List<ItemChange>> GetLeftBatchAdded()
        {
            var result = new Dictionary<ItemClass, List<ItemChange>>();
            foreach(var keyValue in _dicItemAdds)
            {
                if(keyValue.Value.Count > 0)
                {
                    result.Add(keyValue.Key, keyValue.Value);
                }
            }
            _dicItemAdds.Clear();
            return result;
        }

        protected override Dictionary<ItemClass, List<ItemChange>> GetLeftBatchUpdated()
        {
            var result = new Dictionary<ItemClass, List<ItemChange>>();
            foreach (var keyValue in _dicItemUpdates)
            {
                if (keyValue.Value.Count > 0)
                {
                    result.Add(keyValue.Key, keyValue.Value);
                }
            }
            _dicItemUpdates.Clear();
            return result;
        }

        protected override List<ItemChange> GetLeftBatchReadChanged()
        {
            return _dicItemReadChangs;
        }

        protected override List<ItemChange> GetLeftBatchDeleted()
        {
            return _itemDeletes;
        }

        protected override IEnumerable<IEnumerable<IItemDataSync>> GetLeftWriteToStorageItems()
        {
            return _batchItemUtil.AddComplete();
        }
    }
}
