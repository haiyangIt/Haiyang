using DataProtectInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Security;
using DataProtectImpl.Backup;
using Arcserve.Office365.Exchange.Thread;
using Arcserve.Office365.Exchange.Data.Account;
using Arcserve.Office365.Exchange.Data.Increment;

namespace Arcserve.Office365.Exchange.DataProtect.Interface.Backup.Increment
{
    public class SyncBackup : BackupFlowTemplate, ITaskSyncContext<JobProgress>
    {
        public SyncBackup()
        {

        }

        public JobProgress Progress
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
        public IBackupQuerySync<JobProgress> BackupQuery { get; }
        public IEwsServiceAdapter<JobProgress> EwsServiceAdapter { get; set; }
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
            const string liveIDConnectionUri = "https://outlook.office365.com/PowerShell-LiveID";
            const string schemaUri = "http://schemas.microsoft.com/powershell/Microsoft.Exchange";
            PSCredential credentials = new PSCredential(AdminInfo.UserName, StringToSecureString(AdminInfo.UserPassword));

            WSManConnectionInfo connectionInfo = new WSManConnectionInfo(
        new Uri(liveIDConnectionUri),
        schemaUri, credentials);
            connectionInfo.AuthenticationMechanism = AuthenticationMechanism.Basic;

            using (Runspace runspace = RunspaceFactory.CreateRunspace(connectionInfo))
            {
                using (Pipeline pipe = runspace.CreatePipeline())
                {

                    Command CommandGetMailbox = new Command("Get-Mailbox");
                    CommandGetMailbox.Parameters.Add("RecipientTypeDetails", "UserMailbox");
                    pipe.Commands.Add(CommandGetMailbox);

                    var props = new string[] { "Name", "DisplayName", "UserPrincipalName" };
                    Command CommandSelect = new Command("Select-Object");
                    CommandSelect.Parameters.Add("Property", props);
                    pipe.Commands.Add(CommandSelect);

                    runspace.Open();

                    var information = pipe.Invoke();
                    List<IMailboxDataSync> result = new List<IMailboxDataSync>(information.Count);
                    string displayName = string.Empty;
                    string address = string.Empty;
                    foreach (PSObject eachUserMailBox in information)
                    {
                        displayName = string.Empty;
                        address = string.Empty;
                        foreach (PSPropertyInfo propertyInfo in eachUserMailBox.Properties)
                        {
                            if (propertyInfo.Name == "DisplayName")
                                displayName = propertyInfo.Value.ToString();
                            if (propertyInfo.Name == "UserPrincipalName")
                                address = propertyInfo.Value.ToString().ToLower();
                        }

                        //if (IsNeedGenerateMailbox(address) && address.ToLower() == "haiyang.ling@arcserve.com") // todo remove the specific mail address.
                        result.Add(new MailboxInfo(displayName, address));
                    }
                    return result;
                }
            }
        }

        private SecureString StringToSecureString(string userPassword)
        {
            SecureString ss = new SecureString();
            char[] passwordChars = userPassword.ToCharArray();

            foreach (char c in passwordChars)
            {
                ss.AppendChar(c);
            }
            return ss;
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

        public void InitTaskSyncContext(ITaskSyncContext<JobProgress> mainContext)
        {
            throw new NotImplementedException();

        }


        class MailboxInfo : IMailboxDataSync
        {
            public MailboxInfo(string displayName, string mailboxAddress)
            {
                DisplayName = displayName;
                MailAddress = mailboxAddress;
            }

            public string ChangeKey
            {
                get
                {
                    return string.Empty;
                }
            }

            public int ChildFolderCount
            {
                get; set;
            }

            public string DisplayName
            {
                get; set;
            }

            public string Id
            {
                get
                {
                    return RootFolderId;
                }

                set
                {
                    RootFolderId = value;
                }
            }

            public ItemKind ItemKind
            {
                get
                {
                    return ItemKind.Mailbox;
                }

                set
                {
                }
            }

            public string Location
            {
                get; set;
            }

            public string MailAddress
            {
                get; private set;
            }

            public string RootFolderId
            {
                get; set;
            }

            public string SyncStatus
            {
                get
                {
                    return string.Empty;
                }
            }

            public IMailboxData Clone()
            {
                return new MailboxInfo(DisplayName, MailAddress)
                {
                    Location = Location,
                    RootFolderId = RootFolderId
                };
            }
        }
    }
}
