using EwsDataInterface;
using System;

namespace DataProtectInterface.Event
{
    public class CatalogProgressArgs : EventArgs
    {
        private IMailboxData Mailbox;
        private Process MailboxProcess;
        private CatalogProgressType Type;

        public CatalogProgressArgs(CatalogProgressType type, Process mailboxProcess, IMailboxData mailbox)
        {
            this.Type = type;
            this.MailboxProcess = mailboxProcess;
            this.Mailbox = mailbox;
        }
    }
}