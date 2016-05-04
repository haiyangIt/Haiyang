using Arcserve.Office365.Exchange.EwsApi.Increment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arcserve.Office365.Exchange.Data.Account;
using Arcserve.Office365.Exchange.Data.Increment;
using Arcserve.Office365.Exchange.Thread;
using Microsoft.Exchange.WebServices.Data;
using System.Threading;
using System.Management.Automation;
using System.Security;
using System.Management.Automation.Runspaces;
using Arcserve.Office365.Exchange.Data;
using Arcserve.Office365.Exchange.Data.Mail;

namespace Arcserve.Office365.Exchange.EwsApi.Impl.Increment
{
    public abstract class EwsServiceAdapterBase : TaskSyncContextBase, IEwsServiceAdapter<IJobProgress>
    {


        protected void CheckIsCanceled()
        {
            if (CancelToken.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }
        }
        

        public ICollection<IMailboxDataSync> GetAllMailboxes(string adminUserName, string adminPassword)
        {
            CheckIsCanceled();
            WaitForPerformanceLimit();
            Progress.Report("Getting all mailboxes in exchange.");
            var result = RetryFunc(new Topaz.RetryContext("Getting all mailbox", Topaz.OperationType.Others),
                () =>
                {
                    return DoGetAllMailboxes(adminUserName, adminPassword);
                });
            Progress.Report("Getting all mailboxes in exchange completed.total {0} mailboxes.", result.Count);
            return result;
        }

        public Task<ICollection<IMailboxDataSync>> GetAllMailboxesAsync(string adminUserName, string adminPassword)
        {
            throw new NotImplementedException();
        }

        protected virtual ICollection<IMailboxDataSync> DoGetAllMailboxes(string adminUserName, string adminPassword)
        {
            const string liveIDConnectionUri = "https://outlook.office365.com/PowerShell-LiveID";
            const string schemaUri = "http://schemas.microsoft.com/powershell/Microsoft.Exchange";
            PSCredential credentials = new PSCredential(adminUserName, StringToSecureString(adminPassword));

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

        public ExchangeService GetExchangeService(string mailbox, OrganizationAdminInfo adminInfo)
        {
            throw new NotImplementedException();
        }

        public Task<ExchangeService> GetExchangeServiceAsync(string mailbox, OrganizationAdminInfo adminInfo)
        {
            throw new NotImplementedException();
        }

        public void InitTaskSyncContext(ITaskSyncContext<IJobProgress> mainContext)
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

            public IMailboxDataSync Clone()
            {
                return new MailboxInfo(DisplayName, MailAddress)
                {
                    Location = Location,
                    RootFolderId = RootFolderId
                };
            }

            IMailboxData IMailboxData.Clone()
            {
                return Clone();
            }
        }
    }


}
