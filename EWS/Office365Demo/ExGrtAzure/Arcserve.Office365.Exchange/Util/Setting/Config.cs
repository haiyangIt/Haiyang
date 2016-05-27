using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Util.Setting
{
    public class Config
    {
        static Config()
        {
            Instance = new Config();
            RestoreCfgInstance = new RestoreConfig();
            MailConfigInstance = new MailConfig();
        }
        public readonly static Config Instance;
        public readonly static RestoreConfig RestoreCfgInstance;
        public readonly static MailConfig MailConfigInstance;
        private Config() { }


        public class RestoreConfig
        {
            public int ZipFileMaxSize
            {
                get
                {
                    return 1024 * 1024 * 10;
                }
            }

            public string SASPolicyName
            {
                get
                {
                    return "Restore_Container_Policy";
                }
            }

            public int SASExpireHours
            {
                get
                {
                    return 96;
                }
            }

            public int SASStartTimeMinute
            {
                get
                {
                    return -30;
                }
            }
        }

        public class MailConfig
        {
            public string MailServer
            {
                get
                {
                    return "smtp.office365.com";
                }
            }

            public int MailPort
            {
                get
                {
                    return 587;
                }
            }

            public string Sender
            {
                get
                {
                    return "devO365admin@arcservemail.onmicrosoft.com";
                }
            }

            public bool EnableSSL
            {
                get
                {
                    return true;
                }
            }

            public bool UseDefaultCredential
            {
                get
                {
                    return false;
                }
            }

            public string CredentialUserName
            {
                get
                {
                    return "devO365admin@arcservemail.onmicrosoft.com";
                }
            }

            public string CredentialPassword
            {
                get
                {
                    return "arcserve1!";
                }
            }

            public SmtpClient Client()
            {
                return new SmtpClient(MailServer, MailPort)
                {
                    EnableSsl = this.EnableSSL,
                    UseDefaultCredentials = this.UseDefaultCredential,
                    Credentials = new NetworkCredential(this.CredentialUserName, this.CredentialPassword, string.Empty)
                };
            }
        }
    }


}
