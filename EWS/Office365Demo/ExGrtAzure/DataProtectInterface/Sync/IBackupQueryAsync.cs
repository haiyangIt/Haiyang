using EwsDataInterface;
using EwsDataInterface.Sync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProtectInterface.Sync
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
