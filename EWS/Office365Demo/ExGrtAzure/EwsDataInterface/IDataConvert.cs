using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EwsDataInterface
{
    public interface IDataConvert
    {
        DateTime StartTime { get; set; }
        string OrganizationName { get; set; }
        IOrganizationData Convert(string organizationName);
        ICatalogJob Convert(ICatalogJob catalogJobInfo);
        IMailboxData Convert(IMailboxData mailbox);
        IFolderData Convert(Folder folder, string folderMailbox);
        IItemData Convert(Item item);
    }
}
