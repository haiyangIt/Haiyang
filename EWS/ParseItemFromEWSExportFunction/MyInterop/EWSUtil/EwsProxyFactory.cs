using System;
using System.Net;
using Microsoft.Exchange.WebServices.Data;
using FTStreamUtil;
using Microsoft.Exchange.WebServices.Autodiscover;
using MyInterop.EWSUtil.Setting.Internals;

namespace EWSUtil
{
    public class EwsProxyFactory
    {
        public static ExchangeVersion? RequestedExchangeVersion = null;
        public static bool? OverrideTimezone;
        public static string SelectedTimeZoneId;
        public static bool? AllowAutodiscoverRedirect = null;
        public static bool? EnableScpLookup;
        public static bool? PreAuthenticate;
        public static NetworkCredential ServiceCredential = null;
        public static Microsoft.Exchange.WebServices.Data.EmailAddress ServiceEmailAddress = null;
        public static Uri EwsUrl;
        public static bool? OverrideTimeout;
        public static int? Timeout = null;
        public static bool? UseDefaultCredentials = null;
        public static ImpersonatedUserId UserToImpersonate = null;
        public static string UserAgent;

        public static bool SetDefaultProxy = false;
        public static bool BypassProxyForLocalAddress = false;
        public static bool SpecifyProxySettings;
        public static string ProxyServerName;
        public static int ProxyServerPort;
        public static bool OverrideProxyCredentials;
        public static string ProxyServerUser;
        public static string ProxyServerPassword;
        public static string ProxyServerDomain;



        public static int DoAutodiscover(out ExchangeService service)
        {
            return DoAutodiscover(ServiceEmailAddress,out service);
        }

        public static int DoAutodiscover(Microsoft.Exchange.WebServices.Data.EmailAddress emailAddress,out ExchangeService service)
        {
            service = null;
            try
            {
                service = CreateExchangeService();
                service.AutodiscoverUrl(emailAddress.Address, ValidationCallbackHelper.RedirectionUrlValidationCallback);
                EwsUrl = service.Url;
                return 0;
            }
            catch (AutodiscoverLocalException oException) //todo HResult is only support in .NetFrameWork4.5
            {
                LogWriter.Instance.WriteException(typeof(EwsProxyFactory), oException);
                return ExceptionHResult.Default.AutodiscoverLocalException;
            }
            catch (AutodiscoverRemoteException oRemoteException)
            {
                LogWriter.Instance.WriteException(typeof(EwsProxyFactory), oRemoteException);
                return ExceptionHResult.Default.AutodiscoverRemoteException;
            }
            catch (ServiceValidationException serviceValidateException)
            {
                LogWriter.Instance.WriteException(typeof(EwsProxyFactory), serviceValidateException);
                return ExceptionHResult.Default.ServiceValidationException;
            }
            catch (System.IO.IOException oIOException)
            {
                LogWriter.Instance.WriteException(typeof(EwsProxyFactory), oIOException);
                return ExceptionHResult.Default.IOException;
            }
            catch (Exception e)
            {
                LogWriter.Instance.WriteException(typeof(EwsProxyFactory), e);
                return ExceptionHResult.Default.OtherException;
            }

        }

