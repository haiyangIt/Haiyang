using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FTStreamUtil.Build.Implement;
using System.Runtime.InteropServices;

namespace TestBuildMessage
{
    [TestClass]
    public class TestImportFile
    {
        [TestMethod]
        public void TestImportFTStream()
        {
            RestoreMsg restoreMsg = new RestoreMsg();
            string userMail = "xila@linha05-ex20131.com";
            string administratorUser = "administrator";
            string administratorPsw = "arcserve!2013";
            string domain = "linha05-ex20131.com";

            IntPtr userMainPtr = GetStringIntPtr(userMail);
            IntPtr adminUserPtr = GetStringIntPtr(administratorUser);
            IntPtr adminPswPtr = GetStringIntPtr(administratorPsw);
            IntPtr domainPtr = GetStringIntPtr(domain);

            int result = restoreMsg.AutoDiscovery(userMainPtr,adminUserPtr,adminPswPtr,domainPtr);
            Assert.AreEqual(result, 0);

            string folderPath = "Inbox\\0206";
            IntPtr folderPtr = GetStringIntPtr(folderPath);
            string folderPathTemp = Marshal.PtrToStringUni(folderPtr);
            Assert.AreEqual(folderPath, folderPathTemp);
            result = restoreMsg.RestoreToFolder(folderPtr);

            Marshal.FreeHGlobal(userMainPtr);
            Marshal.FreeHGlobal(adminUserPtr);
            Marshal.FreeHGlobal(adminPswPtr);
            Marshal.FreeHGlobal(domainPtr);
            Marshal.FreeHGlobal(folderPtr);

            Assert.AreEqual(result, 0);
        }

        public IntPtr GetStringIntPtr(string str)
        {
            return Marshal.StringToHGlobalUni(str);
        }
    }
}
