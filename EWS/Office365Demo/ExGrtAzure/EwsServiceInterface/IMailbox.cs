
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EwsServiceInterface
{
    /// <summary>
    /// An interface to get mailbox information from EWS.
    /// </summary>
    internal interface IMailbox
    {
        void ConnectMailbox(EwsServiceArgument argument, string connectMailAddress);
        
        string MailboxPrincipalAddress { get; }
        
        ExchangeService CurrentExchangeService { get; }
    }
}
