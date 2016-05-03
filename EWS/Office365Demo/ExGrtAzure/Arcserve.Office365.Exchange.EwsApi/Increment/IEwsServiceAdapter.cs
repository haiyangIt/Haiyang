using Arcserve.Office365.Exchange.Data.Account;
using Arcserve.Office365.Exchange.Data.Increment;
using Arcserve.Office365.Exchange.Thread;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.EwsApi.Increment
{
    public interface IEwsServiceAdapter<ProgressType> : ITaskSyncContext<ProgressType>
    {
        Task<Microsoft.Exchange.WebServices.Data.ExchangeService> GetExchangeServiceSync(string mailbox, OrganizationAdminInfo adminInfo);
        Microsoft.Exchange.WebServices.Data.ExchangeService GetExchangeService(string mailbox, OrganizationAdminInfo adminInfo);

        Task<ICollection<IMailboxDataSync>> GetAllMailboxesSync(string adminUserName, string adminPassword);
        ICollection<IMailboxDataSync> GetAllMailboxes(string adminUserName, string adminPassword);


    }
}
