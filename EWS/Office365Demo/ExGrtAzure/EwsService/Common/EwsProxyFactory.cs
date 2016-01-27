using System;
using System.Net;
using Microsoft.Exchange.WebServices.Data;
using EwsFrame;
using LogInterface;
using EwsServiceInterface;
using EwsFrame.Cache;
using EwsService.Impl;

namespace EwsService.Common
{
    public class EwsProxyFactory
    {

        private static bool DoAutodiscover(ExchangeService exchangeService, string mailboxAddress)
        {
            //service.EnableScpLookup = GlobalSettings.EnableScpLookups;
            string sError = string.Empty;

            try
            {
                exchangeService.AutodiscoverUrl(mailboxAddress, ValidationCallbackHelper.RedirectionUrlValidationCallback);
                //EwsUrl = exchangeService.Url;
                return true;
            }
            catch (AutodiscoverLocalException oException)
            {
                sError += string.Format("Error: {0}\r\n", oException.HResult);
                sError += oException.ToString();
                LogFactory.LogInstance.WriteLog(LogLevel.ERR, "Autodiscovery error", sError);
                throw new ApplicationException("Autodiscovery error", oException);
                return false;
            }
            catch (System.IO.IOException oIOException)
            {
                sError += string.Format("Error: {0}\r\n", oIOException.HResult);
                sError = oIOException.ToString();
                LogFactory.LogInstance.WriteLog(LogLevel.ERR, "Autodiscovery error", sError);
                throw new ApplicationException("Autodiscovery error", oIOException);
                return false;
            }

            //try
            //{
            //    service.EnableScpLookup = GlobalSettings.EnableScpLookups;
            //    service.AutodiscoverUrl(emailAddress.Address, ValidationCallbackHelper.RedirectionUrlValidationCallback);
            //    EwsUrl = service.Url;
            //}
            //catch (AutodiscoverLocalException oException)
            //{
            //    ErrorDialog.ShowError(oException.ToString());
            //}
        }

        public static ExchangeService CreateExchangeService(string mailboxAddress)
        {
            return CreateExchangeService(new EwsServiceArgument(), mailboxAddress);
        }

