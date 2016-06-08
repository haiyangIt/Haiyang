using Arcserve.Office365.Exchange.Tool.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Arcserve.Office365.Exchange.DataProtect.Tool.Result;
using Arcserve.Office365.Exchange.DataProtect.Tool.Resource;
using System.Management.Automation.Remoting;

namespace Arcserve.Office365.Exchange.Tool
{
    public class GetAllMailboxCommand : ArcServeCommand
    {
        public const string CommandName = "GetAllMailbox";
        [CommandArgument("AdminUserName", "Please specify the administartor. like: -AdminUserName:user.", true)]
        public ArgInfo AdminUserName { get; set; }
        [CommandArgument("AdminPassword", "Please specify the password. like: -AdminPassword:userpassword.", true)]
        public ArgInfo AdminPassword { get; set; }

        public GetAllMailboxCommand(CommandArgs args) : base(args)
        {

        }

        protected override ResultBase DoExcute()
        {
            var result = GetAllMailbox(AdminUserName.Value, AdminPassword.Value);

            return new GetAllMailboxResult(result);
        }

        private static MailboxList GetAllMailbox(string adminName, string adminPassword)
        {
            const string liveIDConnectionUri = "https://outlook.office365.com/PowerShell-LiveID";
            const string schemaUri = "http://schemas.microsoft.com/powershell/Microsoft.Exchange";
            PSCredential credentials = new PSCredential(adminName, StringToSecureString(adminPassword));

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

                    var props = new string[] { "Name", "DisplayName", "UserPrincipalName", "Guid" };
                    Command CommandSelect = new Command("Select-Object");
                    CommandSelect.Parameters.Add("Property", props);
                    pipe.Commands.Add(CommandSelect);


                    runspace.Open();

                    var information = pipe.Invoke();
                    MailboxList result = new MailboxList(information.Count);
                    string displayName = string.Empty;
                    string address = string.Empty;
                    string name = string.Empty;
                    string guid = string.Empty;
                    foreach (PSObject eachUserMailBox in information)
                    {
                        displayName = string.Empty;
                        address = string.Empty;
                        name = string.Empty;
                        guid = string.Empty;

                        foreach (PSPropertyInfo propertyInfo in eachUserMailBox.Properties)
                        {
                            if (propertyInfo.Name == "DisplayName")
                                displayName = propertyInfo.Value.ToString();
                            if (propertyInfo.Name == "UserPrincipalName")
                                address = propertyInfo.Value.ToString().ToLower();
                            if (propertyInfo.Name == "Guid")
                                guid = propertyInfo.Value.ToString();
                            if (propertyInfo.Name == "Name")
                                name = propertyInfo.Value.ToString();

                        }

                        result.Add(new Mailbox(displayName, address) { Name = name, Id = guid });
                    }
                    return result;
                }
            }
        }

        private static SecureString StringToSecureString(string str)
        {
            SecureString ss = new SecureString();
            char[] passwordChars = str.ToCharArray();

            foreach (char c in passwordChars)
            {
                ss.AppendChar(c);
            }
            return ss;
        }

        protected override ResultBase GetErrorResultBase(Exception e)
        {
            if (e is PSRemotingTransportException)
            {
                if (e.HResult == -2146233087)
                {
                    if (((PSRemotingTransportException)e).ErrorCode == -2144108103)
                    {
                        return new GetAllMailboxResult(ResMessage.NetworkInvalid);
                    }
                    else if (((PSRemotingTransportException)e).ErrorCode == -2144108102)
                    {
                        return new GetAllMailboxResult(ResMessage.NetworkUnStable);
                    }
                }
            }

            return new GetAllMailboxResult(e.Message);
        }

        protected override ResultBase GetInvalidUserPsw()
        {
            return new GetAllMailboxResult(ResMessage.UserNamePswInvalid);
        }
    }

    [Serializable]
    public class Mailbox
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string MailAddress { get; set; }
        public string Id { get; set; }

        public Mailbox() { }

        public Mailbox(string displayName, string mailAddress)
        {
            DisplayName = displayName;
            MailAddress = mailAddress;
        }
    }

    [Serializable]
    public class MailboxList : List<Mailbox>
    {
        public MailboxList(int capacity) : base(capacity) { }

        public MailboxList() : base() { }
    }
}
