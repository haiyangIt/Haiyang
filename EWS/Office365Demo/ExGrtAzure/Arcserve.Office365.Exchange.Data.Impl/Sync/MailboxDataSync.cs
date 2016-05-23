using Arcserve.Office365.Exchange.Data.Impl.Mail;
using Arcserve.Office365.Exchange.Data.Increment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Data.Impl.Sync
{
    public class MailboxDataSync : MailboxModel, IMailboxDataSync
    {
        public string ChangeKey
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string Name
        {
            get; set;
        }

        public string SyncStatus
        {
            get; set;
        }
    }
}
