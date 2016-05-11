using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Exchange.WebServices.Data;
using EwsService.Common;
using EwsServiceInterface;
using EwsFrame;

namespace EwsService.Impl
{
    public class MailboxOperatorImpl : IMailbox
    {

        public MailboxOperatorImpl()
        {
        }
        
        public ExchangeService CurrentExchangeService
        {
            get; private set;
        }

        public string MailboxPrincipalAddress
        {
            get; private set;
        }

        public void ConnectMailbox(EwsServiceArgument argument, string connectMailAddress)
        {
            argument.SetConnectMailbox(connectMailAddress);
            MailboxPrincipalAddress = connectMailAddress;
            CurrentExchangeService = EwsProxyFactory.CreateExchangeService(argument, MailboxPrincipalAddress);
        }

        public IFolder NewFolderOperatorInstance()
        {
            return new FolderOperatorImpl(CurrentExchangeService);
        }

        class ExchangeServiceObserver
        {
            public ExchangeService CurrentExchangeService { get; set; }
            public EwsServiceArgument Argument { get; set; }
            public string Mailbox { get; set; }


        }
    }
}