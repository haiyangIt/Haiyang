using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arcserve.Office365.Exchange.Data.Increment;
using Microsoft.Exchange.WebServices.Data;
using Arcserve.Office365.Exchange.DataProtect.Interface.Backup.Increment;
using Arcserve.Office365.Exchange.Data.Mail;

namespace Arcserve.Office365.Exchange.StorageAccess.MountSession.EF.Data
{
    public class DataConvert : IDataConvert
    {
        public IMailboxDataSync Convert(IMailboxDataSync mailbox)
        {
            if (mailbox is MailboxSyncModel)
            {
                return mailbox;
            }
            else
            {
                var result = new MailboxSyncModel()
                {
                    Id = mailbox.Id,
                    DisplayName = mailbox.DisplayName,
                    MailAddress = mailbox.MailAddress.ToLower(),
                    SyncStatus = mailbox.SyncStatus.ConvertNullToEmpty(),
                    ChildFolderCount = mailbox.ChildFolderCount,
                    Name = mailbox.Name
                };
                return result;
            }
        }

        public IEnumerable<IItemDataSync> Convert(IEnumerable<Item> items, IFolderDataSync parentFolder)
        {
            List<IItemDataSync> result = new List<IItemDataSync>(items.Count());
            foreach (var item in items)
            {
                result.Add(Convert(item, parentFolder));
            }
            return result;
        }

        public IItemDataSync Convert(Item item, IFolderDataSync parentFolder)
        {
            var itemClass = item.ItemClass.GetItemClass();
            var result = new ItemSyncModel()
            {
                ItemId = item.Id.UniqueId,
                ChangeKey = item.Id.ChangeKey,
                CreateTime = item.DateTimeCreated,
                DisplayName = item.Subject.ConvertNullToEmpty(),
                Size = item.Size,
                ItemClass = item.ItemClass,
                ParentFolderId = item.ParentFolderId.UniqueId,
                Location = parentFolder.Location,
                MailboxAddress = parentFolder.MailboxAddress,
                Data = item,
                SyncStatus = string.Empty,
                IsRead = true
            };
            result.Location = result.GetFileName(parentFolder.Location.GetFolderDisplays());
            if (itemClass == ItemClass.Message)
            {
                result.IsRead = ((EmailMessage)item).IsRead;
            }
            return result;

        }

        public IFolderDataSync Convert(Folder folder, IMailboxDataSync mailboxDataSync)
        {
            var result = new FolderSyncModel()
            {
                FolderId = folder.Id.UniqueId,
                ChangeKey = folder.Id.ChangeKey,
                DisplayName = folder.DisplayName,
                FolderType = folder.FolderClass,
                MailboxAddress = mailboxDataSync.MailAddress,
                MailboxId = mailboxDataSync.Id,
                ParentFolderId = folder.ParentFolderId.UniqueId,
                ChildItemCount = folder.TotalCount,
                ChildFolderCount = folder.ChildFolderCount,
                Location = string.Empty,
                SyncStatus = string.Empty,
                FolderIdInExchange = folder.Id 
            };

            return result;
        }


        public IEnumerable<IMailboxDataSync> Convert(IEnumerable<IMailboxDataSync> mailboxes)
        {
            List<IMailboxDataSync> result = new List<IMailboxDataSync>(mailboxes.Count());
            foreach (var m in mailboxes)
            {
                result.Add(Convert(m));
            }
            return result;
        }

        internal IEnumerable<MailboxSyncModel> ConvertToMailboxModel(IEnumerable<IMailboxDataSync> mailboxes)
        {
            List<MailboxSyncModel> result = new List<MailboxSyncModel>(mailboxes.Count());
            foreach (var m in mailboxes)
            {
                result.Add((MailboxSyncModel)Convert(m));
            }
            return result;
        }

        internal List<ItemSyncModel> ConvertToItemModel(IEnumerable<IItemDataSync> items)
        {
            List<ItemSyncModel> result = new List<ItemSyncModel>(items.Count());
            foreach(var item in items)
            {
                result.Add(item as ItemSyncModel);
            }
            return result;
        }

        internal IItemDataSync Convert(IItemDataSync item)
        {
            if (item is ItemSyncModel)
            {
                return item;
            }
            else
            {
                var result = new ItemSyncModel()
                {
                    ItemId = item.ItemId,
                    ChangeKey = item.ChangeKey,
                    CreateTime = item.CreateTime,
                    DisplayName = item.DisplayName.ConvertNullToEmpty(),
                    Size = item.Size,
                    ItemClass = item.ItemClass,
                    ParentFolderId = item.ParentFolderId,
                    Location = item.Location,
                    MailboxAddress = item.MailboxAddress,
                    Data = item.Data,
                    SyncStatus = item.SyncStatus.ConvertNullToEmpty(),
                    IsRead = item.IsRead,
                    ActualSize = item.ActualSize
                };

                return result;
            }
        }
        
    }

    public static class StringNullConvert
    {
        public static string ConvertNullToEmpty(this string str)
        {
            return str != null ? str : String.Empty;
        }
    }
}
