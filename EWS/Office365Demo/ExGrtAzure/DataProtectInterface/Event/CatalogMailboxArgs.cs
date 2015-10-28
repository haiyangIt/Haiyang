using EwsDataInterface;
using System;

namespace DataProtectInterface.Event
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