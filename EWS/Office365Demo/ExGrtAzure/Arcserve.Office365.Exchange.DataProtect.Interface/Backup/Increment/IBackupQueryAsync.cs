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
        Task<ICollection<IMailboxDataSync>> GetMailboxAsync(ICatalogJob catalogJob);
        Task<ICollection<IMailboxDataSync>> GetMailboxAsync(DateTime currentJobStartTime);

        ICatalogJob GetLastCatalogJob(DateTime currentJobStartTime);
        ICollection<IMailboxDataSync> GetMailboxes(ICatalogJob catalogJob);
        ICollection<IMailboxDataSync> GetMailboxes(DateTime currentJobStartTime);

        bool IsItemContentExist(string itemId);
    }
}
