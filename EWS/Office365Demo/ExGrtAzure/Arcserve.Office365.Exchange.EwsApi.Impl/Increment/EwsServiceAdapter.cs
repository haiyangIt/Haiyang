using Arcserve.Office365.Exchange.EwsApi.Increment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arcserve.Office365.Exchange.Data.Account;
using Arcserve.Office365.Exchange.Data.Increment;
using Arcserve.Office365.Exchange.Thread;
using Microsoft.Exchange.WebServices.Data;
using System.Threading;

namespace Arcserve.Office365.Exchange.EwsApi.Impl.Increment
{
    public class EwsServiceAdapter : IEwsServiceAdapter<IJobProgress>
    {
        public CancellationToken CancelToken
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public IJobProgress Progress
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public TaskScheduler Scheduler
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public ICollection<IMailboxDataSync> GetAllMailboxes(string adminUserName, string adminPassword)
        {
            var result = DoGetAllMailboxes(adminUserName, adminPassword);
            Progress.Report("Getting all mailboxes in exchange completed, total {0} mailboxes.", result.Count);
            return result;
        }

        public async Task<ICollection<IMailboxDataSync>> GetAllMailboxesAsync(string adminUserName, string adminPassword)
        {
            throw new NotImplementedException();
        }

        private ICollection<IMailboxDataSync> DoGetAllMailboxes(string adminUserName, string adminPassword)
        {

        }

        public ExchangeService GetExchangeService(string mailbox, OrganizationAdminInfo adminInfo)
        {
            throw new NotImplementedException();
        }

        public Task<ExchangeService> GetExchangeServiceAsync(string mailbox, OrganizationAdminInfo adminInfo)
        {
            throw new NotImplementedException();
        }

        public void InitTaskSyncContext(ITaskSyncContext<IJobProgress> mainContext)
        {
            throw new NotImplementedException();
        }
    }
}
