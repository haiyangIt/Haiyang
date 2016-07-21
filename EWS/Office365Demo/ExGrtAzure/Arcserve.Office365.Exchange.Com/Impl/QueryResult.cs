using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Arcserve.Office365.Exchange.Data.Query;
using Arcserve.Office365.Exchange.Data.Increment;
using Arcserve.Office365.Exchange.Data;

namespace Arcserve.Office365.Exchange.Com.Impl
{
    [Guid("E824F6D7-E022-4885-A8CD-BF198FB3BCF5")]
    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true)]
    public class QueryResult : IQueryResult
    {
        public uint QueryCount
        {
            get; set;
        }

        public uint TotalCount
        {
            get; set;
        }

        internal IEnumerable<IResult> Results;

        internal QueryResult<IItemDataSync> Items;
        internal QueryResult<IFolderDataSync> Folders;
        internal QueryResult<IMailboxDataSync> Mailboxes;

        private void GetHLIds(Int64 id, out Int32 pId, out Int32 lId)
        {
            var temp = id;
            pId = (int)(id >> 32);
            lId = (int)(temp & 0x00000000FFFFFFFF);
        }

        public IResult GetResult(uint index)
        {
            if(Items != null)
            {
                IMailItemResult result = new MailResultItem();
                var parentId = Items.ParentId;
                
                if (index >= 0 && index < Items.Count)
                {
                    var item = Items.Items.ElementAt((int)index);
                    var id = item.UniqueId;
                    result.DisplayName = item.DisplayName;
                    int hId;
                    int lId;
                    GetHLIds(Items.ParentId, out hId, out lId);
                    result.HParentId = hId;
                    result.LParentId = lId;
                    GetHLIds(item.UniqueId, out hId, out lId);
                    result.HId = hId;
                    result.LId = lId;

                    result.MailSize = (uint)item.Size;
                    result.MailFlag = 0;
                    result.Receiver = item.Receiver;
                    result.Sender = item.Sender;
                    result.ReceiveTime = item.ReceiveTime;
                    result.SentTime = item.SendTime;
                    result.ObjType = (int)ItemTypes.EX_ITEM_GUI__Message;
                    return result;
                }
                return null;
            }
            else if(Mailboxes != null)
            {
                IMailboxResult result = new MailboxResultItem();
                var parentId = Mailboxes.ParentId;

                if (index >= 0 && index < Mailboxes.Count)
                {
                    var item = Mailboxes.Items.ElementAt((int)index);
                    var id = item.UniqueId;
                    result.DisplayName = item.DisplayName;
                    int hId;
                    int lId;
                    GetHLIds(Mailboxes.ParentId, out hId, out lId);
                    result.HParentId = hId;
                    result.LParentId = lId;
                    GetHLIds(item.UniqueId, out hId, out lId);
                    result.HId = hId;
                    result.LId = lId;
                    result.ObjType = (int)ItemTypes.EX_ITEM_GUI_MAILBOX;
                    return result;
                }
                return null;
            }
            else if (Folders != null)
            {
                IFolderResult result = new FolderResultItem();
                var parentId = Folders.ParentId;

                if (index >= 0 && index < Folders.Count)
                {
                    var item = Folders.Items.ElementAt((int)index);
                    var id = item.UniqueId;
                    result.DisplayName = ((IItemBase)item).DisplayName;
                    int hId;
                    int lId;
                    GetHLIds(Folders.ParentId, out hId, out lId);
                    result.HParentId = hId;
                    result.LParentId = lId;
                    GetHLIds(item.UniqueId, out hId, out lId);
                    result.HId = hId;
                    result.LId = lId;
                    result.ObjType = (int)ItemTypes.EX_ITEM_GUI_FOLDER;
                    return result;
                }
                return null;
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }

    internal enum ItemTypes : int
    {
        EX_ITEM_GUI_EDB = 255,
        EX_ITEM_GUI_CALENDAR = 9004,
        EX_ITEM_GUI_CONTACTS = 9005,
        EX_ITEM_GUI_DRAFT = 9006,
        EX_ITEM_GUI_JOURNAL = 9007,
        EX_ITEM_GUI_NOTES = 9008,
        EX_ITEM_GUI_TASKS = 9009,
        EX_ITEM_GUI_ROOT_PUBLIC_FOLDERS = 254,
        EX_ITEM_GUI_MAILBOX = 9013,
        EX_ITEM_GUI_DELETED_ITEMS = 9014,
        EX_ITEM_GUI_INBOX = 9015,
        EX_ITEM_GUI_OUTBOX = 9016,
        EX_ITEM_GUI_SENT_ITEMS = 9017,
        EX_ITEM_GUI_FOLDER = 9018,
        EX_ITEM_GUI__Message = 9023,
        EX_ITEM_GUI_CONTACTS_GROUP = 9024
    }
}
