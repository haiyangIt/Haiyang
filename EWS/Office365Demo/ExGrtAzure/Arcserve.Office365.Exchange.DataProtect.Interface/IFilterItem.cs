using Arcserve.Office365.Exchange.Data.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.DataProtect.Interface
{
    public interface IFilterItem
    {
        bool IsFilterMailbox(IMailboxData mailbox);
        bool IsFilterFolder(IFolderData currentFolder, IMailboxData mailbox, Stack<IFolderData> folders);
        bool IsFilterItem(IItemData item, IMailboxData mailbox, Stack<IFolderData> folders);
    }
    
    public interface IFilterItemWithMailbox : IFilterItem
    {
        List<IMailboxData> GetAllMailbox();

        int GetFolderCount();
    }
}
