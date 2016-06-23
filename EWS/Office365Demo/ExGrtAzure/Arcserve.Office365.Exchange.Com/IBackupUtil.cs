using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Arcserve.Office365.Exchange.Data.Mail;

namespace Arcserve.Office365.Exchange.DataProtect.Tool.Com
{
    [Guid("A4CAABEA-1714-41C4-BCE6-1D1EAFDFC5C5")]
    public interface IBackupUtil
    {
        Mailbox[] GetAllMailbox([MarshalAs(UnmanagedType.LPWStr)]string adminUserName, [MarshalAs(UnmanagedType.LPWStr)]string adminPassword);
    }

    [ClassInterface(ClassInterfaceType.None)]
    [Guid("A69BCA0F-A023-4A19-8E2B-194E43364BCC")]
    public class BackupUtil : IBackupUtil
    {
        public Mailbox[] GetAllMailbox([MarshalAs(UnmanagedType.LPWStr)]string adminUserName, [MarshalAs(UnmanagedType.LPWStr)]string adminPassword)
        {
            var result = EwsApi.Interface.EwsServiceExtension.GetAllMailbox(adminUserName, adminPassword, null);
            return result.ToArray();
        }
    }
}
