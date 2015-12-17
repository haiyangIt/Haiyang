using DataProtectInterface;
using EwsDataInterface;
using EwsFrame;
using SqlDbImpl.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDbImpl
{
    public class DataConvert : IDataConvert
    {
        public string OrganizationName { get; set; }

        public DateTime StartTime
        {
            get; set;
        }

        private IServiceContext Context
        {
            get
            {
                return CatalogFactory.Instance.GetServiceContext();
            }
        }
        public IOrganizationData Convert(string mailboxAddress)
        {
            throw new NotImplementedException();
        }
        public ICatalogJob Convert(ICatalogJob catalogJobInfo)
        {
            CatalogInfoModel model = new CatalogInfoModel()
            {
                CatalogJobName = catalogJobInfo.CatalogJobName,
                Organization = catalogJobInfo.Organization,
                StartTime = this.StartTime
            };
            return model;
        }
        public IMailboxData Convert(IMailboxData mailbox)
        {
            MailboxModel model = new MailboxModel();
            model.DisplayName = mailbox.DisplayName;
            model.MailAddress = mailbox.MailAddress;
            model.RootFolderId = mailbox.RootFolderId;
            model.StartTime = StartTime;
            model.ChildFolderCount = 0;
            return model;
        }

        public IItemData Convert(Microsoft.Exchange.WebServices.Data.Item item)
        {
            ItemModel model = new ItemModel()
            {
                StartTime = this.StartTime,
                ItemId = item.Id.UniqueId,
                ItemClass = item.ItemClass,
                ParentFolderId = item.ParentFolderId.UniqueId,
                DisplayName = item.Subject,
                CreateTime = item.DateTimeCreated,
                Data = item,
                Size = item.Size
            };
            return model;
        }

        public IFolderData Convert(Microsoft.Exchange.WebServices.Data.Folder folder)
        {
            FolderModel model = new FolderModel()
            {
                StartTime = this.StartTime,
                FolderId = folder.Id.UniqueId,
                ParentFolderId = folder.ParentFolderId.UniqueId,
                DisplayName = folder.DisplayName,
                FolderType = folder.FolderClass,
                ChildItemCount = 0,
                ChildFolderCount = 0,
                MailboxAddress = Context.CurrentMailbox
                
            };
            return model;
        }

    }
}
