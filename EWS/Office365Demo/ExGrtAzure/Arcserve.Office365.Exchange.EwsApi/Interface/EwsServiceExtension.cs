using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.EwsApi.Interface
{
    public static class EwsServiceExtension
    {
        public static string GetDetailInformation(this ServiceResponse response)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Response details: errorMsg:").Append(response.ErrorMessage).Append(" errorCode:").Append(response.ErrorCode).Append(" result:").Append(response.Result.ToString()).Append(" errorDetais:");
            if(response.ErrorDetails != null && response.ErrorDetails.Count > 0)
            {
                int index = 0;
                foreach(var item in response.ErrorDetails)
                {
                    sb.Append(" [").Append(index++).Append("]: key:").Append(item.Key).Append(" value:").Append(item.Value);
                }
            }

            sb.Append(" errorProperties:");
            if(response.ErrorProperties != null && response.ErrorProperties.Count > 0)
            {
                int index = 0;
                foreach(var item in response.ErrorProperties)
                {
                    sb.Append(" [").Append(index++).Append("]:").Append(item.Type.FullName).Append(" strProperty:").Append(item.ToString());
                }
            }
            return sb.ToString();
        }

        public static List<Data.Mail.Mailbox> GetAllMailbox(string adminName, string adminPassword, IEnumerable<string> selectedMailbox)
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
                    if (selectedMailbox != null && selectedMailbox.Count() > 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append("{");
                        const string Or = " -or ";
                        foreach (var m in selectedMailbox)
                        {
                            sb.Append(" (PrimarySmtpAddress -eq ").Append("\"").Append(m).Append("\")").Append(Or);
                        }
                        sb.Length -= Or.Length;
                        sb.Append(" }");
                        CommandGetMailbox.Parameters.Add("Filter", sb.ToString());
                    }
                    pipe.Commands.Add(CommandGetMailbox);

                    var props = new string[] { "Name", "DisplayName", "UserPrincipalName", "Guid" };
                    Command CommandSelect = new Command("Select-Object");
                    CommandSelect.Parameters.Add("Property", props);
                    pipe.Commands.Add(CommandSelect);


                    runspace.Open();

                    var information = pipe.Invoke();
                    List<Arcserve.Office365.Exchange.Data.Mail.Mailbox> result = new List<Arcserve.Office365.Exchange.Data.Mail.Mailbox>(information.Count);
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

                        result.Add(new Data.Mail.Mailbox(displayName, address) { Name = name, Id = guid });
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
