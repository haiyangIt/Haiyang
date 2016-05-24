using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EwsServiceInterface;
using EwsFrame;
using System.Configuration;
using DataProtectInterface;
using System.Collections.Generic;
using DataProtectImpl;
using System.Xml.Serialization;
using System.IO;
using System.Text;
using System.Diagnostics;
using Newtonsoft.Json;

namespace ExGrtAzure.Tests
{
    [TestClass]
    public class EwsAdapterTest
    {
        [TestMethod]
        public void TestRetryExportBin()
        {
            IEwsAdapter adpater = CatalogFactory.Instance.NewEwsAdapter();
            var argument = new EwsServiceArgument();
            argument.ServiceCredential = new System.Net.NetworkCredential("devO365admin@arcservemail.onmicrosoft.com", "JackyMao1!");
            argument.SetConnectMailbox("haiyang.ling@arcserve.com");
            adpater.ConnectMailbox(argument, "haiyang.ling@arcserve.com");
            ConfigurationManager.AppSettings["ExportItemTimeOut"] = 1.ToString();
            var val = ConfigurationManager.AppSettings["ExportItemTimeOut"];
            adpater.ExportItem("AAMkAGYxYzc0MTAyLTI3MjAtNDA5Zi04ZDY2LTlmODU5NmJkZDlhZgBGAAAAAADKmQFKsxwfSKwEXH3khGtpBwA1BzsargwHRq9aRKbs1Mp0AAAx45xfAAA1BzsargwHRq9aRKbs1Mp0AAAx4+nQAAA=", argument);
        }

        [TestMethod]
        public void TestGetAllMailbox()
        {
            List<Mailbox> allMails = null;
            using (ICatalogService service = CatalogFactory.Instance.NewCatalogService("devO365admin@arcservemail.onmicrosoft.com", "JackyMao1!", null, "arcserve"))
            {
                var allMailboxes = service.GetAllUserMailbox();
                allMails = new List<Mailbox>(allMailboxes.Count);
                int index = 0;
                foreach (var item in allMailboxes)
                {
                    allMails.Add(new Mailbox()
                    {
                        Name = ((CatalogService.MailboxInfo)item).Name,
                        DisplayName = item.DisplayName,
                        MailAddress = item.MailAddress,
                        Id = string.Format("MailId{0}", index)
                    });
                    index++;
                }
            }

            XmlSerializer s = new XmlSerializer(typeof(List<Mailbox>));

            using (StreamWriter writer = new StreamWriter(@"D:\123.xml"))
            {
                s.Serialize(writer, allMails);
            }

            StringBuilder sb = new StringBuilder();
            using (StringWriter w = new StringWriter(sb))
            {
                s.Serialize(w, allMails);
            }

            Debug.WriteLine(sb);
        }

        [TestMethod]
        public void ParseSelectedFile()
        {

            var file = @"D:\21GitHub\Haiyang\EWS\Office365Demo\ExGrtAzure\LoginTest\bin\OneMailboxFull.txt";
            using (StreamReader reader = new StreamReader(file))
            {
                var str = reader.ReadToEnd();
                var obj = JsonConvert.DeserializeObject<LoadedTreeItem>(str);
            }
        }

        [TestMethod]
        public void TestFunc()
        {
            TimeSpan t = new TimeSpan(0, 105, 0);
            var result = GetGitaEachHour(3330040508, t);
            Debug.WriteLine(result);
        }
        public string GetGitaEachHour(long actualSize, TimeSpan timeSpan)
        {
            double result = (double)actualSize / (1024 * 1024 * 1024 * timeSpan.TotalHours);
            return result.ToString("0.00");
        }
    }

    public class Mailbox
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string MailAddress { get; set; }
        public string Id { get; set; }
    }
}
