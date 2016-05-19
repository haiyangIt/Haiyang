using Arcserve.Office365.Exchange.Data;
using Arcserve.Office365.Exchange.Data.Increment;
using Arcserve.Office365.Exchange.DataProtect.Interface.Backup.Increment;
using Arcserve.Office365.Exchange.StorageAccess.MountSession.EF.Data;
using Arcserve.Office365.Exchange.Thread;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Arcserve.Office365.Exchange.EwsApi.Increment;
using Arcserve.Office365.Exchange.Util;

namespace Arcserve.Office365.Exchange.StorageAccess.MountSession.EF
{
    public class CatalogDbAccess : ICatalogAccess<IJobProgress>, ITaskSyncContext<IJobProgress>, IDisposable
    {
        private CatalogSyncDbContext _updateContext;
        private CatalogSyncDbContext _queryContext = null;
        private DataConvert _dataConvert;
        public CatalogDbAccess(string newCatalogFile, string lastCatalogFile)
        {
            _updateContext = new CatalogSyncDbContext(newCatalogFile);
            if (!string.IsNullOrEmpty(lastCatalogFile))
                _queryContext = new CatalogSyncDbContext(lastCatalogFile);
            _dataConvert = new DataConvert();
        }

        public CancellationToken CancelToken
        {
            get; set;
        }

        public IJobProgress Progress
        {
            get; set;
        }

        public TaskScheduler Scheduler
        {
            get; set;
        }

        private void SaveChanges()
        {
            throw new NotImplementedException();
        }

        public void AddMailboxesToCatalog(IEnumerable<IMailboxDataSync> mailboxes)
        {
            _updateContext.Mailboxes.AddRange(_dataConvert.Convert(mailboxes));
            SaveChanges();
        }

        public Task AddMailboxesToCatalogAsync(IEnumerable<IMailboxDataSync> mailboxes)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IFolderDataSync> GetFoldersFromLatestCatalog(IMailboxDataSync mailboxData)
        {
            return (from folder in _updateContext.Folders where folder.MailboxAddress == mailboxData.MailAddress select folder).AsEnumerable();
        }

        public Task<IEnumerable<IFolderDataSync>> GetFoldersFromLatestCatalogAsync(IMailboxDataSync mailboxData)
        {
            throw new NotImplementedException();
        }

        public void InitTaskSyncContext(ITaskSyncContext<IJobProgress> mainContext)
        {
            this.CloneSyncContext(mainContext);
        }

        public bool IsItemContentExist(string itemId)
        {
            return false;
        }


        public void Dispose()
        {
            if (_updateContext != null)
            {
                _updateContext.Dispose();
                _updateContext = null;
            }
        }

        public void AddItemsToCatalog(IEnumerable<IItemDataSync> items)
        {
            _updateContext.Items.AddRange(items as IEnumerable<ItemSyncModel>);
            SaveChanges();
        }

        public Task AddItemsToCatalogAsync(IEnumerable<IItemDataSync> items)
        {
            throw new NotImplementedException();
        }

        public void WriteItemsToStorage(IEnumerable<ItemDatas> items)
        {
            throw new InvalidOperationException();
        }

        public Task WriteItemsToStorageAsync(IEnumerable<ItemDatas> items)
        {
            throw new InvalidOperationException();
        }

        public void AddFolderToCatalog(IFolderDataSync folder)
        {
            _updateContext.Folders.Add(folder as FolderSyncModel);
            SaveChanges();
        }

        public Task AddFolderToCatalogAsync(IFolderDataSync folder)
        {
            throw new NotImplementedException();
        }

        public void AddItemToCatalog(IItemDataSync item)
        {
            _updateContext.Items.Add(item as ItemSyncModel);
            SaveChanges();
        }
        public Task AddItemToCatalogAsync(IItemDataSync item)
        {
            throw new NotImplementedException();
        }

        private static long _tempTableCount = 0;
        private static object _lock = new object();
        public IEnumerable<IItemDataSync> GetItemsFromLatestCatalog(IEnumerable<string> itemIds)
        {
            if(_queryContext != null)
            {
                return (from item in _queryContext.Items where itemIds.Contains(item.ItemId) select item).AsEnumerable();
            }
            return new List<IItemDataSync>(0);
        }
        
        public Task<IEnumerable<IItemDataSync>> GetItemsFromLatestCatalogAsync(IEnumerable<string> itemIds)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IItemDataSync> GetItemsByParentFolderIdFromCatalog(string parentFolderId)
        {
            if(_queryContext != null)
            {
                return (from item in _queryContext.Items where item.ParentFolderId == parentFolderId select item).AsEnumerable();
            }
            return new List<IItemDataSync>(0);
        }

        public Task<IEnumerable<IItemDataSync>> GetItemsByParentFolderIdFromCatalogAsync(string parentFolderId)
        {
            throw new NotImplementedException();
        }

        public void UpdateMailbox(IMailboxDataSync mailbox)
        {
            _updateContext.Entry(mailbox).State = System.Data.Entity.EntityState.Modified;
            SaveChanges();
        }

        public Task UpdateMailboxAsync(IMailboxDataSync mailbox)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IMailboxDataSync> GetMailboxesFromLatestCatalog(ICatalogJob catalogJob)
        {
            return (from mailbox in _queryContext.Mailboxes select mailbox).AsEnumerable();
        }

        public Task<IEnumerable<IMailboxDataSync>> GetMailboxFromLatestCatalogAsync(ICatalogJob catalogJob)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IMailboxDataSync> GetMailboxesFromLatestCatalog(DateTime currentJobStartTime)
        {
            return (from mailbox in _queryContext.Mailboxes select mailbox).AsEnumerable();
        }

        public Task<IEnumerable<IMailboxDataSync>> GetMailboxesFromLatestCatalogAsync(DateTime currentJobStartTime)
        {
            throw new NotImplementedException();
        }
    }
}
