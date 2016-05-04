using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EwsFrame
{
    public class Config
    {

        private static object _lock = new object();
        private static Config _instance = null;
        public static Config Instance
        {
            get
            {
                if (_instance == null)
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new Config();
                        }
                    }
                return _instance;
            }
        }

        public static RestoreConfig _RestoreCfgInstance;
        public  static RestoreConfig RestoreCfgInstance
        {
            get
            {
                if (_RestoreCfgInstance == null)
                    lock (_lock)
                    {
                        if (_RestoreCfgInstance == null)
                        {
                            _RestoreCfgInstance = new RestoreConfig();
                        }
                    }
                return _RestoreCfgInstance;
            }
        } 
        public static MailConfig _MailConfigInstance;
        public  static MailConfig MailConfigInstance
        {
            get
            {
                if (_MailConfigInstance == null)
                    lock (_lock)
                    {
                        if (_MailConfigInstance == null)
                        {
                            _MailConfigInstance = new MailConfig();
                        }
                    }
                return _MailConfigInstance;
            }
        } 
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
                    return "JackyMao1!";
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
