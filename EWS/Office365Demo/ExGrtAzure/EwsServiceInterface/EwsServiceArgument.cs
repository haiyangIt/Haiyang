using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EwsServiceInterface
{
    public class EwsServiceArgument
    {
        public ExchangeVersion? RequestedExchangeVersion = ExchangeVersion.Exchange2013_SP1;
        public bool? OverrideTimezone;
        public string SelectedTimeZoneId;
        public bool? AllowAutodiscoverRedirect = true;
        public bool? EnableScpLookup;
        public bool? PreAuthenticate;
        public NetworkCredential ServiceCredential = null;
        public Microsoft.Exchange.WebServices.Data.EmailAddress ServiceEmailAddress = null;
        public Uri EwsUrl;
        public bool? OverrideTimeout;
        public int? Timeout = null;
        public bool? UseDefaultCredentials = null;
        public ImpersonatedUserId UserToImpersonate;
        public string UserAgent;

        public bool SetDefaultProxy = false;
        public bool BypassProxyForLocalAddress = false;
        public bool SpecifyProxySettings;
        public string ProxyServerName;
        public int ProxyServerPort;
        public bool OverrideProxyCredentials;
        public string ProxyServerUser;
        public string ProxyServerPassword;
        public string ProxyServerDomain;

        public bool SetXAnchorMailbox;
        public string XAnchorMailbox;

        public string CurrentMailbox
        {
            get
            {
                return ServiceEmailAddress.Address;
            }
        }

        public EwsServiceArgument()
        {
            RequestedExchangeVersion = ExchangeVersion.Exchange2013_SP1;

            OverrideTimezone = EWSSetting.Instance.OverrideTimezone;
            SelectedTimeZoneId = EWSSetting.Instance.SelectedTimeZoneId;

            AllowAutodiscoverRedirect = EWSSetting.Instance.AllowAutodiscoverRedirect;
            UseDefaultCredentials = false;
            EnableScpLookup = EWSSetting.Instance.EnableScpLookups;
            PreAuthenticate = EWSSetting.Instance.PreAuthenticate;

            OverrideTimeout = EWSSetting.Instance.OverrideTimeout;
            Timeout = EWSSetting.Instance.Timeout;
            UserAgent = EWSSetting.Instance.UserAgent;

            SetDefaultProxy = EWSSetting.Instance.SetDefaultProxy;
            BypassProxyForLocalAddress = EWSSetting.Instance.BypassProxyForLocalAddress;
            SpecifyProxySettings = EWSSetting.Instance.SpecifyProxySettings;
            ProxyServerName = EWSSetting.Instance.ProxyServerName;
            ProxyServerPort = EWSSetting.Instance.ProxyServerPort;
            OverrideProxyCredentials = EWSSetting.Instance.OverrideProxyCredentials;
            ProxyServerUser = EWSSetting.Instance.ProxyServerUser;
            ProxyServerPassword = EWSSetting.Instance.ProxyServerPassword;
            ProxyServerDomain = EWSSetting.Instance.ProxyServerDomain;

            SetXAnchorMailbox = EWSSetting.Instance.SetXAnchorMailbox;
            XAnchorMailbox = EWSSetting.Instance.XAnchorMailbox;
        }

        public void SetConnectMailbox(string currentMailbox)
        {
            if (currentMailbox.ToLower() != ServiceCredential.UserName.ToLower())
            {
                ServiceEmailAddress = currentMailbox;
                UserToImpersonate = new ImpersonatedUserId(ConnectingIdType.SmtpAddress, currentMailbox);
                SetXAnchorMailbox = true;
                XAnchorMailbox = currentMailbox;
            }
            else
            {
                if(UserToImpersonate != null)
                {
                    UserToImpersonate = null;
                    SetXAnchorMailbox = false;
                    XAnchorMailbox = string.Empty;
                }
                ServiceEmailAddress = currentMailbox;
            }

        }
    }
}
