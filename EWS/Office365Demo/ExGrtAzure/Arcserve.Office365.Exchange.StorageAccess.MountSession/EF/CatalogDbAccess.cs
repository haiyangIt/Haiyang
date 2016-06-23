using Arcserve.Office365.Exchange.Data;
using Arcserve.Office365.Exchange.Data.Increment;
using Arcserve.Office365.Exchange.DataProtect.Interface.Backup;
using Arcserve.Office365.Exchange.StorageAccess.MountSession.EF.Data;
using Arcserve.Office365.Exchange.Thread;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Arcserve.Office365.Exchange.Util;
using System.Data.Entity.Infrastructure;
using Arcserve.Office365.Exchange.Util.Setting;
using System.Data.SqlClient;
using System.Diagnostics;

namespace Arcserve.Office365.Exchange.StorageAccess.MountSession.EF
{
    public class CatalogDbAccess : ICatalogAccess<IJobProgress>, ITaskSyncContext<IJobProgress>, IDisposable
    {
        private CatalogSyncDbContext _updateContext;
        private CatalogSyncDbContext _queryContext = null;
        private DataConvert _dataConvert;
        public CatalogDbAccess(string newCatalogFolder, string lastCatalogFolder, string organizationName)
        {
            if (!Directory.Exists(newCatalogFolder))
            {
                Directory.CreateDirectory(newCatalogFolder);
            }

            string fileName = string.Empty;
            string newCatalogFileName = string.Empty;
            if (!string.IsNullOrEmpty(lastCatalogFolder))
            {
                fileName = CatalogAccess.GetCatalogFileName(organizationName);
                newCatalogFileName = Path.Combine(newCatalogFolder, fileName);
                var lastCatalogFile = Path.Combine(lastCatalogFolder, fileName);
                File.Copy(lastCatalogFile, newCatalogFileName);
                var logFile = Path.GetFileNameWithoutExtension(fileName);
                logFile = string.Format("{0}_log.ldf", logFile);

                var lastLogFilePath = Path.Combine(lastCatalogFolder, logFile);
                var newLogFilePath = Path.Combine(newCatalogFolder, logFile);
                File.Copy(lastLogFilePath, newLogFilePath);

                _queryContext = new CatalogSyncDbContext(lastCatalogFile);
            }
            else
            {
                fileName = CatalogAccess.GetCatalogFileName(organizationName);
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
                SqlConnection.ClearPool(_updateContext.Database.Connection as SqlConnection);
                _updateContext.Dispose();
                
                _updateContext = null;
                
            }
            if (_queryContext != null)
            {
                SqlConnection.ClearPool(_queryContext.Database.Connection as SqlConnection);
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

        public void UpdateMailboxSyncAndTreeToCatalog(IMailboxDataSync mailbox)
        {
            DoSaveChanges();
            var mailboxInfo = (from m in _updateContext.Mailboxes where m.Id == mailbox.Id select m).FirstOrDefault();
            if (mailboxInfo == null)
                throw new ArgumentException("mailbox is not in catalog.");
            mailboxInfo.SyncStatus = mailbox.SyncStatus;
            mailboxInfo.FolderTree = mailbox.FolderTree;
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

            var itemCount = (from m in _updateContext.Items where m.ParentFolderId == folder.FolderId select m).Count();
            folderInfo.ChildItemCount = itemCount;

            var childFolderCount = (from m in _updateContext.Folders where m.ParentFolderId == folder.FolderId select m).Count();
            folderInfo.ChildFolderCount = childFolderCount;

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

        public Task UpdateMailboxSyncAndTreeToCatalogAsync(IMailboxDataSync mailbox)
        {
            throw new NotSupportedException();
        }

        public void DeleteFolderToCatalog(string folderId)
        {
            var folder = (from m in _updateContext.Folders where m.FolderId == folderId select m).FirstOrDefault();
            if (folder != null)
            {
                _updateContext.Folders.Remove(folder);
                int i = _updateContext.SaveChanges();
                Debug.Write(String.Format("delete {0} folder record.", i));
            }

            var items = (from m in _updateContext.Items where m.ParentFolderId == folderId select m);
            if(items.Count() > 0)
            {
                
                foreach(var i in items)
                {
                    _updateContext.Items.Remove(i);
                }
                int deleteCount = _updateContext.SaveChanges();
                Debug.Write(String.Format("delete {0} item record.", deleteCount));
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
        public CatalogTestAccess(string newCatalogFile, string lastCatalogFolder, string organizationName)
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

        public void UpdateMailboxSyncAndTreeToCatalog(IMailboxDataSync mailbox)
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
            
        }

        public Task DeleteItemsToCatalogAsync(IEnumerable<string> itemIds)
        {
            throw new NotImplementedException();
        }

        public void UpdateFolderToCatalog(IFolderDataSync folder)
        {
           
        }

        public Task UpdateFolderToCatalogAsync(IFolderDataSync folder)
        {
            throw new NotImplementedException();
        }

        public void UpdateFolderSyncStatusToCatalog(IFolderDataSync folder)
        {
            
        }

        public Task UpdateFolderSyncStatusToCatalogAsync(IFolderDataSync folder)
        {
            throw new NotImplementedException();
        }

        public void UpdateItemsToCatalog(IEnumerable<IItemDataSync> items)
        {
            
        }

        public Task UpdateItemsToCatalogAsync(IEnumerable<IItemDataSync> items)
        {
            throw new NotImplementedException();
        }

        public Task UpdateMailboxSyncAndTreeToCatalogAsync(IMailboxDataSync mailbox)
        {
            throw new NotImplementedException();
        }

        public void DeleteFolderToCatalog(string folderId)
        {
            
        }

        public Task DeleteFolderToCatalogAsync(string folderId)
        {
            throw new NotImplementedException();
        }

        public void UpdateMailboxToCatalog(ICollection<IMailboxDataSync> mailboxes)
        {
            
        }

        public Task UpdateMailboxToCatalogAsync(ICollection<IMailboxDataSync> mailboxes)
        {
            throw new NotImplementedException();
        }

        public void DeleteMailboxToCatalog(ICollection<IMailboxDataSync> mailboxes)
        {
            
        }

        public Task DeleteMailboxToCatalogAsync(ICollection<IMailboxDataSync> mailboxes)
        {
            throw new NotImplementedException();
        }

        public void UpdateMailboxToCatalog(IMailboxDataSync mailbox)
        {
            
        }

        public Task UpdateMailboxToCatalogAsync(IMailboxDataSync mailbox)
        {
            throw new NotImplementedException();
        }

        public void AddMailboxesToCatalog(IMailboxDataSync mailbox)
        {
            
        }

        public Task AddMailboxesToCatalogAsync(IMailboxDataSync mailbox)
        {
            throw new NotImplementedException();
        }
    }
}