        public static ExchangeService CreateExchangeService(EwsServiceArgument ewsServiceArgument, string mailboxAddress)
        {
            if (String.IsNullOrEmpty(ewsServiceArgument.ServiceCredential.Password))
            {
                throw new ArgumentException("Please input password first.");
            }

            ewsServiceArgument.EwsUrl = null;
            ExchangeService service = null;
            TimeZoneInfo oTimeZone = null;
            if (ewsServiceArgument.SelectedTimeZoneId != null)
            {
                if (ewsServiceArgument.OverrideTimezone == true)
                {
                    oTimeZone = TimeZoneInfo.FindSystemTimeZoneById(ewsServiceArgument.SelectedTimeZoneId);
                }
            }

            if (ewsServiceArgument.RequestedExchangeVersion.HasValue)
            {
                if (oTimeZone != null)
                    service = new ExchangeService(ewsServiceArgument.RequestedExchangeVersion.Value, oTimeZone);
                else
                    service = new ExchangeService(ewsServiceArgument.RequestedExchangeVersion.Value);


                //System.Diagnostics.Debug.WriteLine(service.PreferredCulture);

            }
            else
            {
                if (oTimeZone != null)
                    service = new ExchangeService(oTimeZone);
                else
                    service = new ExchangeService();
            }

            if (ewsServiceArgument.UserAgent.Length != 0)
                service.UserAgent = ewsServiceArgument.UserAgent;

            // EWS Tracing: http://msdn.microsoft.com/en-us/library/office/dn495632(v=exchg.150).aspx
            service.TraceEnabled = true;
            service.TraceListener = new EwsTraceListener();

            // Instrumentation settings: http://msdn.microsoft.com/en-us/library/office/dn720380(v=exchg.150).aspx
            service.ReturnClientRequestId = true;  // This will give us more data back about the servers used in the response headers
            service.SendClientLatencies = true;  // sends latency info which is used by Microsoft to improve EWS and Exchagne 365.

            if (ewsServiceArgument.EnableScpLookup.HasValue)
            {
                service.EnableScpLookup = ewsServiceArgument.EnableScpLookup.Value;
            }

            if (ewsServiceArgument.PreAuthenticate.HasValue)
            {
                service.PreAuthenticate = ewsServiceArgument.PreAuthenticate.Value;
            }


            if (ewsServiceArgument.OverrideTimeout.HasValue)
            {
                if (ewsServiceArgument.OverrideTimeout == true)
                {
                    if (ewsServiceArgument.Timeout.HasValue)
                        service.Timeout = (int)ewsServiceArgument.Timeout;
                }
            }

            if (ewsServiceArgument.SpecifyProxySettings == true)
            {
                WebProxy oWebProxy = null;
                oWebProxy = new WebProxy(ewsServiceArgument.ProxyServerName, ewsServiceArgument.ProxyServerPort);


                oWebProxy.BypassProxyOnLocal = ewsServiceArgument.BypassProxyForLocalAddress;


                if (ewsServiceArgument.OverrideProxyCredentials == true)
                {

                    if (ewsServiceArgument.ProxyServerUser.Trim().Length == 0)
                    {
                        oWebProxy.UseDefaultCredentials = true;
                    }
                    else
                    {
                        if (ewsServiceArgument.ProxyServerDomain.Trim().Length == 0)
                            oWebProxy.Credentials = new NetworkCredential(ewsServiceArgument.ProxyServerUser, ewsServiceArgument.ProxyServerPassword);
                        else
                            oWebProxy.Credentials = new NetworkCredential(ewsServiceArgument.ProxyServerUser, ewsServiceArgument.ProxyServerPassword, ewsServiceArgument.ProxyServerDomain);
                    }
                }
                else
                {

                    oWebProxy.UseDefaultCredentials = true;
                }
                service.WebProxy = oWebProxy;

            }


            if (ewsServiceArgument.ServiceCredential != null)
            {
                service.Credentials = ewsServiceArgument.ServiceCredential;
            }

            if (ewsServiceArgument.EwsUrl != null)
            {
                service.Url = ewsServiceArgument.EwsUrl;
            }



            if (ewsServiceArgument.UseDefaultCredentials.HasValue)
            {
                service.UseDefaultCredentials = ewsServiceArgument.UseDefaultCredentials.Value;
            }

            if (ewsServiceArgument.UserToImpersonate != null)
            {
                service.ImpersonatedUserId = ewsServiceArgument.UserToImpersonate;

                if(ewsServiceArgument.SetXAnchorMailbox)
                    service.HttpHeaders.Add("X-AnchorMailbox", mailboxAddress);

                // Set headers which help with affinity when Impersonation is being used against Exchange 2013 and Exchagne Online 15.
                // http://blogs.msdn.com/b/mstehle/archive/2013/07/17/more-affinity-considerations-for-exchange-online-and-exchange-2013.aspx
                if (service.RequestedServerVersion.ToString().StartsWith("Exchange2007") == false &&
                    service.RequestedServerVersion.ToString().StartsWith("Exchange2010") == false)
                {
                    //// Should set for 365...:

                    //if (service.HttpHeaders.ContainsKey("X-AnchorMailbox") == false)
                    //    service.HttpHeaders.Add("X-AnchorMailbox", service.ImpersonatedUserId.Id);
                    //else
                    //    service.HttpHeaders["X-AnchorMailbox"] = service.ImpersonatedUserId.Id;

                    //if (service.HttpHeaders.ContainsKey("X-PreferServerAffinity") == false)
                    //    service.HttpHeaders.Add("X-PreferServerAffinity", "true");
                    //else
                    //    service.HttpHeaders["X-PreferServerAffinity"] = "true";
                }
            }

            var domainName = AutodiscoveryUrlCache.GetDomainName(mailboxAddress);
            var urlCache = OrganizationCacheManager.CacheManager.GetCache(domainName, AutodiscoveryUrlCache.CacheName);
            if(urlCache == null)
            {
                urlCache = OrganizationCacheManager.CacheManager.NewCache(domainName, AutodiscoveryUrlCache.CacheName, typeof(AutodiscoveryUrlCache));
            }

            StringCacheKey mailboxKey = new StringCacheKey(domainName);
            object urlObj = null;
            if(!urlCache.TryGetValue(mailboxKey, out urlObj))
            {
                DoAutodiscover(service, mailboxAddress);
                urlObj = service.Url;
                urlCache.AddKeyValue(mailboxKey, urlObj);
            }
            else
            {
                try
                {
                    service.Url = new Uri(urlObj.ToString());
                    TestExchangeService(service);
                }
                catch(Exception ex)
                {
                    DoAutodiscover(service, mailboxAddress);
                    urlCache.SetKeyValue(mailboxKey, service.Url);
                }
            }

            ewsServiceArgument.EwsUrl = service.Url;
            return service;
        }

