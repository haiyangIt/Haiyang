using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EwsServiceInterface;
using EwsFrame;
using System.Configuration;

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
    }
}
