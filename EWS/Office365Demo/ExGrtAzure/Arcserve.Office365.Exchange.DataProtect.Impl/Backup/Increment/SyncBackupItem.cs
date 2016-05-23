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
using Arcserve.Office365.Exchange.Data.Mail;
using Arcserve.Office365.Exchange.Util.Setting;
using Arcserve.Office365.Exchange.Util;

namespace Arcserve.Office365.Exchange.DataProtect.Impl.Backup.Increment
{
    public class SyncBackupItem : BackupItemFlow, ITaskSyncContext<IJobProgress>, IExchangeAccess<IJobProgress>
    {
        BatchItemUtil<IItemDataSync> _batchItemUtil;
        public SyncBackupItem()
        {
            _batchItemUtil = new BatchItemUtil<IItemDataSync>((item) =>
            {
                return item.Size;
            });
        }

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


        public override Func<IEnumerable<IItemDataSync>, int> FuncWriteItemsToStorage
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

        public override Action<IEnumerable<Item>, ItemClass> ActionLoadPropertyForItems
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
                    isGet = true;
                }
            }))
            { }

            batchItems = outPut;
            return isGet;
        }

        protected override bool CheckCanBatchDelete(ItemChange itemChange, ItemClass itemClass, out ICollection<ItemChange> batchItems)
        {
            batchItems = null;
            return false;
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