        /// <summary>
        ///  This is used for preparing an HttpWebRequest for a raw post.
        /// </summary>
        /// <param name="oRequest"></param>
        public static void CreateHttpWebRequest(ref HttpWebRequest oRequest, EwsServiceArgument argument)
        {
            if (String.IsNullOrEmpty(argument.ServiceCredential.Password))
            {
                throw new ArgumentException("Please input password first.");
            }

            HttpWebRequest oHttpWebRequest = (HttpWebRequest)WebRequest.Create(argument.EwsUrl);

            if (argument.UserAgent.Length != 0)
                oHttpWebRequest.Headers.Add("UserAgent", argument.UserAgent);

            oHttpWebRequest.Method = "POST";
            oHttpWebRequest.ContentType = "text/xml";

            if (argument.OverrideTimeout.HasValue)
            {
                if (argument.OverrideTimeout == true)
                {
                    if (argument.Timeout.HasValue)
                        oHttpWebRequest.Timeout = (int)argument.Timeout;
                }
            }

            oHttpWebRequest.Headers.Add("Translate", "f");
            oHttpWebRequest.Headers.Add("Pragma", "no-cache");
            oHttpWebRequest.Headers.Add("return-client-request-id", "true");  // This will give us more data back about the servers used in the response headers
            if (argument.PreAuthenticate.HasValue)
            {
                oHttpWebRequest.Headers.Add("PreAuthenticate", argument.PreAuthenticate.Value.ToString());
            }

            // TODO:  Add timezone injection
            //TimeZoneInfo oTimeZone = null;
            //if (SelectedTimeZoneId != null)
            //{
            //    if (OverrideTimezone == true)
            //    {
            //        oTimeZone = TimeZoneInfo.FindSystemTimeZoneById(SelectedTimeZoneId);
            //    }
            //}

            //if (RequestedExchangeVersion.HasValue)
            //{
            //    if (oTimeZone != null)
            //        service = new ExchangeService(RequestedExchangeVersion.Value, oTimeZone);
            //    else
            //        service = new ExchangeService(RequestedExchangeVersion.Value);


            //    //System.Diagnostics.Debug.WriteLine(service.PreferredCulture);

            //}
            //else
            //{
            //    if (oTimeZone != null)
            //        service = new ExchangeService(oTimeZone);
            //    else
            //        service = new ExchangeService();
            //}


            if (argument.SpecifyProxySettings == true)
            {
                WebProxy oWebProxy = null;
                oWebProxy = new WebProxy(argument.ProxyServerName, argument.ProxyServerPort);

                oWebProxy.BypassProxyOnLocal = argument.BypassProxyForLocalAddress;


                if (argument.OverrideProxyCredentials == true)
                {

                    if (argument.ProxyServerUser.Trim().Length == 0)
                    {
                        oWebProxy.UseDefaultCredentials = true;
                    }
                    else
                    {
                        if (argument.ProxyServerDomain.Trim().Length == 0)
                            oWebProxy.Credentials = new NetworkCredential(argument.ProxyServerUser, argument.ProxyServerPassword);
                        else
                            oWebProxy.Credentials = new NetworkCredential(argument.ProxyServerUser, argument.ProxyServerPassword, argument.ProxyServerDomain);
                    }
                }
                else
                {

                    oWebProxy.UseDefaultCredentials = true;
                }
                oHttpWebRequest.Proxy = oWebProxy;
            }


            if (argument.UseDefaultCredentials.HasValue)
            {
                oHttpWebRequest.UseDefaultCredentials = argument.UseDefaultCredentials.Value;
            }


            if (argument.ServiceCredential != null)
            {
                oHttpWebRequest.Credentials = argument.ServiceCredential;
            }
            //else
            //{
            //    oHttpWebRequest.Credentials =   GetNetworkCredential();

            //}



            //if (ServiceCredential != null)
            //{
            //    service.Credentials = ServiceCredential;
            //}

            //    if (sAuthentication == "DefaultCredentials")
            //    {
            //        oHttpWebRequest.UseDefaultCredentials = true;
            //        oHttpWebRequest.Credentials = CredentialCache.DefaultCredentials;
            //    }
            //    else
            //    {
            //        if (sAuthentication == "DefaultNetworkCredentials")
            //            oHttpWebRequest.Credentials = CredentialCache.DefaultNetworkCredentials;
            //        else
            //        {
            //            oHttpWebRequest.Credentials = oCrentialCache;
            //        }
            //    }


            if (argument.UserToImpersonate != null)
            {
                //service.ImpersonatedUserId = UserToImpersonate;
                // TODO: Add injection of impersonation.

            }

            oRequest = oHttpWebRequest;


        }

