using Arcserve.Office365.Exchange.DataProtect.Interface.Backup;
using Arcserve.Office365.Exchange.Thread;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Arcserve.Office365.Exchange.Data.Account;
using Arcserve.Office365.Exchange.EwsApi.Interface;
using Arcserve.Office365.Exchange.Data;
using Arcserve.Office365.Exchange.Data.Increment;
using Arcserve.Office365.Exchange.Util.Setting;
using Arcserve.Office365.Exchange.Log;

namespace Arcserve.Office365.Exchange.DataProtect.Impl.AsyncBackup
{
    public class AsyncBackup : BackupFlowAsyncTemplate, ITaskSyncContext<IJobProgress>, IExchangeAccess<IJobProgress>
    {
        public async Task BackupAsync()
        {
            try
            {
                var mailboxesInPlanTask = GetAllMailboxFromPlan(GetAllMailboxFromExchange);
                var catalogJobTask = GetLatestCatalogJob();

                await Task.WhenAll(mailboxesInPlanTask, catalogJobTask);

                var mailboxesInExchangeTask = GetAllMailboxFromExchange(from m in mailboxesInPlanTask.Result select m.MailAddress);
                var mailboxesInLastCatalogTask = GetAllMailboxFromLastCatalog(catalogJobTask.Result);

                await Task.WhenAll(mailboxesInExchangeTask, mailboxesInLastCatalogTask);

                var mailboxesValid = GetIntersectionMailboxCollection(mailboxesInExchangeTask.Result, mailboxesInPlanTask.Result);
                var mailboxesUAD = GetMailboxUAD(mailboxesValid, mailboxesInLastCatalogTask.Result);

                var deleteMailboxTask = DeleteMailboxToCurrentCatalog(mailboxesUAD[ItemUADStatus.Delete]);

                // todo test case: if a mailbox delete and a new mailbox whose name is same as deleted mailbox create. how to generate catalog.
                List<Task> allTask = new List<Task>();
                allTask.Add(deleteMailboxTask);
                foreach (var mailboxesItem in mailboxesUAD)
                {
                    var eachMailboxTask = ForEachLoop(mailboxesItem.Value, mailboxesItem.Key, (mailbox, uadStatus) =>
                    {
                        return DoEachMailbox(mailbox, uadStatus);
                    });

                    allTask.Add(eachMailboxTask);
                }

                await Task.WhenAll(allTask.ToArray());
            }
            catch (Exception e)
            {
                LogFactory.LogInstance.WriteException(LogLevel.DEBUG, "Generate catalog End with error", e, e.Message);
                throw new Exception(e.Message, e);
            }
            finally
            {
            }
        }

        public IJobProgress Progress
        {
            get; set;
        }

        public CancellationToken CancelToken
        {
            get; set;
        }

        public TaskScheduler Scheduler
        {
            get; set;
        }

        public void InitTaskSyncContext(ITaskSyncContext<IJobProgress> mainContext)
        {
            this.CloneSyncContext(mainContext);
        }

        public OrganizationAdminInfo AdminInfo { get; set; }
        public string Organization { get; private set; }
        public ICatalogAccess<IJobProgress> CatalogAccess { get; set; }
        public IEwsServiceAdapter<IJobProgress> EwsServiceAdapter { get; set; }
        public IDataFromBackup<IJobProgress> DataFromClient { get; set; }
        public DateTime JobStartTime { get; private set; }

        public IDataConvert DataConvert
        {
            get; set;
        }

        public void Dispose()
        {
        }

        public async Task<ICollection<IMailboxDataSync>> GetAllMailboxFromPlan(Func<IEnumerable<string>, Task<ICollection<IMailboxDataSync>>> funcGetAllMailboxFromExchange)
        {
            return await DataFromClient.GetAllMailboxesAsync(funcGetAllMailboxFromExchange);
        }

        public async Task<ICollection<IMailboxDataSync>> GetAllMailboxFromExchange(IEnumerable<string> mailboxAddresses)
        {
            return await EwsServiceAdapter.GetAllMailboxesAsync(AdminInfo.UserName, AdminInfo.UserPassword, mailboxAddresses);
        }

        public async Task<ICatalogJob> GetLatestCatalogJob()
        {
            return await DataFromClient.GetLatestCatalogJobAsync();
        }

        public async Task<IEnumerable<IMailboxDataSync>> GetAllMailboxFromLastCatalog(ICatalogJob catalogJob)
        {
            return await CatalogAccess.GetMailboxFromLatestCatalogAsync(catalogJob);
        }

