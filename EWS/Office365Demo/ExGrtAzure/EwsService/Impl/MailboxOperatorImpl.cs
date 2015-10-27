using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Exchange.WebServices.Data;
using EwsService.Common;
using EwsServiceInterface;

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
            MailboxPrincipalAddress = connectMailAddress;
            CurrentExchangeService = EwsProxyFactory.CreateExchangeService(argument, MailboxPrincipalAddress);
        }

        public IFolder NewFolderOperatorInstance()
        {
            return new FolderOperatorImpl(CurrentExchangeService);
        }
    }
}