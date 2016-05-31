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
using System.Data.Entity.Infrastructure;
using Arcserve.Office365.Exchange.Util.Setting;

namespace Arcserve.Office365.Exchange.StorageAccess.MountSession.EF
{
    public class CatalogDbAccess : ICatalogAccess<IJobProgress>, ITaskSyncContext<IJobProgress>, IDisposable
    {
        private CatalogSyncDbContext _updateContext;
        private CatalogSyncDbContext _queryContext = null;
        private DataConvert _dataConvert;
        public CatalogDbAccess(string newCatalogFolder, string lastCatalogFile, string organizationName)
        {
            if (!Directory.Exists(newCatalogFolder))
            {
                Directory.CreateDirectory(newCatalogFolder);
            }

            string fileName = string.Empty;
            string newCatalogFileName = string.Empty;
            if (!string.IsNullOrEmpty(lastCatalogFile) && File.Exists(lastCatalogFile))
            {
                fileName = Path.GetFileName(lastCatalogFile);
                newCatalogFileName = Path.Combine(newCatalogFolder, fileName);
                File.Copy(lastCatalogFile, newCatalogFileName);
                _queryContext = new CatalogSyncDbContext(lastCatalogFile);
            }
            else
            {
                fileName = string.Format("Catalog_{0}", MD5Utility.ConvertToMd5(organizationName));
                newCatalogFileName = Path.Combine(newCatalogFolder, fileName);
            }

            _updateContext = new CatalogSyncDbContext(newCatalogFileName);

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

        private int _SaveCount = 0;
        private object _lock = new object();
        private void SaveChanges()
        {
            bool isSave = false;
            using (_lock.LockWhile(() =>
            {
                _SaveCount++;
                if (_SaveCount > CloudConfig.Instance.BatchSaveToCatalogCount)
                {
                    isSave = true;
                    _SaveCount = 0;
                }
            }))
            { }

            if (isSave)
            {
                DoSaveChanges();
            }
        }

        private void DoSaveChanges()
        {
            bool saveFailed;
            do
            {
                saveFailed = false;
                try
                {
                    _updateContext.SaveChanges();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    saveFailed = true;

                    // Update original values from the database 
                    var entry = ex.Entries.Single();
                    entry.OriginalValues.SetValues(entry.GetDatabaseValues());
                }
            } while (saveFailed);
        }

        public void AddMailboxesToCatalog(IEnumerable<IMailboxDataSync> mailboxes)
        {
            _updateContext.Mailboxes.AddRange(_dataConvert.ConvertToMailboxModel(mailboxes));
            SaveChanges();
        }

        public Task AddMailboxesToCatalogAsync(IEnumerable<IMailboxDataSync> mailboxes)
        {
            throw new NotSupportedException();
        }

        public IEnumerable<IFolderDataSync> GetFoldersFromLatestCatalog(IMailboxDataSync mailboxData)
        {
            if (_queryContext != null)
                return (from folder in _queryContext.Folders where folder.MailboxAddress == mailboxData.MailAddress select folder).AsEnumerable();
            else
                return new List<IFolderDataSync>(0);
        }

        public Task<IEnumerable<IFolderDataSync>> GetFoldersFromLatestCatalogAsync(IMailboxDataSync mailboxData)
        {
            throw new NotSupportedException();
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
            DoSaveChanges();
            if (_updateContext != null)
            {
                _updateContext.Dispose();
                _updateContext = null;
            }
            if (_queryContext != null)
            {
                _queryContext.Dispose();
                _queryContext = null;
            }
        }

        private HashSet<string> itemsIdHash = new HashSet<string>();
        public void AddItemsToCatalog(IEnumerable<IItemDataSync> items)
        {
            var result = _dataConvert.ConvertToItemModel(items);
            foreach (var i in result)
            {
                itemsIdHash.Add(i.ItemId);
            }
            _updateContext.Items.AddRange(result);
            SaveChanges();
        }

        public Task AddItemsToCatalogAsync(IEnumerable<IItemDataSync> items)
        {
            throw new NotSupportedException();
        }


        public void AddFolderToCatalog(IFolderDataSync folder)
        {
            _updateContext.Folders.Add(folder as FolderSyncModel);
            SaveChanges();
        }

        public Task AddFolderToCatalogAsync(IFolderDataSync folder)
        {
            throw new NotSupportedException();
        }

        public void AddItemToCatalog(IItemDataSync item)
        {
            _updateContext.Items.Add(item as ItemSyncModel);
            SaveChanges();
        }
        public Task AddItemToCatalogAsync(IItemDataSync item)
        {
            throw new NotSupportedException();
        }

        public IEnumerable<IItemDataSync> GetItemsFromLatestCatalog(IEnumerable<string> itemIds)
        {
            if (_queryContext != null)
            {
                return (from item in _queryContext.Items where itemIds.Contains(item.ItemId) select item).AsEnumerable();
            }
            return new List<IItemDataSync>(0);
        }

        public Task<IEnumerable<IItemDataSync>> GetItemsFromLatestCatalogAsync(IEnumerable<string> itemIds)
        {
            throw new NotSupportedException();
        }

        public IEnumerable<IItemDataSync> GetItemsByParentFolderIdFromCatalog(string parentFolderId)
        {
            if (_queryContext != null)
            {
                return (from item in _queryContext.Items where item.ParentFolderId == parentFolderId select item).AsEnumerable();
            }
            return new List<IItemDataSync>(0);
        }

        public Task<IEnumerable<IItemDataSync>> GetItemsByParentFolderIdFromCatalogAsync(string parentFolderId)
        {
            throw new NotSupportedException();
        }

        public void UpdateMailboxSyncToCatalog(IMailboxDataSync mailbox)
        {
            DoSaveChanges();
            var mailboxInfo = (from m in _updateContext.Mailboxes where m.Id == mailbox.Id select m).FirstOrDefault();
            if (mailboxInfo == null)
                throw new ArgumentException("mailbox is not in catalog.");
            mailboxInfo.SyncStatus = mailbox.SyncStatus;
            _updateContext.SaveChanges();
        }

        public Task UpdateMailboxAsync(IMailboxDataSync mailbox)
        {
            throw new NotSupportedException();
        }

        public IEnumerable<IMailboxDataSync> GetMailboxesFromLatestCatalog(ICatalogJob catalogJob)
        {
            if (_queryContext != null)
                return (from mailbox in _queryContext.Mailboxes select mailbox).AsEnumerable();
            else
                return new List<IMailboxDataSync>(0);
        }

        public Task<IEnumerable<IMailboxDataSync>> GetMailboxFromLatestCatalogAsync(ICatalogJob catalogJob)
        {
            throw new NotSupportedException();
        }

        public IEnumerable<IMailboxDataSync> GetMailboxesFromLatestCatalog(DateTime currentJobStartTime)
        {
            if (_queryContext != null)
                return (from mailbox in _queryContext.Mailboxes select mailbox).AsEnumerable();
            else
                return new List<IMailboxDataSync>(0);
        }

        public Task<IEnumerable<IMailboxDataSync>> GetMailboxesFromLatestCatalogAsync(DateTime currentJobStartTime)
        {
            throw new NotSupportedException();
        }

        public void WriteBufferToStorage(IItemDataSync itemId, byte[] buffer, int length)
        {
            throw new NotSupportedException();
        }

        public void WriteComplete(IItemDataSync itemId)
        {
            throw new NotSupportedException();
        }

        public void ExportItemError(EwsResponseException ewsResponseError)
        {
            throw new NotSupportedException();
        }

        public void DeleteItemsToCatalog(IEnumerable<string> itemIds)
        {
            foreach (var i in itemIds)
            {
                var item = (from m in _updateContext.Items where m.ItemId == i select m).FirstOrDefault();
                if (item != null)
                {
                    _updateContext.Items.Remove(item);
                }
            }
            _updateContext.SaveChanges();
        }

        public Task DeleteItemsToCatalogAsync(IEnumerable<string> itemIds)
        {
            throw new NotSupportedException();
        }

        public void UpdateFolderToCatalog(IFolderDataSync folder)
        {
            var folderInfo = (from m in _updateContext.Folders where m.FolderId == folder.FolderId select m).FirstOrDefault();
            if (folderInfo == null)
                throw new ArgumentException("mailbox is not in catalog.");
            folderInfo.Clone(folder);
            _updateContext.SaveChanges();
        }

        public Task UpdateFolderToCatalogAsync(IFolderDataSync folder)
        {
            throw new NotSupportedException();
        }

        public void UpdateFolderSyncStatusToCatalog(IFolderDataSync folder)
        {
            var folderInfo = (from m in _updateContext.Folders where m.FolderId == folder.FolderId select m).FirstOrDefault();
            if (folderInfo == null)
                throw new ArgumentException("mailbox is not in catalog.");
            folderInfo.SyncStatus = folder.SyncStatus;
            _updateContext.SaveChanges();
        }

        public Task UpdateFolderSyncStatusToCatalogAsync(IFolderDataSync folder)
        {
            throw new NotSupportedException();
        }

        public void UpdateItemsToCatalog(IEnumerable<IItemDataSync> items)
        {
            foreach (var item in items)
            {
                var itemInDb = (from i in _updateContext.Items where i.ItemId == item.ItemId select i).FirstOrDefault();
                itemInDb.Clone(item);
            }
            _updateContext.SaveChanges();
        }

        public Task UpdateItemsToCatalogAsync(IEnumerable<IItemDataSync> items)
        {
            throw new NotSupportedException();
        }

        public Task UpdateMailboxSyncToCatalogAsync(IMailboxDataSync mailbox)
        {
            throw new NotSupportedException();
        }

        public void DeleteFolderToCatalog(string folderId)
        {
            var folder = (from m in _updateContext.Folders where m.FolderId == folderId select m).FirstOrDefault();
            if (folder != null)
            {
                _updateContext.Folders.Remove(folder);
                _updateContext.SaveChanges();
            }

            var items = (from m in _updateContext.Items where m.ParentFolderId == folderId select m);
            if(items.Count() > 0)
            {
                foreach(var i in items)
                {
                    _updateContext.Items.Remove(i);
                }
                _updateContext.SaveChanges();
            }
        }

        public Task DeleteFolderToCatalogAsync(string folderId)
        {
            throw new NotSupportedException();
        }

        public void UpdateMailboxToCatalog(ICollection<IMailboxDataSync> mailboxes)
        {
            foreach (var mailbox in mailboxes)
            {
                UpdateMailboxToCatalog(mailbox);
            }
        }

        public Task UpdateMailboxToCatalogAsync(ICollection<IMailboxDataSync> mailboxes)
        {
            throw new NotSupportedException();
        }

        public void DeleteMailboxToCatalog(ICollection<IMailboxDataSync> mailboxes)
        {
            foreach (var mailbox in mailboxes)
            {
                var mailboxInDb = (from m in _updateContext.Mailboxes where m.Id == mailbox.Id select m).FirstOrDefault();
                if (mailboxInDb != null)
                {
                    _updateContext.Mailboxes.Remove(mailboxInDb);
                }

                var folders = (from m in _updateContext.Folders where m.MailboxAddress == mailbox.MailAddress select m);
                if (folders.Count() > 0)
                {
                    foreach (var folder in folders)
                    {
                        _updateContext.Folders.Remove(folder);

                        var items = (from m in _updateContext.Items where m.ParentFolderId == folder.FolderId select m);
                        if (items.Count() > 0)
                        {
                            foreach (var i in items)
                            {
                                _updateContext.Items.Remove(i);
                            }
                            _updateContext.SaveChanges();
                        }
                    }
                }
            }
            _updateContext.SaveChanges();
        }

        public Task DeleteMailboxToCatalogAsync(ICollection<IMailboxDataSync> mailboxes)
        {
            throw new NotSupportedException();
        }

        public void UpdateMailboxToCatalog(IMailboxDataSync mailbox)
        {
            var mailboxInDb = (from m in _updateContext.Mailboxes where m.Id == mailbox.Id select m).FirstOrDefault();
            if (mailboxInDb != null)
            {
                mailboxInDb.Clone(mailbox);
                _updateContext.SaveChanges();
            }
        }

        public Task UpdateMailboxToCatalogAsync(IMailboxDataSync mailbox)
        {
            throw new NotImplementedException();
        }

        public void AddMailboxesToCatalog(IMailboxDataSync mailbox)
        {
            _updateContext.Mailboxes.Add(mailbox as MailboxSyncModel);
            SaveChanges();
        }

        public Task AddMailboxesToCatalogAsync(IMailboxDataSync mailbox)
        {
            throw new NotImplementedException();
        }
    }

