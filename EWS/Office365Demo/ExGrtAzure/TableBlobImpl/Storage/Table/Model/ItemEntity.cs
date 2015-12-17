using EwsDataInterface;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TableBlobImpl.Access.Table;

namespace TableBlobImpl.Storage.Table.Model
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Its table name is organization name+"item".
    /// Its rowkeys is itemId.
    /// Its partition key is start time ticks.
    /// </remarks>
    public class ItemEntity : TableEntity, IItemData, ICatalogInfo
    {
        public ItemEntity() { }
        [IgnoreProperty()]
        public string ItemId { get; set; }
        public string Subject { get; set; }
        public DateTime? SendTime { get; set; }
        public DateTime? RecvTime { get; set; }
        public string SendName { get; set; }
        public byte HasAttachment { get; set; }
        public byte Important { get; set; }
        public byte IsRead { get; set; }
        
        public string ParentFolderId
        {
            get; set;
        }

        [IgnoreProperty()]
        public string DisplayName
        {
            get
            {
                return Subject;
            }
        }

        public DateTime? CreateTime
        {
            get; set;
        }
        [IgnoreProperty()]
        public object Data
        {
            get; set;
        }

        public string ItemClass { get; set; }
        [IgnoreProperty()]
        public string Location { get; set; }

        [IgnoreProperty()]
        public DateTime StartTime
        {
            get; set;
        }

        public int Size
        {
            get; set;
        }

        public int ActualSize
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string Id
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ItemKind ItemKind
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public static ItemEntity CreateItemEntityFromEws(Item item, DateTime catalogStartTime)
        {
            ItemEntity entity = new ItemEntity();
            entity.ItemId = item.Id.UniqueId;
            entity.Subject = item.Subject;
            EmailMessage message = item as EmailMessage;
            if (message != null)
            {
                entity.SendName = message.Sender.Name;
                entity.IsRead = (byte)(message.IsRead ? 1 : 0);
            }

            entity.ItemClass = item.ItemClass;
            entity.CreateTime = item.DateTimeCreated;
            entity.SendTime = item.DateTimeSent;
            entity.RecvTime = item.DateTimeReceived;
            entity.HasAttachment = (byte)(item.HasAttachments ? 1 : 0);
            entity.Important = (byte)item.Importance;
            entity.ParentFolderId = item.ParentFolderId.UniqueId;
            entity.Data = item;
            long ticks = DateTime.MaxValue.Ticks - catalogStartTime.Ticks;
            entity.PartitionKey = TableDataAccess.ValidateRowPartitionKey(ticks.ToString(), false);
            entity.RowKey = TableDataAccess.ValidateRowPartitionKey(entity.ItemId, false);
            
            return entity;
        }

        public static string GetItemTableName(string organizationName, string folderId, DateTime startTime)
        {
            return string.Format("{0}{1}{2}", organizationName.ToLower(), folderId, "item");
        }

        public IItemData Clone()
        {
            throw new NotImplementedException();
        }
    }

    
}