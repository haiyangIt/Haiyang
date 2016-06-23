using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Data.Mail
{
    [Serializable]
    public class Mailbox
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string MailAddress { get; set; }
        public string Id { get; set; }

        public Mailbox() { }

        public Mailbox(string displayName, string mailAddress)
        {
            DisplayName = displayName;
            MailAddress = mailAddress;
        }
    }
}
