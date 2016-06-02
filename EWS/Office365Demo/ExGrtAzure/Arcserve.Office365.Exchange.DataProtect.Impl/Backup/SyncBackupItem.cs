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

        public IDataFromClient<IJobProgress> DataFromClient
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

        private Dictionary<ItemClass, List<ItemChange>> _dicItemChangs = new Dictionary<ItemClass, List<ItemChange>>();

        protected override bool CheckCanBatchAdded(ItemChange itemChange, ItemClass itemClass, out ICollection<ItemChange> batchItems)
        {
            return CheckCanBatch(itemChange, itemClass, out batchItems);
        }

        private bool CheckCanBatch(ItemChange itemChange, ItemClass itemClass, out ICollection<ItemChange> batchItems)
        {
            List<ItemChange> outPut = null;
            bool isGet = false;
            using (_dicItemChangs.LockWhile(() =>
            {
                List<ItemChange> result;
                if (!_dicItemChangs.TryGetValue(itemClass, out result))
                {
                    result = new List<ItemChange>(_loadPropertyMaxCount);
                    _dicItemChangs.Add(itemClass, result);
                }
                result.Add(itemChange);
                if (result.Count >= _loadPropertyMaxCount)
                {
                    outPut = result;
                    result = new List<ItemChange>(_loadPropertyMaxCount);
                    _dicItemChangs[itemClass] = result;

                    isGet = true;
                }
            }))
            { }

            batchItems = outPut;
            return isGet;
        }

        protected override bool CheckCanBatchDelete(ItemChange itemChange, out ICollection<ItemChange> batchItems)
        {
            throw new NotImplementedException();
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
            return CheckCanBatch(itemChange, itemClass, out batchItems);
        }


        protected override bool CheckCanWriteToStorage(IItemDataSync item, out IEnumerable<IEnumerable<IItemDataSync>> items)
        {
            return _batchItemUtil.AddItem(item, out items);
        }

        protected override Dictionary<ItemClass, List<ItemChange>> GetLeftBatchAdded()
        {
            var result = new Dictionary<ItemClass, List<ItemChange>>(_dicItemChangs);
            _dicItemChangs.Clear();
            return result;
        }

        protected override Dictionary<ItemClass, List<ItemChange>> GetLeftBatchUpdated()
        {
            var result = new Dictionary<ItemClass, List<ItemChange>>(_dicItemChangs);
            _dicItemChangs.Clear();
            return result;
        }

        protected override List<ItemChange> GetLeftBatchReadChanged()
        {
            return _dicItemReadChangs;
        }

        protected override Dictionary<ItemClass, List<ItemChange>> GetLeftBatchDeleted()
        {
            return new Dictionary<ItemClass, List<ItemChange>>(0);
        }

        protected override IEnumerable<IEnumerable<IItemDataSync>> GetLeftWriteToStorageItems()
        {
            return _batchItemUtil.AddComplete();
        }
    }
}
