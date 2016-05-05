using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Security;
using Arcserve.Office365.Exchange.Thread;
using Arcserve.Office365.Exchange.Data.Account;
using Arcserve.Office365.Exchange.Data.Increment;
using Arcserve.Office365.Exchange.EwsApi.Increment;
using Arcserve.Office365.Exchange.Data;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Arcserve.Office365.Exchange.Data.Mail;

namespace Arcserve.Office365.Exchange.DataProtect.Interface.Backup.Increment
{
    public class SyncBackup : BackupFlowTemplate, ITaskSyncContext<IJobProgress>
    {
        public SyncBackup()
        {

        }

        public IJobProgress Progress
        {
            get; set;
        }

        public TaskScheduler Scheduler
        {
            get; set;
        }

        public CancellationToken CancelToken
        {
            get; set;
        }

        public OrganizationAdminInfo AdminInfo { get; set; }
        public string Organization { get; }
        public IBackupQueryAsync<IJobProgress> BackupQuery { get; }
        public IEwsServiceAdapter<IJobProgress> EwsServiceAdapter { get; set; }
        public IDataFromClient<IJobProgress> DataFromClient { get; set; }
        public DateTime JobStartTime { get; }

        public override Func<ICollection<IMailboxDataSync>> FuncGetAllMailboxFromExchange
        {
            get
            {
                return () =>
                {
                    return EwsServiceAdapter.GetAllMailboxes(AdminInfo.UserName, AdminInfo.UserPassword);
                };
            }
        }

        public override Func<ICatalogJob> FuncGetLatestCatalogJob
        {
            get
            {
                return () =>
                {
                    return DataFromClient.GetLatestCatalogJob();
                };
            }
        }


        public override Func<ICatalogJob, ICollection<IMailboxDataSync>> FuncGetAllMailboxFromLastCatalog
        {
            get
            {
                return (latestCatalogJob) =>
                {
                    return BackupQuery.GetMailboxes(latestCatalogJob);
                };
            }
        }

        public override Func<ICollection<IMailboxDataSync>> FuncGetAllMailboxFromPlan
        {
            get
            {
                return () =>
                {
                    return DataFromClient.GetAllMailboxes();
                };
            }
        }

        public override Func<ICollection<IMailboxDataSync>, ICollection<IMailboxDataSync>, ICollection<IMailboxDataSync>> FuncGetValidMailbox
        {
            get
            {
                return (mailboxInExchange, mailboxInPlan) =>
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
                            result.Add(dicPlan[key]);
                        }
                    }

                    return result;
                };
            }
        }

        public override Func<ICollection<IMailboxDataSync>, ICollection<IMailboxDataSync>, ICollection<IMailboxDataSync>[]> FuncGetMailboxCatalog
        {
            get
            {
                return (mailboxValid, mailboxInLastCatalog) =>
                {
                    var result = new ICollection<IMailboxDataSync>[2];

                    var dicValid = new Dictionary<string, IMailboxDataSync>();
                    foreach (var item in mailboxValid)
                    {
                        dicValid.Add(item.Id, item);
                    }

                    var dicCatalog = new Dictionary<string, IMailboxDataSync>();
                    foreach (var item in mailboxInLastCatalog)
                    {
                        dicCatalog.Add(item.Id, item);
                    }

                    HashSet<string> ids = new HashSet<string>();
                    
                    foreach (var key in dicValid.Keys)
                    {
                        if (dicCatalog.ContainsKey(key))
                        {
                            ids.Add(key);
                        }
                    }

                    result[0] = (from keyValue in dicValid where !ids.Contains(keyValue.Key) select keyValue.Value).ToList();
                    result[1] = (from keyValue in dicCatalog where !ids.Contains(keyValue.Key) select keyValue.Value).ToList();

                    return result;
                };
            }
        }

        public override Action<ICollection<IMailboxDataSync>> AddMailboxToCurrentCatalog
        {
            get
            {
                return (mailboxes) =>
                {
                    BackupQuery.AddMailboxes(mailboxes);
                };
            }
        }


        public override Action<ICollection<IMailboxDataSync>> RemoveMailboxToCurrentCatalog
        {
            get
            {
                return (mailboxes) =>
                {
                    BackupQuery.DeleteMailboxes(mailboxes);
                };
            }
        }


        public override BackupMailboxFlowTemplate MailboxTemplate
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override void ForEachLoop(ICollection<IMailboxDataSync> items, Action<IMailboxDataSync> DoEachMailbox)
        {
            foreach(var mailbox in items)
            {
                DoEachMailbox(mailbox);
            }
        }

        public void InitTaskSyncContext(ITaskSyncContext<IJobProgress> mainContext)
        {
            this.CloneSyncContext(mainContext);
        }
    }
}
