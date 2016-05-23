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


        public override Func<ICatalogJob, IEnumerable<IMailboxDataSync>> FuncGetAllMailboxFromLastCatalog
        {
            get
            {
                return (latestCatalogJob) =>
                {
                    return CatalogAccess.GetMailboxesFromLatestCatalog(latestCatalogJob);
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

        public override Func<ICollection<IMailboxDataSync>, ICollection<IMailboxDataSync>, ICollection<IMailboxDataSync>> FuncGetIntersectionMailboxCollection
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

                    var temp = new List<IMailboxDataSync>(result.Count);
                    foreach(var item in result)
                    {
                        temp.Add(DataConvert.Convert(item));
                    }

                    return temp;
                };
            }
        }


        public override Action<ICollection<IMailboxDataSync>> AddMailboxToCurrentCatalog
        {
            get
            {
                return (mailboxes) =>
                {
                    CatalogAccess.AddMailboxesToCatalog(mailboxes);
                };
            }
        }


        public override Func<BackupMailboxFlowTemplate> FuncNewMailboxTemplate
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

        public override void ForEachLoop(ICollection<IMailboxDataSync> items, Action<IMailboxDataSync> DoEachMailbox)
        {
            foreach (var mailbox in items)
            {
                DoEachMailbox(mailbox);
            }
        }
        
    }
}
