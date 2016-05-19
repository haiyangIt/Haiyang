using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EwsWSData = Microsoft.Exchange.WebServices.Data;
using EwsWS = Microsoft.Exchange.WebServices;
using Arcserve.Office365.Exchange.Thread;
using Arcserve.Office365.Exchange.Data.Account;
using Arcserve.Office365.Exchange.EwsApi.Increment;
using Arcserve.Office365.Exchange.Data.Increment;

namespace Arcserve.Office365.Exchange.DataProtect.Interface.Backup.Increment
{
    public class AsyncBackup : ITaskSyncContext<IJobProgress>
    {
        public IFilterItemWithFolderAsync<IJobProgress> Filters { get; set; }
        public IJobProgress Progress { get; set; }
        public TaskScheduler Scheduler { get; set; }
        public CancellationToken CancelToken { get; set; }
        public OrganizationAdminInfo AdminInfo { get; set; }
        public string Organization { get; }
        public ICatalogAccess<IJobProgress> BackupQuery { get; }
        public IEwsServiceAdapter<IJobProgress> EwsServiceAdapter { get; set; }
        public DateTime JobStartTime { get; }
        public async System.Threading.Tasks.Task Run()
        {
            //var allMailboxFromPlan = Filters.GetAllMailboxAsync();
            //var lastMailboxSyncFromDb = BackupQuery.GetMailboxSync(JobStartTime);
            //await System.Threading.Tasks.Task.WhenAll(allMailboxFromPlan, lastMailboxSyncFromDb).ConfigureAwait(false);
            throw new NotImplementedException();
        }

        public void InitTaskSyncContext(ITaskSyncContext<IJobProgress> mainContext)
        {
            this.CloneSyncContext(mainContext);
        }
    }

    public abstract class AsyncBackupFlowTemplate : ITaskSyncContext<IJobProgress>
    {
        public abstract IJobProgress Progress
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

        public abstract Task<IList<IMailboxDataSync>> TaskGetAllMailboxFromPlan { get; }

        public abstract Task<IList<IMailboxDataSync>> TaskGetAllMailboxFromLastJobCatalog { get; }

        public abstract Task<IList<IMailboxDataSync>> TaskGetAllMailboxFromExchange
        {
            get;
        }

        public abstract Task<IList<EwsWSData.FolderChange>> TaskGetFoldersChangesFromEws { get; }

        public abstract Task<EwsWSData.ExchangeService> TaskGetExchangeService { get; }

        public abstract Task<IFolderDataSync> TaskGetLastFolderCatalogFromDb
        {
            get;
        }

        public abstract Task<IList<IFolderDataSync>> TaskGetAllFoldersFromCatalog
        {
            get;
        }


        public abstract int ConcurrentMailboxNumber { set; protected get; }

        public async Task Run()
        {
            var getMailboxTasks = new[] { TaskGetAllMailboxFromPlan, TaskGetAllMailboxFromLastJobCatalog, TaskGetAllMailboxFromExchange };
            foreach (var task in getMailboxTasks)
            {
                if (task.Status == TaskStatus.Created)
                {
                    task.Start();
                    await task.ConfigureAwait(false);
                    await task;
                }
            }

            var result = await Task.WhenAll(getMailboxTasks);
            var mailboxInDb = result[0];
            var mailboxInPlan = result[1];
            var mailboxInExchanges = result[2];

            // todo. update catalog file and update database.

            BlockingCollection<IMailboxDataSync> _blockingQueue = new BlockingCollection<IMailboxDataSync>(ConcurrentMailboxNumber);
            foreach (var mailbox in mailboxInPlan)
            {
                _blockingQueue.Add(mailbox);
            }
            _blockingQueue.CompleteAdding();

            var dealEachMailboxTask = new Task<IMailboxDataSync>[mailboxInPlan.Count];
            for (int i = 0; i < mailboxInPlan.Count; i++)
            {
                dealEachMailboxTask[i] = DealEachMailbox(_blockingQueue);
            }
            var mailboxDealResult = await Task.WhenAll(dealEachMailboxTask);
        }

        private async Task<IMailboxDataSync> DealEachMailbox(BlockingCollection<IMailboxDataSync> mailboxQueue)
        {
            var mailboxData = mailboxQueue.Take();

            // get all folders from last complete job from catalog file.
            //var folderInLastJobTask = TaskForAllFoldersFromCatalog(mailboxData);

            //var exchangeServiceTask = GetTaskForGetExchangeService(mailboxData);

            //var folderChangesTaskContinue = TaskContinuaFolderChanges(exchangeServiceTask, mailboxData);

            //var continua = await Task.WhenAll(folderInLastJobTask, folderChangesTaskContinue);

            //var exchangeService = exchangeServiceTask.Result;
            // 1. first we need update folders catalog by lastJobTask and folderChanges.

            //var updateFoldersCatalogTask = TaskForUpdateFolderCatalog(continua[0], continua[1], exchangeService);

            // 2. loop each folder, update items


            // 1. for each folder change, to get last folder catalog. and change catalog by new changes and update database.

            // 2. for each folder change, to get item sync

            throw new NotImplementedException();
        }

        public void InitTaskSyncContext(ITaskSyncContext<IJobProgress> mainContext)
        {
            throw new NotImplementedException();
        }
    }

    
}
