using ExGrtAzure.EWS.DataAccess;
using ExGrtAzure.EWS.ItemOperator;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ExGrtAzure.EWS.MailboxOperator;

namespace ExGrtAzure.EWS.Catalog.Local
{
    public class DataAccessDisk : IDataAccess
    {
        public IService GetLastCatalogJob(DateTime thisJobStartTime)
        {
            throw new NotImplementedException();
        }

        public void SaveCatalogJob(IService service)
        {
            throw new NotImplementedException();
        }

        public void SaveFolder(IFolderData folder, IMailboxData mailboxData, IFolderData parentFolderData)
        {
            throw new NotImplementedException();
        }

        public void SaveItem(IItemData item, IMailboxData mailboxData, IFolderData parentFolderData)
        {
            throw new NotImplementedException();
        }

        public void SaveMailbox(IMailboxData mailboxAddress)
        {
            throw new NotImplementedException();
        }

        ICatalogJob IDataAccess.GetLastCatalogJob(DateTime thisJobStartTime)
        {
            throw new NotImplementedException();
        }
    }
}