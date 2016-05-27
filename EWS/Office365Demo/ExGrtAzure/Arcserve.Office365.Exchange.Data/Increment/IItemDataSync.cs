using Arcserve.Office365.Exchange.Data.Mail;
using Arcserve.Office365.Exchange.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Data.Increment
{
    public interface IItemDataSync : IItemData, IDataSync
    {
        bool? IsRead { get; set; }
        string MailboxAddress { get; set; }
    }

    
}