        public ICollection<IMailboxDataSync> GetIntersectionMailboxCollection(ICollection<IMailboxDataSync> mailboxInExchange, ICollection<IMailboxDataSync> mailboxInPlan)
        {
            if (CloudConfig.Instance.IsTestForDemo)
            {
                var result = new List<IMailboxDataSync>(mailboxInExchange.Count);

                var dicExchange = new Dictionary<string, IMailboxDataSync>();
                foreach (var item in mailboxInExchange)
                {
                    dicExchange.Add(item.MailAddress, item);
                }

                var dicPlan = new Dictionary<string, IMailboxDataSync>();
                foreach (var item in mailboxInPlan)
                {
                    dicPlan.Add(item.MailAddress, item);
                }

                foreach (var key in dicExchange.Keys)
                {
                    if (dicPlan.ContainsKey(key))
                    {
                        result.Add(dicExchange[key]);
                    }
                }

                var temp = new List<IMailboxDataSync>(result.Count);
                foreach (var item in result)
                {
                    temp.Add(DataConvert.Convert(item));
                }

                return temp;

            }

            {
                var result = new List<IMailboxDataSync>(mailboxInExchange.Count);

                var dicExchange = new Dictionary<string, IMailboxDataSync>();
                foreach (var item in mailboxInExchange)
                {
                    dicExchange.Add(item.Id, item);
                }

                var dicPlan = new Dictionary<string, IMailboxDataSync>();
                foreach (var item in mailboxInPlan)
                {
                    dicPlan.Add(item.Id, item);
                }

                foreach (var key in dicExchange.Keys)
                {
                    if (dicPlan.ContainsKey(key))
                    {
                        result.Add(dicExchange[key]);
                    }
                }

                var temp = new List<IMailboxDataSync>(result.Count);
                foreach (var item in result)
                {
                    temp.Add(DataConvert.Convert(item));
                }

                return temp;
            }
        }

        public IDictionary<ItemUADStatus, ICollection<IMailboxDataSync>> GetMailboxUAD(ICollection<IMailboxDataSync> validMailboxes, IEnumerable<IMailboxDataSync> mailboxInLastCatalog)
        {

            Dictionary<ItemUADStatus, ICollection<IMailboxDataSync>> result = new Dictionary<ItemUADStatus, ICollection<IMailboxDataSync>>()
                    {
                        {ItemUADStatus.Add, new List<IMailboxDataSync>() },
                        {ItemUADStatus.Delete, new List<IMailboxDataSync>() },
                        {ItemUADStatus.Update, new List<IMailboxDataSync>() },
                        {ItemUADStatus.None, new List<IMailboxDataSync>() }
                    };
            Dictionary<string, IMailboxDataSync> mailboxSyncDic = new Dictionary<string, IMailboxDataSync>();
            foreach (var mailbox in mailboxInLastCatalog)
            {
                mailboxSyncDic.Add(mailbox.Id, mailbox);
            }

            IMailboxDataSync temp = null;
            foreach (var mailbox in validMailboxes)
            {
                if (mailboxSyncDic.TryGetValue(mailbox.Id, out temp))
                {
                    mailbox.SyncStatus = temp.SyncStatus;

                    if (mailbox.IsDataEqual(temp))
                    {
                        result[ItemUADStatus.None].Add(mailbox);
                    }
                    else
                    {
                        result[ItemUADStatus.Update].Add(mailbox);
                    }

                    mailboxSyncDic.Remove(mailbox.Id);
                }
                else
                {
                    result[ItemUADStatus.Add].Add(mailbox);
                }
            }

            foreach (var mailbox in mailboxInLastCatalog)
            {
                if (mailboxSyncDic.ContainsKey(mailbox.Id))
                {
                    result[ItemUADStatus.Delete].Add(mailbox);
                }
            }
            return result;

        }

        public async Task AddMailboxToCurrentCatalog(ICollection<IMailboxDataSync> mailboxes)
        {
            await CatalogAccess.AddMailboxesToCatalogAsync(mailboxes);
        }

        public async Task UpdateMailboxToCurrentCatalog(ICollection<IMailboxDataSync> mailboxes)
        {
            await CatalogAccess.UpdateMailboxToCatalogAsync(mailboxes);
        }

        public async Task DeleteMailboxToCurrentCatalog(ICollection<IMailboxDataSync> mailboxes)
        {
            await CatalogAccess.DeleteMailboxToCatalogAsync(mailboxes);
        }

        public async Task DoEachMailbox(IMailboxDataSync mailbox, ItemUADStatus uadStatus)
        {
            var mailboxFlowTemplate = NewMailboxTemplate();
            mailboxFlowTemplate.MailboxInfo = mailbox;

            await mailboxFlowTemplate.BackupAsync(uadStatus);
        }

        public async Task ForEachLoop(ICollection<IMailboxDataSync> mailboxes, ItemUADStatus uadStatus, Func<IMailboxDataSync, ItemUADStatus, Task> FuncDoEachMailbox)
        {
            await Task.Run(() =>
            {
                Parallel.ForEach(mailboxes,
                    new ParallelOptions()
                    {
                        CancellationToken = CancelToken,
                        TaskScheduler = Scheduler,
                        MaxDegreeOfParallelism = CloudConfig.Instance.MaxDegreeOfParallelismForMailbox
                    },
                    (mailbox) =>
                    {
                        FuncDoEachMailbox(mailbox, uadStatus).Wait();
                    });
            });
        }

        public BackupMailboxAsyncFlowTemplate NewMailboxTemplate()
        {
            var result = new AsyncBackupMailbox();
            result.CloneSyncContext(this);
            result.CloneExchangeAccess(this);
            result.Organization = Organization;
            result.AdminInfo = AdminInfo;
            return result;
        }


    }
}