        //private void InitializeWithDefaults()
        //{
        //    InitializeWithDefaults(null, null, null);
        //}

        ///// <summary>
        ///// Create a service binding based off of default credentials,
        ///// the assumed root folder, and an assumed autodiscover email address
        ///// </summary>
        ///// <param name="version">EWS schema version to use.  Passing NULL uses the
        ///// EWS Managed API default value.</param>
        ///// <param name="ewsUrl">URL to EWS endpoint.  Passing NULL or an empty string
        ///// results in a call to Autodiscover</param>
        ///// <param name="autodiscoverAddress">Email address to use for Autodiscover.
        ///// Passing NULL or an empty string results in a ActiveDirectory querying.</param>
        ///// <returns>A new instance of an ExchangeService</returns>
        //private void InitializeWithDefaults(ExchangeVersion? version, Uri ewsUrl, string autodiscoverAddress)
        //{
        //    RequestedExchangeVersion = version;
        //    UseDefaultCredentials = true;

        //    // If the EWS URL is not specified, use Autodiscover to find it
        //    if (ewsUrl == null)
        //    {
        //        // If no email address was given to use with Autodiscover, attempt
        //        // to look it up in Active Directory
        //        if (String.IsNullOrEmpty(autodiscoverAddress))
        //        {
        //            autodiscoverAddress = ActiveDirectoryHelper.GetPrimarySmtp(
        //                System.Security.Principal.WindowsIdentity.GetCurrent().Name);
        //        }

        //        DoAutodiscover(autodiscoverAddress);
        //    }
        //    else
        //    {
        //        EwsUrl = ewsUrl;
        //    }

        //    try
        //    {
        //        CreateExchangeService().TestExchangeService();
        //    }
        //    catch (ServiceVersionException ex)
        //    {
        //        LogFactory.LogInstance().WriteLog(LogLevel.ERR, ex.Message, "Initial requested version of {0} didn't work,detail:{1}", Enum.GetName(typeof(ExchangeVersion), version), ex.StackTrace);
        //        // Pass the autodiscover email address and URL if we've already looked those up
        //        //InitializeWithDefaults(ExchangeVersion.Exchange2007_SP1, EwsUrl, autodiscoverAddress);
        //    }

        //}

        public static void TestExchangeService(ExchangeService service)
        {
            // mstehle - 11/15/2011 - The validation override is now handled by GlobalSettings, no need
            // to do all this stuff anymore.  Just try ConvertIds and let the exceptions bubble up.
            service.ConvertIds(
                new AlternateId[] { new AlternateId(IdFormat.HexEntryId, "00", "blah@blah.com") },
                IdFormat.HexEntryId);
        }
    }
}
