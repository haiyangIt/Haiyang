using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace EwsFrame.Util
{
    public class SendMailHelper
    {
        private List<string> DownloadUrl = new List<string>();

        public void AddDownloadUrl(string downloadUrl)
        {
            DownloadUrl.Add(downloadUrl);
        }

        public void AddDownloadUrls(List<string> urls)
        {
            DownloadUrl.AddRange(urls);
        }

        public string GetHtmlBody()
        {
            if (DownloadUrl.Count == 0)
                throw new ArgumentException("No restore zip download urls.");

            StringBuilder sb = new StringBuilder();
            int index = 1;
            foreach(var url in DownloadUrl)
            {
                sb.AppendFormat(MailConfig.HtmlBodyForRestoreUrlTemplate, HttpUtility.HtmlEncode(url), index++);
            }

            return string.Format(MailConfig.HtmlBodyTemplate, DownloadUrl.Count, sb.ToString());
        }
    }
}
