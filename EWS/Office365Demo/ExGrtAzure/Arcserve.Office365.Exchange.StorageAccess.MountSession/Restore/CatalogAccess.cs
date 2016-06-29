using Arcserve.Office365.Exchange.DataProtect.Interface.Restore;
using Arcserve.Office365.Exchange.Thread;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arcserve.Office365.Exchange.Data.Increment;
using System.Threading;
using Arcserve.Office365.Exchange.StorageAccess.MountSession.EF;

namespace Arcserve.Office365.Exchange.StorageAccess.MountSession.Restore
{
    public class CatalogAccess : ICatalogAccessForRestore<IJobProgress>, IDisposable
    {
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

        public CatalogAccess(string catalogFile)
        {
            _queryContext = CatalogDbContextBase.NewCatalogContext(catalogFile);
        }

        public CatalogDbContextBase _queryContext = null;

        public IEnumerable<IMailboxDataSync> GetAllMailboxFromCatalog()
        {
            return _queryContext.Mailboxes;
        }

        public ItemList GetFolderItemsFromCatalog(IFolderDataSync folder, int offset, int pageCount)
        {
            var result = (from s in _queryContext.Items where s.ParentFolderId == folder.FolderId orderby s.CreateTime descending select s).Skip(offset).Take(pageCount);

            return new ItemList()
            {
                Items = result,
                MoreAvailable = (offset + pageCount) < folder.ChildItemCount,
                NextOffset = offset + pageCount
            };
        }

        public IEnumerable<IFolderDataSync> GetFoldersFromCatalog(IMailboxDataSync mailboxInfo)
        {
            return from s in _queryContext.Folders where s.MailboxId == mailboxInfo.Id select s;
        }

        public void InitTaskSyncContext(ITaskSyncContext<IJobProgress> mainContext)
        {
            this.CloneSyncContext(mainContext);
        }

        public void Dispose()
        {
            if(_queryContext != null)
            {
                _queryContext.Dispose();
                _queryContext = null;
            }
        }
    }
}