    public class CatalogTestAccess : ICatalogAccess<IJobProgress>, ITaskSyncContext<IJobProgress>, IDisposable
    {
        private CatalogSyncDbContext _updateContext;
        private CatalogSyncDbContext _queryContext = null;
        private DataConvert _dataConvert;
        public CatalogTestAccess(string newCatalogFile, string lastCatalogFile, string organizationName)
        {
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

        }


        public void AddMailboxesToCatalog(IEnumerable<IMailboxDataSync> mailboxes)
        {

        }

        public Task AddMailboxesToCatalogAsync(IEnumerable<IMailboxDataSync> mailboxes)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IFolderDataSync> GetFoldersFromLatestCatalog(IMailboxDataSync mailboxData)
        {
            if (_queryContext != null)
                return (from folder in _queryContext.Folders where folder.MailboxAddress == mailboxData.MailAddress select folder).AsEnumerable();
            else
                return new List<IFolderDataSync>(0);
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
            if (_queryContext != null)
            {
                _queryContext.Dispose();
                _queryContext = null;
            }
        }

        private HashSet<string> itemsIdHash = new HashSet<string>();
        public void AddItemsToCatalog(IEnumerable<IItemDataSync> items)
        {

        }

        public Task AddItemsToCatalogAsync(IEnumerable<IItemDataSync> items)
        {
            throw new NotImplementedException();
        }


