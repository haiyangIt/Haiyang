
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.EwsApi
{
    /// <summary>
    /// An interface to get mailbox information from EWS.
    /// </summary>
    public interface IMailbox
    {
        void ConnectMailbox(EwsServiceArgument argument, string connectMailAddress);
        
        string MailboxPrincipalAddress { get; }
        
        ExchangeService CurrentExchangeService { get; }
    }
}
