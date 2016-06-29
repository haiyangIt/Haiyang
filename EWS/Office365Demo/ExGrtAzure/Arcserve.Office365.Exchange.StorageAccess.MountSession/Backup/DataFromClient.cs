using Arcserve.Office365.Exchange.DataProtect.Interface.Backup;
using Arcserve.Office365.Exchange.Thread;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arcserve.Office365.Exchange.Data;
using Arcserve.Office365.Exchange.Data.Increment;
using System.Threading;
using Arcserve.Office365.Exchange.Data.Mail;

namespace Arcserve.Office365.Exchange.StorageAccess.MountSession.Backup
{
    public class DataFromClient : IDataFromBackup<IJobProgress>
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

        public ICollection<IMailboxDataSync> GetAllMailboxFromPlanAndExchange(Func<IEnumerable<string>, ICollection<IMailboxDataSync>> funcGetAllMailboxFromExchange)
        {
            return funcGetAllMailboxFromExchange(new List<string>() { "haiyang.ling@arcserve.com" });
        }

        public Task<ICollection<IMailboxDataSync>> GetAllMailboxesAsync(Func<IEnumerable<string>, Task<ICollection<IMailboxDataSync>>> funcGetAllMailboxFromExchange)
        {
            throw new NotImplementedException();
        }


        public ICatalogJob GetLatestCatalogJob()
        {
            return null;
        }

        public Task<ICatalogJob> GetLatestCatalogJobAsync()
        {
            throw new NotImplementedException();
        }

        public void InitTaskSyncContext(ITaskSyncContext<IJobProgress> mainContext)
        {
            this.CloneSyncContext(mainContext);
        }

        public bool IsFolderClassValid(string folderClass)
        {
            return FolderClassUtil.IsFolderValid(folderClass);
        }

        public bool IsFolderInPlan(IFolderDataSync folderData)
        {
            return true;
        }

        public bool IsFolderInPlan(string uniqueFolderId)
        {
            return true;
        }

        public Task<bool> IsFolderInPlanAsync(string uniqueFolderId)
        {
            throw new NotImplementedException();
        }

        public bool IsItemValid(string itemChangeId, IFolderDataSync parentFolder)
        {
            return true;
        }

        public bool IsItemValid(IItemDataSync item, IFolderDataSync parentFolder)
        {
            return true;
        }
    }
}