        public void AddFolderToCatalog(IFolderDataSync folder)
        {

        }

        public Task AddFolderToCatalogAsync(IFolderDataSync folder)
        {
            throw new NotImplementedException();
        }

        public void AddItemToCatalog(IItemDataSync item)
        {

        }
        public Task AddItemToCatalogAsync(IItemDataSync item)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IItemDataSync> GetItemsFromLatestCatalog(IEnumerable<string> itemIds)
        {
            if (_queryContext != null)
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
            if (_queryContext != null)
            {
                return (from item in _queryContext.Items where item.ParentFolderId == parentFolderId select item).AsEnumerable();
            }
            return new List<IItemDataSync>(0);
        }

        public Task<IEnumerable<IItemDataSync>> GetItemsByParentFolderIdFromCatalogAsync(string parentFolderId)
        {
            throw new NotImplementedException();
        }

        public void UpdateMailboxSyncToCatalog(IMailboxDataSync mailbox)
        {

        }

        public Task UpdateMailboxAsync(IMailboxDataSync mailbox)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IMailboxDataSync> GetMailboxesFromLatestCatalog(ICatalogJob catalogJob)
        {
            if (_queryContext != null)
                return (from mailbox in _queryContext.Mailboxes select mailbox).AsEnumerable();
            else
                return new List<IMailboxDataSync>(0);
        }

