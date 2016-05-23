using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arcserve.Office365.Exchange.Data.Increment;
using Microsoft.Exchange.WebServices.Data;
using Arcserve.Office365.Exchange.Util;

namespace Arcserve.Office365.Exchange.DataProtect.Interface.Backup.Increment
{
    public interface IDataConvert
    {
        IFolderDataSync Convert(Folder folder, IMailboxDataSync mailboxDataSync);
        IEnumerable<IItemDataSync> Convert(IEnumerable<Item> items, IFolderDataSync parentFolder);
        IItemDataSync Convert(Item item, IFolderDataSync parentFolder);
        IMailboxDataSync Convert(IMailboxDataSync mailbox);
        IEnumerable<IMailboxDataSync> Convert(IEnumerable<IMailboxDataSync> mailboxes);
    }
}
