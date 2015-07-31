using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security;
using System.Text;

namespace SendMail
{
    public class EmsSession
    {
        protected readonly PowerShell powershell;

        public EmsSession()
        {
            powershell = PowerShell.Create();
            PSSnapInException ex;
            powershell
              .Runspace
              .RunspaceConfiguration
              .AddPSSnapIn(
                 "Microsoft.Exchange.Management.PowerShell.E2013",
                 out ex);
            if (ex != null)
                throw ex;
            powershell.AddScript(
              ". $env:ExchangeInstallPath\\bin\\RemoteExchange.ps1");
            powershell.AddScript("Connect-ExchangeServer -auto");
            powershell.Invoke();
            powershell.Streams.ClearStreams();
            powershell.Commands.Clear();
        }

        public void RunScript(string scriptText,
            Action<Collection<PSObject>> handler)
        {
            powershell.Streams.ClearStreams();
            powershell.Commands.Clear();
            powershell.AddScript(scriptText, true);
            var results = powershell.Invoke();
            ThrowIfError();
            if (handler != null)
            {
                handler(results);
            }
        }

        public string RunScript(string scriptText)
        {
            var result = new StringBuilder();
            RunScript(scriptText, results =>
            {
                foreach (var line in results)
                {
                    if (line != null)
                    {
                        result.AppendLine(line.ToString());
                    }
                }
            });
            return result.ToString().TrimEnd();
        }

        public void RunCommand(string command, Dictionary<string,string> parameters)
        {

        }

        private void ThrowIfError()
        {
            var errors = powershell.Streams.Error;
            if (errors.Count > 0)
            {
                var e = errors[0].Exception;
                powershell.Streams.ClearStreams();
                throw e;
            }
        }

        private static Runspace _runSpace;

        private static Pipeline CreatePipeline()
        {
            if (_runSpace == null)
            {
                RunspaceConfiguration runspaceConf = RunspaceConfiguration.Create();

                PSSnapInException PSException = null;

                PSSnapInInfo info = runspaceConf.AddPSSnapIn("Microsoft.Exchange.Management.PowerShell.E2010", out PSException);

                _runSpace = RunspaceFactory.CreateRunspace(runspaceConf);

                _runSpace.Open();
            }

            return _runSpace.CreatePipeline();
        }

        public static void Release()
        {
            if(_runSpace != null)
            {
                _runSpace.Close();
                _runSpace.Dispose();
                _runSpace = null;
            }
        }


        public static string CreateMails(ICollection<string> userNames, string domainName,string dbName)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                foreach (string name in userNames)
                {
                    CreateMail(name, domainName, dbName);
                    sb.Append("Create ").Append(name).Append("@").Append(domainName).Append(" success").AppendLine();

                }
                return sb.ToString();
            }
            catch(Exception ex)
            {
                sb.AppendLine(ex.Message);
                return sb.ToString();
            }
        }

        public static string CreateMails(ICollection<string> userNames, string domainName,string dbName,out bool isSuccess)
        {
            StringBuilder sb = new StringBuilder();
            isSuccess = false;
            try
            {
                foreach (string name in userNames)
                {
                    CreateMail(name, domainName, dbName);
                    sb.Append("Create ").Append(name).Append("@").Append(domainName).Append(" success").AppendLine();

                }
                isSuccess = true;
                return sb.ToString();
            }
            catch (Exception ex)
            {
                sb.AppendLine(ex.Message);
                return sb.ToString();
            }
        }

        public static void CreateMail(string userName, string domainName, string dbName)
        {
            using (Pipeline p = CreatePipeline())
            {
                const string psw = "123.com";

                SecureString securePwd = StringToSecureString(psw);
                Command newMailBox = new Command("New-Mailbox");

                newMailBox.Parameters.Add("Name", userName);
                newMailBox.Parameters.Add("Alias", userName);
                newMailBox.Parameters.Add("database", dbName);
                newMailBox.Parameters.Add("Password", securePwd);
                newMailBox.Parameters.Add("DisplayName", userName);
                newMailBox.Parameters.Add("UserPrincipalName", string.Format("{0}@{1}", userName, domainName));
                //newMailBox.Parameters.Add("OrganizationalUnit", "ou=myorg,dc=ad,dc=lab");
                newMailBox.Parameters.Add("FirstName", userName);
                p.Commands.Add(newMailBox);

                try
                {
                    Collection<PSObject> result = p.Invoke();
                }
                catch (Exception e)
                {
                    throw e;
                }
            }

        }

        public static SecureString StringToSecureString(string str)
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