        public Task<IEnumerable<IMailboxDataSync>> GetMailboxFromLatestCatalogAsync(ICatalogJob catalogJob)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IMailboxDataSync> GetMailboxesFromLatestCatalog(DateTime currentJobStartTime)
        {
            if (_queryContext != null)
                return (from mailbox in _queryContext.Mailboxes select mailbox).AsEnumerable();
            else
                return new List<IMailboxDataSync>(0);
        }

        public Task<IEnumerable<IMailboxDataSync>> GetMailboxesFromLatestCatalogAsync(DateTime currentJobStartTime)
        {
            throw new NotImplementedException();
        }

        public void WriteBufferToStorage(IItemDataSync itemId, byte[] buffer, int length)
        {
            throw new NotImplementedException();
        }

        public void WriteComplete(IItemDataSync itemId)
        {
            throw new NotImplementedException();
        }

        public void ExportItemError(EwsResponseException ewsResponseError)
        {
            throw new NotImplementedException();
        }

        public void DeleteItemsToCatalog(IEnumerable<string> itemIds)
        {
            throw new NotImplementedException();
        }

        public Task DeleteItemsToCatalogAsync(IEnumerable<string> itemIds)
        {
            throw new NotImplementedException();
        }

        public void UpdateFolderToCatalog(IFolderDataSync folder)
        {
            throw new NotImplementedException();
        }

        public Task UpdateFolderToCatalogAsync(IFolderDataSync folder)
        {
            throw new NotImplementedException();
        }

        public void UpdateFolderSyncStatusToCatalog(IFolderDataSync folder)
        {
            throw new NotImplementedException();
        }

        public Task UpdateFolderSyncStatusToCatalogAsync(IFolderDataSync folder)
        {
            throw new NotImplementedException();
        }

        public void UpdateItemsToCatalog(IEnumerable<IItemDataSync> items)
        {
            throw new NotImplementedException();
        }

        public Task UpdateItemsToCatalogAsync(IEnumerable<IItemDataSync> items)
        {
            throw new NotImplementedException();
        }

        public Task UpdateMailboxSyncToCatalogAsync(IMailboxDataSync mailbox)
        {
            throw new NotImplementedException();
        }

        public void DeleteFolderToCatalog(string folderId)
        {
            throw new NotImplementedException();
        }

        public Task DeleteFolderToCatalogAsync(string folderId)
        {
            throw new NotImplementedException();
        }

        public void UpdateMailboxToCatalog(ICollection<IMailboxDataSync> mailboxes)
        {
            throw new NotImplementedException();
        }

        public Task UpdateMailboxToCatalogAsync(ICollection<IMailboxDataSync> mailboxes)
        {
            throw new NotImplementedException();
        }

        public void DeleteMailboxToCatalog(ICollection<IMailboxDataSync> mailboxes)
        {
            throw new NotImplementedException();
        }

        public Task DeleteMailboxToCatalogAsync(ICollection<IMailboxDataSync> mailboxes)
        {
            throw new NotImplementedException();
        }

        public void UpdateMailboxToCatalog(IMailboxDataSync mailbox)
        {
            throw new NotImplementedException();
        }

        public Task UpdateMailboxToCatalogAsync(IMailboxDataSync mailbox)
        {
            throw new NotImplementedException();
        }

        public void AddMailboxesToCatalog(IMailboxDataSync mailbox)
        {
            throw new NotImplementedException();
        }

        public Task AddMailboxesToCatalogAsync(IMailboxDataSync mailbox)
        {
            throw new NotImplementedException();
        }
    }
}
