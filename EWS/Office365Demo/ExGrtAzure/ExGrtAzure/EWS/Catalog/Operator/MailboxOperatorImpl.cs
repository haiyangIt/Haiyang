using ExGrtAzure.EWS.MailboxOperator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ExGrtAzure.EWS.FolderOperator;
using ExGrtAzure.EWS.ItemOperator;
using Microsoft.Exchange.WebServices.Data;

namespace ExGrtAzure.EWS.Catalog.Operator
{
    public class MailboxOperatorImpl : IMailbox
    {
        private readonly string _adminUserName;
        private readonly string _adminPassword;
        private readonly string _organization;

        public MailboxOperatorImpl(string mailboxAddress, string adminUserName, string adminPassword, string organization)
        {
            MailboxPrincipalAddress = mailboxAddress;
            _adminUserName = adminUserName;
            _adminPassword = adminPassword;
            _organization = organization;
        }

        public ExchangeService CurrentExchangeService
        {
            get; private set;
        }

        public string MailboxPrincipalAddress
        {
            get; private set;
        }

        public void ConnectMailbox()
        {
            var argument = ServiceContext.CurrentContext.GetExchangeServiceArgument();
            CurrentExchangeService = EwsProxyFactory.CreateExchangeService(argument, MailboxPrincipalAddress);
        }

        public IFolder NewFolderOperatorInstance()
        {
            return new FolderOperatorImpl(CurrentExchangeService, _organization);
        }
    }
}