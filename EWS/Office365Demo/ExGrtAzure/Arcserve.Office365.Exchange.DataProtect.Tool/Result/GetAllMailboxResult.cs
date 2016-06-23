using Arcserve.Office365.Exchange.Data.Mail;
using Arcserve.Office365.Exchange.DataProtect.Tool.Data;
using Arcserve.Office365.Exchange.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.DataProtect.Tool.Result
{
    public class GetAllMailboxResult : ResultBase
    {
        public List<Mailbox> Mailboxes { get; set; }

        public GetAllMailboxResult() : base() { }
        public GetAllMailboxResult(string errorMsg) : base(errorMsg) { }

        public GetAllMailboxResult(List<Mailbox> mailboxes)
        {
            Mailboxes = mailboxes;
            Status = ResultStatus.Success;
        }
    }
}
