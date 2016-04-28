using Arcserve.Office365.Exchange.Data;
using Arcserve.Office365.Exchange.Data.Increment;
using Arcserve.Office365.Exchange.Thread;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.DataProtect.Interface.Backup.Increment
{
    public interface IBackupQueryAsync<ProgressType> : ITaskSyncContext<ProgressType>
    {
        Task<ICatalogJob> GetLastCatalogJobAsync(DateTime currentJobStartTime);
        Task<IMailboxDataSync> GetMailboxAsync(ICatalogJob catalogJob);
        Task<IMailboxDataSync> GetMailboxAsync(DateTime currentJobStartTime);

        ICatalogJob GetLastCatalogJob(DateTime currentJobStartTime);
        IMailboxDataSync GetMailbox(ICatalogJob catalogJob);
        IMailboxDataSync GetMailbox(DateTime currentJobStartTime);

        bool IsItemContentExist(string itemId);
    }
}