        public static ExchangeService CreateExchangeService()
        {
            ExchangeService service = null;

            TimeZoneInfo oTimeZone = null;
            if (SelectedTimeZoneId != null)
            {
                if (OverrideTimezone == true)
                {
                    oTimeZone = TimeZoneInfo.FindSystemTimeZoneById(SelectedTimeZoneId);
                }
            }

            if (RequestedExchangeVersion.HasValue)
            {
                if (oTimeZone != null)
                    service = new ExchangeService(RequestedExchangeVersion.Value, oTimeZone);
                else
                    service = new ExchangeService(RequestedExchangeVersion.Value);


                //System.Diagnostics.Debug.WriteLine(service.PreferredCulture);

            }
            else
            {
                if (oTimeZone != null)
                    service = new ExchangeService(oTimeZone);
                else
                    service = new ExchangeService();
            }

            if (UserAgent.Length != 0)
                service.UserAgent = UserAgent;

            // EWS Tracing: http://msdn.microsoft.com/en-us/library/office/dn495632(v=exchg.150).aspx
            // todo Config Trace
            service.TraceEnabled = false;
            //service.TraceListener = new EWSEditor.Logging.EwsTraceListener();

            // Instrumentation settings: http://msdn.microsoft.com/en-us/library/office/dn720380(v=exchg.150).aspx
            service.ReturnClientRequestId = true;  // This will give us more data back about the servers used in the response headers
            service.SendClientLatencies = true;  // sends latency info which is used by Microsoft to improve EWS and Exchagne 365.

            if (EnableScpLookup.HasValue)
            {
                service.EnableScpLookup = EnableScpLookup.Value;
            }

            if (PreAuthenticate.HasValue)
            {
                service.PreAuthenticate = PreAuthenticate.Value;
            }


            if (OverrideTimeout.HasValue)
            {
                if (OverrideTimeout == true)
                {
                    if (Timeout.HasValue)
                        service.Timeout = (int)Timeout;
                }
            }

            if (SpecifyProxySettings == true)
            {
                WebProxy oWebProxy = null;
                oWebProxy = new WebProxy(ProxyServerName, ProxyServerPort);


                oWebProxy.BypassProxyOnLocal = BypassProxyForLocalAddress;


                if (OverrideProxyCredentials == true)
                {

                    if (ProxyServerUser.Trim().Length == 0)
                    {
                        oWebProxy.UseDefaultCredentials = true;
                    }
                    else
                    {
                        if (ProxyServerDomain.Trim().Length == 0)
                            oWebProxy.Credentials = new NetworkCredential(ProxyServerUser, ProxyServerPassword);
                        else
                            oWebProxy.Credentials = new NetworkCredential(ProxyServerUser, ProxyServerPassword, ProxyServerDomain);
                    }
                }
                else
                {

                    oWebProxy.UseDefaultCredentials = true;
                }
                service.WebProxy = oWebProxy;

            }


            if (ServiceCredential != null)
            {
                service.Credentials = ServiceCredential;
            }

            if (EwsUrl != null)
            {
                service.Url = EwsUrl;
            }



            if (UseDefaultCredentials.HasValue)
            {
                service.UseDefaultCredentials = UseDefaultCredentials.Value;
            }

            if (UserToImpersonate != null)
            {
                service.ImpersonatedUserId = UserToImpersonate;

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

            return service;
        }

        /// <summary>
        ///  This is used for preparing an HttpWebRequest for a raw post.
        /// </summary>
        /// <param name="oRequest"></param>
        public static void CreateHttpWebRequest(ref HttpWebRequest oRequest)
        {
            HttpWebRequest oHttpWebRequest = (HttpWebRequest)WebRequest.Create(EwsUrl);

            if (UserAgent.Length != 0)
                oHttpWebRequest.Headers.Add("UserAgent", UserAgent);

            oHttpWebRequest.Method = "POST";
            oHttpWebRequest.ContentType = "text/xml";

            if (OverrideTimeout.HasValue)
            {
                if (OverrideTimeout == true)
                {
                    if (Timeout.HasValue)
                        oHttpWebRequest.Timeout = (int)Timeout;
                }
            }

            oHttpWebRequest.Headers.Add("Translate", "f");
            oHttpWebRequest.Headers.Add("Pragma", "no-cache");
            oHttpWebRequest.Headers.Add("return-client-request-id", "true");  // This will give us more data back about the servers used in the response headers
            if (PreAuthenticate.HasValue)
            {
                oHttpWebRequest.Headers.Add("PreAuthenticate", PreAuthenticate.Value.ToString());
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


            if (SpecifyProxySettings == true)
            {
                WebProxy oWebProxy = null;
                oWebProxy = new WebProxy(ProxyServerName, ProxyServerPort);

                oWebProxy.BypassProxyOnLocal = BypassProxyForLocalAddress;


                if (OverrideProxyCredentials == true)
                {

                    if (ProxyServerUser.Trim().Length == 0)
                    {
                        oWebProxy.UseDefaultCredentials = true;
                    }
                    else
                    {
                        if (ProxyServerDomain.Trim().Length == 0)
                            oWebProxy.Credentials = new NetworkCredential(ProxyServerUser, ProxyServerPassword);
                        else
                            oWebProxy.Credentials = new NetworkCredential(ProxyServerUser, ProxyServerPassword, ProxyServerDomain);
                    }
                }
                else
                {

                    oWebProxy.UseDefaultCredentials = true;
                }
                oHttpWebRequest.Proxy = oWebProxy;
            }


            if (UseDefaultCredentials.HasValue)
            {
                oHttpWebRequest.UseDefaultCredentials = UseDefaultCredentials.Value;
            }


            if (ServiceCredential != null)
            {
                oHttpWebRequest.Credentials = ServiceCredential;
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


            if (UserToImpersonate != null)
            {
                //service.ImpersonatedUserId = UserToImpersonate;
                // TODO: Add injection of impersonation.

            }

            oRequest = oHttpWebRequest;


        }

    }
}
