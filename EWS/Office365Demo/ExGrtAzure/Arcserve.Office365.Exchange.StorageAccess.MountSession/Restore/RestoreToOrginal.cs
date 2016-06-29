using Arcserve.Office365.Exchange.DataProtect.Interface.Restore;
using Arcserve.Office365.Exchange.Thread;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arcserve.Office365.Exchange.Data.Account;
using Arcserve.Office365.Exchange.Data.Increment;
using System.Threading;

namespace Arcserve.Office365.Exchange.StorageAccess.MountSession.Restore
{
    public class RestoreToOrginal : IRestoreToPosition<IJobProgress>
    {
        public OrganizationAdminInfo AdminInfo
        {
            get; set;
        }

        public CancellationToken CancelToken
        {
            get; set;
        }

        public IExchangeAccessForRestore<IJobProgress> ExchangeAccessForRestore
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

        public void ConnectExchangeService()
        {
            
        }

        public IRestoreFolder GetAndCreateFolderIfFolderNotExist(IFolderDataSync folder, IRestoreFolder parentFolder)
        {
            throw new NotImplementedException();
        }

        public void ImportItems(IEnumerable<ImportItemStatus> partition, IRestoreFolder folder)
        {
            throw new NotImplementedException();
        }

        public void InitTaskSyncContext(ITaskSyncContext<IJobProgress> mainContext)
        {
            this.CloneSyncContext(mainContext);
        }
    }
}
