using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Arcserve.Office365.Exchange.Com.Impl
{
    [Guid("D663CC04-D4BD-4C5A-BFC6-4F1896EEE856")]
    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true)]
    public class MailResultItem : ResultItem, IMailItemResult
    {
        public uint MailFlag
        {
            get; set;
        }

        public uint MailSize
        {
            get; set;
        }

        public string Receiver
        {
            get; set;
        }

        public DateTime ReceiveTime
        {
            get; set;
        }

        public string Sender
        {
            get; set;
        }

        public DateTime SentTime
        {
            get; set;
        }
    }
}
