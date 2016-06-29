using Arcserve.Office365.Exchange.DataProtect.Interface.Restore;
using Arcserve.Office365.Exchange.Thread;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arcserve.Office365.Exchange.Data.Increment;
using System.Threading;

namespace Arcserve.Office365.Exchange.StorageAccess.MountSession.Restore
{
    public class DataFromClient : IDataFromClientForRestore<IJobProgress>
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

        public IEnumerable<IFolderDataSync> GetAllFoldersFromPlan(IMailboxDataSync mailboxInfo, Func<IMailboxDataSync, IEnumerable<IFolderDataSync>> funcGetFolderFromCatalog)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IMailboxDataSync> GetAllMailboxFromPlan(Func<IEnumerable<IMailboxDataSync>> funcGetAllMailboxFromCatalog)
        {
            throw new NotImplementedException();
        }

        public ItemList GetFolderItems(IFolderDataSync folder, int offset, int pageCount, Func<IFolderDataSync, int, int, ItemList> getFolderItemsFromCatalog)
        {
            throw new NotImplementedException();
        }

        public bool ImportExistItems()
        {
            throw new NotImplementedException();
        }

        public bool ImportNotExistItems()
        {
            throw new NotImplementedException();
        }

        public void InitTaskSyncContext(ITaskSyncContext<IJobProgress> mainContext)
        {
            this.CloneSyncContext(mainContext);
        }
    }
}
