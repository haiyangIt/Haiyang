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
    public class EwsServiceAdapter : IEwsServiceAdapter<JobProgress>
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

        public JobProgress Progress
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
            System.Threading.Tasks.Task t = new System.Threading.Tasks.Task();
        }

        public async Task<ICollection<IMailboxDataSync>> GetAllMailboxesSync(string adminUserName, string adminPassword)
        {
            throw new NotImplementedException();
        }

        public ExchangeService GetExchangeService(string mailbox, OrganizationAdminInfo adminInfo)
        {
            throw new NotImplementedException();
        }

        public Task<ExchangeService> GetExchangeServiceSync(string mailbox, OrganizationAdminInfo adminInfo)
        {
            throw new NotImplementedException();
        }

        public void InitTaskSyncContext(ITaskSyncContext<JobProgress> mainContext)
        {
            throw new NotImplementedException();
        }
    }
}
