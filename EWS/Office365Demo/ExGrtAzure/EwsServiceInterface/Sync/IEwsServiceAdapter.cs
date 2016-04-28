using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EwsServiceInterface.Sync
{
    public interface IEwsServiceAdapter<ProgressType> : ITaskSyncContext<ProgressType>
    {
        Task<Microsoft.Exchange.WebServices.Data.ExchangeService> GetExchangeService(string mailbox, OrganizationAdminInfo adminInfo);
    }
}
