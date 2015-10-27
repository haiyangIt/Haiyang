using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EwsDataInterface
{
    public interface IMailboxData
    {
        string Location { get; set; }
        string DisplayName { get; }
        string MailAddress { get; }
        string RootFolderId { get; }
    }
}
