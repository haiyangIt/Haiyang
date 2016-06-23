using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using Arcserve.Office365.Exchange.DataProtect.Tool.Result;
using Arcserve.Office365.Exchange.Tool;
using System.IO;
using Arcserve.Office365.Exchange.DataProtect.Tool.Data;
using Arcserve.Office365.Exchange.Data.Mail;

namespace ExGrtAzure.Tests
{
    [TestClass]
    public class CmdProcessCallTest
    {
        [TestMethod]
        public void TestCallGetAllMailbox()
        {
            var arg = string.Format("-JobType:{0} -AdminUserName:{1} -AdminPassword:{2}",
                "GetAllMailbox",
                "devO365admin@arcservemail.onmicrosoft.com",
                "JackyMao1!");
            ProcessStartInfo startInfo = new ProcessStartInfo("Arcserve.Office365.Exchange.DataProtect.Tool.exe", arg);
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            using (var p = Process.Start(startInfo))
            {
                using (StreamReader reader = p.StandardOutput)
                {
                    var str = reader.ReadToEnd();
                    Console.WriteLine(str);
                    Debug.WriteLine(str);
                }
            }
        }

        [TestMethod]
        public void TestCallGetAllMailboxWithWrongPsw()
        {
            var arg = string.Format("-JobType:{0} -AdminUserName:{1} -AdminPassword:{2}",
                "GetAllMailbox",
                "devO365admin@arcservemail.onmicrosoft.com",
                "JackyMao1");
            ProcessStartInfo startInfo = new ProcessStartInfo("Arcserve.Office365.Exchange.DataProtect.Tool.exe", arg);
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            using (var p = Process.Start(startInfo))
            {
                using (StreamReader reader = p.StandardOutput)
                {
                    var str = reader.ReadToEnd();
                    Console.WriteLine(str);
                    Debug.WriteLine(str);
                }
            }
        }

        [TestMethod]
        public void TestCallGetAllMailboxWithNotExistUser()
        {
            var arg = string.Format("-JobType:{0} -AdminUserName:{1} -AdminPassword:{2}",
                "GetAllMailbox",
                "devO365ad@arcservemail.onmicrosoft.com",
                "JackyMao1!");
            ProcessStartInfo startInfo = new ProcessStartInfo("Arcserve.Office365.Exchange.DataProtect.Tool.exe", arg);
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            using (var p = Process.Start(startInfo))
            {
                using (StreamReader reader = p.StandardOutput)
                {
                    var str = reader.ReadToEnd();
                    Console.WriteLine(str);
                    Debug.WriteLine(str);
                }
            }
        }

        [TestMethod]
        public void TestCallGetAllMailboxNoNetConnect()
        {
            var arg = string.Format("-JobType:{0} -AdminUserName:{1} -AdminPassword:{2}",
                "GetAllMailbox",
                "devO365admin@arcservemail.onmicrosoft.com",
                "JackyMao1!");
            ProcessStartInfo startInfo = new ProcessStartInfo("Arcserve.Office365.Exchange.DataProtect.Tool.exe", arg);
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            using (var p = Process.Start(startInfo))
            {
                using (StreamReader reader = p.StandardOutput)
                {
                    var str = reader.ReadToEnd();
                    Console.WriteLine(str);
                    Debug.WriteLine(str);
                }
            }
        }

        [TestMethod]
        public void TestCallGetAllMailboxDisConnect()
        {
            var arg = string.Format("-JobType:{0} -AdminUserName:{1} -AdminPassword:{2}",
                "GetAllMailbox",
                "devO365admin@arcservemail.onmicrosoft.com",
                "JackyMao1!");
            ProcessStartInfo startInfo = new ProcessStartInfo("Arcserve.Office365.Exchange.DataProtect.Tool.exe", arg);
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            using (var p = Process.Start(startInfo))
            {
                using (StreamReader reader = p.StandardOutput)
                {
                    var str = reader.ReadToEnd();
                    Console.WriteLine(str);
                    Debug.WriteLine(str);
                }
            }
        }

        [TestMethod]
        public void XmlSerialize()
        {
            MailboxList list = new MailboxList();
            list.Add(new Mailbox() { DisplayName = "1", Id = "2", MailAddress = "3", Name = "4" });
            var result = new GetAllMailboxResult(list);
            var str = ResultBase.Serialize(result);
        }
    }
}
