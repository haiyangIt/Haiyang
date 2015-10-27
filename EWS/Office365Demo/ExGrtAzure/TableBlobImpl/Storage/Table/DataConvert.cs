using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Exchange.WebServices.Data;
using EwsDataInterface;
using TableBlobImpl.Storage.Table.Model;

namespace TableBlobImpl.Storage.Table
{
    public class DataConvert : IDataConvert, ICatalogInfo
    {
        public string OrganizationName { get; set; }

        public DateTime StartTime { get; set; }

        public ICatalogJob Convert(ICatalogJob catalogJobInfo)
        {
            throw new NotImplementedException();
        }

        public IOrganizationData Convert(string mailboxAddress)
        {
            throw new NotImplementedException();
        }

        public IMailboxData Convert(IMailboxData mailboxData)
        {
            return MailboxEntity.CreateMailboxEntityFromEws(OrganizationName, mailboxData.DisplayName, mailboxData.MailAddress, StartTime);
        }

        public IItemData Convert(Item item)
        {
            return ItemEntity.CreateItemEntityFromEws(item, StartTime);
        }

        public IFolderData Convert(Folder folder)
        {
            return FolderEntity.CreateFolderEntityFromEws(folder, StartTime);
        }
        
    }
}