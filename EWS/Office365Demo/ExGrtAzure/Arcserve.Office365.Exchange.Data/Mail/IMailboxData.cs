using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Data.Mail
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
