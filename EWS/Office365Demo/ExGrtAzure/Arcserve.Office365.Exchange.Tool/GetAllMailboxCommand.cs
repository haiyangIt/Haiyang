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

namespace Arcserve.Office365.Exchange.Tool
{
    public class GetAllMailboxCommand : ArcServeCommand
    {
        public const string CommandName = "GetAllMailbox";
        [CommandArgument("AdminUserName", "Please specify the job type. like: -AdminUserName:user.", true)]
        public ArgInfo AdminUserName { get; set; }
        [CommandArgument("AdminPassword", "Please specify the job type. like: -AdminPassword:user.", true)]
        public ArgInfo AdminPassword { get; set; }
        
        public GetAllMailboxCommand(CommandArgs args) : base(args)
        {

        }

        protected override string DoExcute()
        {
            var result = GetAllMailbox(AdminUserName.Value, AdminPassword.Value);
            XmlSerializer s = new XmlSerializer(typeof(List<Mailbox>));
            StringBuilder sb = new StringBuilder();
            using (StringWriter writer = new StringWriter(sb))
            {
                s.Serialize(writer, result);
            }
            return sb.ToString();
        }

        private static ICollection<Mailbox> GetAllMailbox(string adminName, string adminPassword)
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
                    List<Mailbox> result = new List<Mailbox>(information.Count);
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
    }
}
