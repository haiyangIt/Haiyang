using Arcserve.Office365.Exchange.Data.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.DataProtect.Tool.Data
{
    [Serializable]
    public class MailboxList : List<Mailbox>
    {
        public MailboxList(int capacity) : base(capacity) { }

        public MailboxList() : base() { }
    }
}
