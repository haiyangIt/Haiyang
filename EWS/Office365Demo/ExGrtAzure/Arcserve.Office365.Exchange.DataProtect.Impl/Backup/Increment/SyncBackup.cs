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
using Arcserve.Office365.Exchange.DataProtect.Interface.Backup.Increment;
using Arcserve.Office365.Exchange.Util;
using Arcserve.Office365.Exchange.Util.Setting;

namespace Arcserve.Office365.Exchange.DataProtect.Impl.Backup.Increment
{
    public class SyncBackup : BackupFlowTemplate, ITaskSyncContext<IJobProgress>, IExchangeAccess<IJobProgress>
    {
        public SyncBackup()
        {

        }

        public OrganizationAdminInfo AdminInfo { get; set; }
        public string Organization { get; }
        public ICatalogAccess<IJobProgress> CatalogAccess { get; set; }
        public IEwsServiceAdapter<IJobProgress> EwsServiceAdapter { get; set; }
        public IDataFromClient<IJobProgress> DataFromClient { get; set; }
        public DateTime JobStartTime { get; }

        protected override Func<ICollection<IMailboxDataSync>> FuncGetAllMailboxFromExchange
        {
            get
            {
                return () =>
                {
                    return EwsServiceAdapter.GetAllMailboxes(AdminInfo.UserName, AdminInfo.UserPassword);
                };
            }
        }

        protected override Func<ICatalogJob> FuncGetLatestCatalogJob
        {
            get
            {
                return () =>
                {
                    return DataFromClient.GetLatestCatalogJob();
                };
            }
        }


        protected override Func<ICatalogJob, IEnumerable<IMailboxDataSync>> FuncGetAllMailboxFromLastCatalog
        {
            get
            {
                return (latestCatalogJob) =>
                {
                    return CatalogAccess.GetMailboxesFromLatestCatalog(latestCatalogJob);
                };
            }
        }

        protected override Func<ICollection<IMailboxDataSync>> FuncGetAllMailboxFromPlan
        {
            get
            {
                return () =>
                {
                    return DataFromClient.GetAllMailboxes();
                };
            }
        }

        protected override Func<ICollection<IMailboxDataSync>, ICollection<IMailboxDataSync>, ICollection<IMailboxDataSync>> FuncGetIntersectionMailboxCollection
        {
            get
            {
                if (CloudConfig.Instance.IsTestForDemo)
                {
                    return (mailboxInExchange, mailboxInPlan) =>
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
                    };
                }

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
                            result.Add(dicExchange[key]);
                        }
                    }

                    var temp = new List<IMailboxDataSync>(result.Count);
                    foreach(var item in result)
                    {
                        temp.Add(DataConvert.Convert(item));
                    }

                    return temp;
                };
            }
        }

        protected override Action<ICollection<IMailboxDataSync>, IEnumerable<IMailboxDataSync>> ActionSetMailboxSyncStatus
        {
            get
            {
                return (validMailboxes, mailboxInLastCatalog) =>
                {
                    Dictionary<string, string> mailboxSyncDic = new Dictionary<string, string>();
                    foreach (var mailbox in mailboxInLastCatalog)
                    {
                        mailboxSyncDic.Add(mailbox.Id, mailbox.SyncStatus);
                    }

                    string syncStatus = string.Empty;
                    foreach (var mailbox in validMailboxes)
                    {
                        if (mailboxSyncDic.TryGetValue(mailbox.Id, out syncStatus))
                        {
                            mailbox.SyncStatus = syncStatus;
                        }
                    }
                };
            }
        }

        protected override Action<ICollection<IMailboxDataSync>> AddMailboxToCurrentCatalog
        {
            get
            {
                return (mailboxes) =>
                {
                    CatalogAccess.AddMailboxesToCatalog(mailboxes);
                };
            }
        }


        protected override Func<BackupMailboxFlowTemplate> FuncNewMailboxTemplate
        {
            get
            {
                return () =>
                {
                    var result = new SyncBackupMailbox();
                    result.CloneSyncContext(this);
                    result.CloneExchangeAccess(this);
                    result.Organization = Organization;
                    result.AdminInfo = AdminInfo;
                    return result;
                };
            }
        }

        public IDataConvert DataConvert
        {
            get; set;
        }

        protected override void ForEachLoop(ICollection<IMailboxDataSync> items, Action<IMailboxDataSync> DoEachMailbox)
        {
            foreach (var mailbox in items)
            {
                DoEachMailbox(mailbox);
            }
        }

        public override void Dispose()
        {
        }
    }
}
