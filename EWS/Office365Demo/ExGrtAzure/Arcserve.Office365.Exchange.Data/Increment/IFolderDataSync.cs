using Arcserve.Office365.Exchange.Data.Mail;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Data.Increment
{
    public interface IFolderDataSync : IFolderData, IDataSync
    {
        string MailboxId { get; set; }

        FolderId FolderIdInExchange { get; set; }
    }
}
