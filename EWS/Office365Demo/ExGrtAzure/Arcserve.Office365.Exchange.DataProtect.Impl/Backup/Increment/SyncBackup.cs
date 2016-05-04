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
        public DateTime JobStartTime { get; }

        public override Action<IList<IMailboxDataSync>> AddMailboxToCurrentCatalog
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        
        public override Func<IList<IMailboxDataSync>> FuncGetAllMailboxFromExchange
        {
            get
            {
                return GetAllMailboxesFromExchange;
            }
        }

        private IList<IMailboxDataSync> GetAllMailboxesFromExchange()
        {
            
        }

        

        public override Func<IList<IMailboxDataSync>> FuncGetAllMailboxFromLastCatalog
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override Func<IList<IMailboxDataSync>> FuncGetAllMailboxFromPlan
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override Func<IList<IMailboxDataSync>, IList<IMailboxDataSync>, IList<IMailboxDataSync>> FuncGetMailboxCatalog
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override Func<IList<IMailboxDataSync>, IList<IMailboxDataSync>, IList<IMailboxDataSync>> FuncGetValidMailbox
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override BackupMailboxFlowTemplate MailboxTemplate
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override void ForEachLoop(IList<IMailboxDataSync> items, Action<IMailboxDataSync> DoEachMailbox)
        {
            throw new NotImplementedException();
        }

        public void InitTaskSyncContext(ITaskSyncContext<IJobProgress> mainContext)
        {
            throw new NotImplementedException();

        }


       
    }
}
