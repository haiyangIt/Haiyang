using Arcserve.Office365.Exchange.Data.Mail;
using System;

namespace Arcserve.Office365.Exchange.Data.Event
{
    public class CatalogMailboxArgs : EventArgs
    {
        private IMailboxData Mailbox;
        private CatalogMailboxProgressType Type;

        public CatalogMailboxArgs(CatalogMailboxProgressType type, IMailboxData mailbox)
        {
            this.Type = type;
            this.Mailbox = mailbox;
        }
    }
}