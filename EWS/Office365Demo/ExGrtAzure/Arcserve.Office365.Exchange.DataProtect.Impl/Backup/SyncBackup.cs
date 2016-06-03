using System;
using System.Collections.Generic;
using Arcserve.Office365.Exchange.Thread;
using Arcserve.Office365.Exchange.Data.Account;
using Arcserve.Office365.Exchange.Data.Increment;
using Arcserve.Office365.Exchange.Data;
using Arcserve.Office365.Exchange.DataProtect.Interface.Backup;
using Arcserve.Office365.Exchange.Util.Setting;
using Arcserve.Office365.Exchange.EwsApi.Interface;

namespace Arcserve.Office365.Exchange.DataProtect.Impl.Backup
{
    internal class SyncBackup : BackupFlowTemplate, ITaskSyncContext<IJobProgress>, IExchangeAccess<IJobProgress>
    {
        public SyncBackup()
        {

        }

        public OrganizationAdminInfo AdminInfo { get; set; }
        public string Organization { get; private set; }
        public ICatalogAccess<IJobProgress> CatalogAccess { get; set; }
        public IEwsServiceAdapter<IJobProgress> EwsServiceAdapter { get; set; }
        public IDataFromClient<IJobProgress> DataFromClient { get; set; }
        public DateTime JobStartTime { get; private set; }

        protected override Func<IEnumerable<string>, ICollection<IMailboxDataSync>> FuncGetAllMailboxFromExchange
        {
            get
            {
                return (mailboxes) =>
                {
                    return EwsServiceAdapter.GetAllMailboxes(AdminInfo.UserName, AdminInfo.UserPassword, mailboxes);
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
                    foreach (var item in result)
                    {
                        temp.Add(DataConvert.Convert(item));
                    }

                    return temp;
                };
            }
        }

        protected override Func<ICollection<IMailboxDataSync>, IEnumerable<IMailboxDataSync>, IDictionary<ItemUADStatus, ICollection<IMailboxDataSync>>> FuncGetMailboxUAD
        {
            get
            {

                return (validMailboxes, mailboxInLastCatalog) =>
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
                        result[ItemUADStatus.Delete].Add(mailbox);
                    }
                    return result;
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

        protected override Action<ICollection<IMailboxDataSync>> UpdateMailboxToCurrentCatalog
        {
            get
            {
                return (mailboxes) =>
                {
                    CatalogAccess.UpdateMailboxToCatalog(mailboxes);
                };
            }
        }
        protected override Action<ICollection<IMailboxDataSync>> DeleteMailboxToCurrentCatalog
        {
            get
            {
                return (mailboxes) =>
                {
                    CatalogAccess.DeleteMailboxToCatalog(mailboxes);
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

        protected override void ForEachLoop(ICollection<IMailboxDataSync> items, ItemUADStatus uadStatus, Action<IMailboxDataSync, ItemUADStatus> DoEachMailbox)
        {
            foreach (var mailbox in items)
            {
                DoEachMailbox(mailbox, uadStatus);
            }
        }

        public override void Dispose()
        {
        }
    }
}
