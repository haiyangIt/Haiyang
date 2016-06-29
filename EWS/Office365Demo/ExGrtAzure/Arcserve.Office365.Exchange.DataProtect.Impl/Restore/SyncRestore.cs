using Arcserve.Office365.Exchange.DataProtect.Interface.Backup;
using Arcserve.Office365.Exchange.DataProtect.Interface.Restore;
using Arcserve.Office365.Exchange.Thread;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arcserve.Office365.Exchange.Data.Increment;
using Arcserve.Office365.Exchange.EwsApi.Interface;
using Arcserve.Office365.Exchange.Data.Account;

namespace Arcserve.Office365.Exchange.DataProtect.Impl.Restore
{
    public class SyncRestore : RestoreFlowTemplate, ITaskSyncContext<IJobProgress>, IExchangeAccessForRestore<IJobProgress>
    {
        public ICatalogAccessForRestore<IJobProgress> CatalogAccess
        {
            get; set;
        }
        

        public IDataFromClientForRestore<IJobProgress> DataFromClient
        {
            get; set;
        }

        public IEwsServiceAdapter<IJobProgress> EwsServiceAdapter
        {
            get; set;
        }
        public OrganizationAdminInfo AdminInfo { get; set; }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }
        

        protected override void ForEachMailbox(IEnumerable<IMailboxDataSync> mailboxes, Action<IMailboxDataSync> actionDealMailbox)
        {
            foreach(var mailbox in mailboxes)
            {
                actionDealMailbox(mailbox);
            }
        }

        protected override IEnumerable<IMailboxDataSync> GetAllMailboxFromCatalog()
        {
            return CatalogAccess.GetAllMailboxFromCatalog();
        }

        protected override IEnumerable<IMailboxDataSync> GetAllMailboxFromPlan(Func<IEnumerable<IMailboxDataSync>> funcGetAllMailboxFromCatalog)
        {
            return DataFromClient.GetAllMailboxFromPlan(funcGetAllMailboxFromCatalog);
        }

        protected override RestoreMailboxFlowTemplate NewRestoreMailboxFlowTemplate()
        {
            var result = new SyncRestoreMailbox();
            result.CloneSyncContext(this);
            result.CloneExchangeAccess(this);
            result.AdminInfo = AdminInfo;
            return result;
        }
    }
}
