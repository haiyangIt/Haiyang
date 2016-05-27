using Arcserve.Office365.Exchange.DataProtect.Interface.Backup.Increment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arcserve.Office365.Exchange.Data;
using Arcserve.Office365.Exchange.Data.Increment;
using Arcserve.Office365.Exchange.Thread;
using System.Threading;
using Arcserve.Office365.Exchange.EwsApi.Increment;
using Arcserve.Office365.Exchange.StorageAccess.MountSession.EF;
using Arcserve.Office365.Exchange.Util.Setting;

namespace Arcserve.Office365.Exchange.StorageAccess.MountSession
{
    public class CatalogAccess : ICatalogAccess<IJobProgress>, IDisposable
    {
        private string CatalogFile;
        private string LatestCatalogFile;
        private string StorageFolder;
        private ICatalogAccess<IJobProgress> _catalogDbAccess;
        private ExportItemWriter _exportItemWriter;

        /// <summary>
        /// Dispose. please use using.
        /// </summary>
        /// <param name="newCatalogFile"></param>
        /// <param name="latestCatalogFile"></param>
        /// <param name="storageFolder"></param>
        /// <remarks>Dispose.</remarks>
        public CatalogAccess(string newCatalogFile, string latestCatalogFile, string storageFolder)
        {
            CatalogFile = newCatalogFile;
            LatestCatalogFile = latestCatalogFile;
            StorageFolder = storageFolder;
            if (CloudConfig.Instance.IsTestForDemo)
            {
                _catalogDbAccess = new CatalogTestAccess(newCatalogFile, latestCatalogFile);
            }
            else
                _catalogDbAccess = new CatalogDbAccess(newCatalogFile, latestCatalogFile);
            _catalogDbAccess.CloneSyncContext(this);
            _exportItemWriter = new ExportItemWriter(storageFolder);
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

        public void AddFolderToCatalog(IFolderDataSync folder)
        {
            _catalogDbAccess.AddFolderToCatalog(folder);
        }

        public Task AddFolderToCatalogAsync(IFolderDataSync folder)
        {
            throw new NotImplementedException();
        }

        public void AddItemsToCatalog(IEnumerable<IItemDataSync> items)
        {
            _catalogDbAccess.AddItemsToCatalog(items);
        }

        public Task AddItemsToCatalogAsync(IEnumerable<IItemDataSync> items)
        {
            throw new NotImplementedException();
        }

        public void AddItemToCatalog(IItemDataSync item)
        {
            _catalogDbAccess.AddItemToCatalog(item);
        }
        public Task AddItemToCatalogAsync(IItemDataSync item)
        {
            throw new NotImplementedException();
        }

        public void AddMailboxesToCatalog(IEnumerable<IMailboxDataSync> mailboxes)
        {
            _catalogDbAccess.AddMailboxesToCatalog(mailboxes);
        }

        public Task AddMailboxesToCatalogAsync(IEnumerable<IMailboxDataSync> mailboxes)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            if (_catalogDbAccess != null)
            {
                if (_catalogDbAccess is IDisposable)
                    ((IDisposable)_catalogDbAccess).Dispose();
                _catalogDbAccess = null;
            }
            if (_exportItemWriter != null)
            {
                _exportItemWriter.Dispose();
                _exportItemWriter = null;
            }
        }


        public IEnumerable<IFolderDataSync> GetFoldersFromLatestCatalog(IMailboxDataSync mailboxData)
        {
            return _catalogDbAccess.GetFoldersFromLatestCatalog(mailboxData);
        }

        public Task<IEnumerable<IFolderDataSync>> GetFoldersFromLatestCatalogAsync(IMailboxDataSync mailboxData)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IItemDataSync> GetItemsByParentFolderIdFromCatalog(string parentFolderId)
        {
            return _catalogDbAccess.GetItemsByParentFolderIdFromCatalog(parentFolderId);
        }

        public Task<IEnumerable<IItemDataSync>> GetItemsByParentFolderIdFromCatalogAsync(string parentFolderId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IItemDataSync> GetItemsFromLatestCatalog(IEnumerable<string> itemIds)
        {
            return _catalogDbAccess.GetItemsFromLatestCatalog(itemIds);
        }

        public Task<IEnumerable<IItemDataSync>> GetItemsFromLatestCatalogAsync(IEnumerable<string> itemIds)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IMailboxDataSync> GetMailboxesFromLatestCatalog(DateTime currentJobStartTime)
        {
            return _catalogDbAccess.GetMailboxesFromLatestCatalog(currentJobStartTime);
        }

        public IEnumerable<IMailboxDataSync> GetMailboxesFromLatestCatalog(ICatalogJob catalogJob)
        {
            return _catalogDbAccess.GetMailboxesFromLatestCatalog(catalogJob);
        }

        public Task<IEnumerable<IMailboxDataSync>> GetMailboxesFromLatestCatalogAsync(DateTime currentJobStartTime)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IMailboxDataSync>> GetMailboxFromLatestCatalogAsync(ICatalogJob catalogJob)
        {
            throw new NotImplementedException();
        }

        public void InitTaskSyncContext(ITaskSyncContext<IJobProgress> mainContext)
        {
            this.CloneSyncContext(mainContext);
            _catalogDbAccess.CloneSyncContext(mainContext);
        }

        public bool IsItemContentExist(string itemId)
        {
            return _catalogDbAccess.IsItemContentExist(itemId);
        }

        public void UpdateMailbox(IMailboxDataSync mailbox)
        {
            _catalogDbAccess.UpdateMailbox(mailbox);
        }

        public Task UpdateMailboxAsync(IMailboxDataSync mailbox)
        {
            throw new NotImplementedException();
        }

        public void WriteBufferToStorage(IItemDataSync item, byte[] buffer, int length)
        {
            _exportItemWriter.WriteBufferToStorage(item, buffer, length);
        }

        public void WriteComplete(IItemDataSync item)
        {
            _exportItemWriter.WriteComplete(item);
        }
        public void ExportItemError(EwsResponseException ewsResponseError)
        {
            _exportItemWriter.ExportItemError(ewsResponseError);
        }
    }
}
