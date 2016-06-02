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

namespace Arcserve.Office365.Exchange.StorageAccess.MountSession
{
    public class DataFromClient : IDataFromClient<IJobProgress>
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

        public ICollection<IMailboxDataSync> GetAllMailboxes()
        {
            return new List<IMailboxDataSync>(1)
            {
                new MailboxDataSyncBase("Haiyang.Ling", "haiyang.ling@arcserve.com")
                {
                    Id = "ce7b5ec2-8732-4b85-a1bd-3196a2284bf2",
                    Name = "Haiyang.Ling"
                }
            };
        }

        public Task<ICollection<IMailboxDataSync>> GetAllMailboxesAsync()
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
