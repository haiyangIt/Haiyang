using DataProtectInterface;
using EwsDataInterface;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EwsWSData = Microsoft.Exchange.WebServices.Data;
using EwsWS = Microsoft.Exchange.WebServices;

namespace DataProtectImpl
{
    public class AsyncBackup : ITaskSyncContext<JobProgress>
    {
        public IFilterItemWithFolderAsync<JobProgress> Filters { get; set; }
        public JobProgress Progress { get; set; }
        public TaskScheduler Scheduler { get; set; }
        public CancellationToken CancelToken { get; set; }
        public OrganizationAdminInfo AdminInfo { get; set; }
        public string Organization { get; }
        public IBackupQuerySync<JobProgress> BackupQuery { get; }
        public IEwsServiceAdapter<JobProgress> EwsServiceAdapter { get; set; }
        public DateTime JobStartTime { get; }
        public async System.Threading.Tasks.Task Run()
        {
            var allMailboxFromPlan = Filters.GetAllMailboxAsync();
            var lastMailboxSyncFromDb = BackupQuery.GetMailboxSync(JobStartTime);
            await System.Threading.Tasks.Task.WhenAll(allMailboxFromPlan, lastMailboxSyncFromDb).ConfigureAwait(false);
            throw new NotImplementedException();
        }

        
    }

    public abstract class AsyncBackupFlowTemplate : ITaskSyncContext<JobProgress>
    {
        public abstract JobProgress Progress
        {
            get; set;
        }

        public abstract CancellationToken CancelToken
        {
            get; set;
        }

        public TaskScheduler Scheduler
        {
            get; set;
        }

        public virtual async Task<IList<IMailboxDataSync>> TaskForAllMailboxFromPlan()
        {
            throw new NotSupportedException();
        }
        public virtual async Task<IList<IMailboxDataSync>> TaskForAllMailboxFromDb()
        {
            throw new NotSupportedException();
        }

        public virtual async Task<IList<EwsWSData.FolderChange>> TaskForFoldersChanges(IMailboxDataSync mailboxData, EwsWSData.ExchangeService exchangeService)
        {
            throw new NotSupportedException();
        }

        public virtual async Task<EwsWSData.ExchangeService> GetTaskForGetExchangeService(IMailboxDataSync mailboxData)
        {
            throw new NotSupportedException();
        }

        public virtual async Task<IFolderDataSync> GetLastFolderCatalogFromDb()
        {
            throw new NotSupportedException();
        }

        public abstract int ConcurrentMailboxNumber { set; protected get; }

        public async Task Run()
        {
            var getMailboxTasks = new[] { TaskForAllMailboxFromDb(), TaskForAllMailboxFromPlan()};
            var result = await Task.WhenAll(getMailboxTasks);
            var mailboxInDb = result[0];
            var mailboxInPlan = result[1];

            BlockingCollection<IMailboxDataSync> _blockingQueue = new BlockingCollection<IMailboxDataSync>(ConcurrentMailboxNumber);
            foreach(var mailbox in mailboxInPlan)
            {
                _blockingQueue.Add(mailbox);
            }
            _blockingQueue.CompleteAdding();

            var dealEachMailboxTask = new Task<IMailboxDataSync>[mailboxInPlan.Count];
            for(int i = 0; i < mailboxInPlan.Count; i++)
            {
                dealEachMailboxTask[i] = DealEachMailbox(_blockingQueue);
            }
            var mailboxDealResult = await Task.WhenAll(dealEachMailboxTask);
        }

        private async Task<IMailboxDataSync> DealEachMailbox(BlockingCollection<IMailboxDataSync> mailboxQueue)
        {
            var mailboxData = mailboxQueue.Take();
            var exchangeService = await GetTaskForGetExchangeService(mailboxData);

            var folderChanges = await TaskForFoldersChanges(mailboxData, exchangeService);

            // 1. for each folder change, to get last folder catalog. and change catalog by new changes and update database.

            // 2. for each folder change, to get item sync

            throw new NotImplementedException();
        }
    }

    public interface ITaskSyncContext<ProgressType>
    {
        JobProgress Progress { get; set; }
        CancellationToken CancelToken { get; set; }
        TaskScheduler Scheduler { get; set; }
    }

    public interface JobProgress : IProgress<double>
    {
        void Report(double val, string message);
        void Report(string message);
    }

    public interface IDataSync
    {
        string SyncStatus { get; }

        string ChangeKey { get; }
    }

    public interface IBackupQuerySync<ProgressType> : ITaskSyncContext<ProgressType>
    {
        Task<ICatalogJob> GetLastCatalogJob(DateTime currentJobStartTime);
        Task<IMailboxDataSync> GetMailboxSync(ICatalogJob catalogJob);
        Task<IMailboxDataSync> GetMailboxSync(DateTime currentJobStartTime);

        bool IsItemContentExist(string itemId);
    }

    public interface IMailboxDataSync : IMailboxData, IDataSync
    {
    }

    public interface IFolderDataSync : IFolderData, IDataSync
    {
    }

    public interface IItemDataSync: IItemData, IDataSync
    {
    }

    public interface IEwsServiceAdapter<ProgressType> : ITaskSyncContext<ProgressType>
    {
        Task<Microsoft.Exchange.WebServices.Data.ExchangeService> GetExchangeService(string mailbox, OrganizationAdminInfo adminInfo);
    }
    public interface IFilterItemSync<ProgressType> : ITaskSyncContext<ProgressType>
    {
        bool IsFilterMailbox(IMailboxDataSync mailbox);
        bool IsFilterFolder(IFolderDataSync currentFolder, IMailboxDataSync mailbox, Stack<IFolderDataSync> folders);
        bool IsFilterItem(IItemDataSync item, IMailboxDataSync mailbox, Stack<IFolderDataSync> folders);
    }
    public interface IFilterItemWithMailboxAsync<ProgressType> : IFilterItemSync<ProgressType>
    {
        List<IMailboxDataSync> GetAllMailbox();
        Task<List<IMailboxDataSync>> GetAllMailboxAsync();
    }

    public interface IFilterItemWithFolderAsync<ProgressType> : IFilterItemWithMailboxAsync<ProgressType>
    {
        List<IFolderDataSync> GetAllFolders(IMailboxDataSync mailboxData);
        Task<List<IFolderDataSync>> GetAllFoldersAsync(IMailboxDataSync mailboxData);
    }
}
