using EwsFrame.Util;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace EwsService.Common
{
    public class CertificateValidationHelper
    {
        private static List<String> allowedUserAgents = new List<string>();
        private static List<Uri> allowedUrls = new List<Uri>();

        private static object uaLock = new object();
        private static object urlLock = new object();

        
         

        public static bool ServerCertificateValidationCallback(
                    Object obj,
                    X509Certificate certificate,
                    X509Chain chain,
                    SslPolicyErrors errors)
        {
            bool result = false;

            HttpWebRequest request = obj as HttpWebRequest;
            if (request != null)
            {
                // Check for allowed UserAgent
                

                using (uaLock.LockWhile(() =>
                {
                    if (CertificateValidationHelper.allowedUserAgents.Contains(request.UserAgent))
                    {
                        result = true;
                    }
                }))
                { };

                // Check for allowed Url
                
                using (urlLock.LockWhile(() =>
                {
                    if (CertificateValidationHelper.allowedUrls.Contains(request.RequestUri))
                    {
                        result = true;
                    }
                }))
                { };
            }

            return result;
        }

 
    }
}
