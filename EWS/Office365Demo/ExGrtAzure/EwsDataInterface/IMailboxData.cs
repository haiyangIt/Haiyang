using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EwsDataInterface
{
    public interface IMailboxData : IItemBase
    {
        string Location { get; set; }
        string MailAddress { get; }
        string RootFolderId { get; set; }

        int ChildFolderCount { get; set; }

        IMailboxData Clone();
    }
}
