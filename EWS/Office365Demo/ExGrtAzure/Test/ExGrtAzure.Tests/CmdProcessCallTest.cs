using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.IO;

namespace ExGrtAzure.Tests
{
    [TestClass]
    public class CmdProcessCallTest
    {
        public string ProcessPath
        {
            get
            {
                var path = AppDomain.CurrentDomain.BaseDirectory;
                DirectoryInfo info = new DirectoryInfo(path);
                do
                {
                    info = info.Parent;
                } while (info.Name != "Test");

                info = info.Parent;
                path = Path.Combine(info.FullName, "packages");
                path = Path.Combine(path, "Arcserve_Office365_dll");
                return Path.Combine(path, "Arcserve.Office365.Exchange.DataProtect.Tool.exe");
            }
        }

        [TestMethod]
        public void TestCallGetAllMailbox()
        {
            var arg = string.Format("-JobType:{0} -AdminUserName:{1} -AdminPassword:{2}",
                "GetAllMailbox",
                "devO365admin@arcservemail.onmicrosoft.com",
                "JackyMao1!");
            ProcessStartInfo startInfo = new ProcessStartInfo(ProcessPath, arg);
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
            ProcessStartInfo startInfo = new ProcessStartInfo(ProcessPath, arg);
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
            ProcessStartInfo startInfo = new ProcessStartInfo(ProcessPath, arg);
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
            ProcessStartInfo startInfo = new ProcessStartInfo(ProcessPath, arg);
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
            ProcessStartInfo startInfo = new ProcessStartInfo(ProcessPath, arg);
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
        public void TestCallValidateUserSuccess()
        {
            var arg = string.Format("-JobType:{0} -AdminUserName:{1} -AdminPassword:{2}",
                   "ValidateUser",
                   "devO365admin@arcservemail.onmicrosoft.com",
                   "JackyMao1!");
            ProcessStartInfo startInfo = new ProcessStartInfo(ProcessPath, arg);
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
        public void TestCallValidateUserPasswordWrong()
        {
            var arg = string.Format("-JobType:{0} -AdminUserName:{1} -AdminPassword:{2}",
                "ValidateUser",
                "devO365admin@arcservemail.onmicrosoft.com",
                "JackyMao1");
            ProcessStartInfo startInfo = new ProcessStartInfo(ProcessPath, arg);
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
        public void TestCallValidateUserUserWrong()
        {
            var arg = string.Format("-JobType:{0} -AdminUserName:{1} -AdminPassword:{2}",
                "ValidateUser",
                "devO365admi@arcservemail.onmicrosoft.com",
                "JackyMao1!");
            ProcessStartInfo startInfo = new ProcessStartInfo(ProcessPath, arg);
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
        public void TestCallValidateUserImpersonateWrong()
        {
            var arg = string.Format("-JobType:{0} -AdminUserName:{1} -AdminPassword:{2} -ConnectMailbox:{3}",
                "ValidateUser",
                "devO365admin@arcservemail.onmicrosoft.com",
                "JackyMao1", "Hualiang.Wang@arcserve.com");
            ProcessStartInfo startInfo = new ProcessStartInfo(ProcessPath, arg);
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
    }
}
