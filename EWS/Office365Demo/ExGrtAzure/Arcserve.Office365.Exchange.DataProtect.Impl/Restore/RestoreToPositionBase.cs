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

namespace Arcserve.Office365.Exchange.DataProtect.Impl.Restore
{
    public abstract class RestoreToPositionBase : IRestoreToPosition<IJobProgress>
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
            if (IsConnectExchangeService())
            {
                ConnectExchangeService(ExchangeAccessForRestore.EwsServiceAdapter.GetExchangeService);
            }
        }

        public abstract IRestoreDestinationFolder GetAndCreateFolderIfFolderNotExist(IFolderDataSync folder, IRestoreDestinationFolder parentFolder);

        public abstract IEnumerable<string> GetNotExistItems(IRestoreDestinationFolder destinationFolder, IFolderDataSync folderInCatalog);

        public abstract bool ImportExistItems();

        public abstract void ImportItems(IEnumerable<ImportItemStatus> partition, IRestoreDestinationFolder folder);

        public abstract bool ImportNotExistItems();

        public void InitTaskSyncContext(ITaskSyncContext<IJobProgress> mainContext)
        {
            this.CloneSyncContext(mainContext);
        }

        public abstract IRestoreToPosition<IJobProgress> NewRestoreToPosition(IMailboxDataSync currentRestoreMailbox);

        protected void CloneBaseMembers(RestoreToPositionBase newObj)
        {
            newObj.CloneSyncContext(this);
        }

        protected abstract void ConnectExchangeService(Func<string, OrganizationAdminInfo, string> funcConnectExchangeService);

        protected abstract bool IsConnectExchangeService();
    }
}
