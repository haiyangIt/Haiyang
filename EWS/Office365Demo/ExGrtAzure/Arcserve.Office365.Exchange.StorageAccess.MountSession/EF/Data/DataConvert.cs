using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arcserve.Office365.Exchange.Data.Increment;
using Microsoft.Exchange.WebServices.Data;

namespace Arcserve.Office365.Exchange.StorageAccess.MountSession.EF.Data
{
    public class DataConvert
    {
        internal IEnumerable<ItemSyncModel> Convert(IEnumerable<Item> items)
        {
            throw new NotImplementedException();
        }

        internal IEnumerable<MailboxSyncModel> Convert(IEnumerable<IMailboxDataSync> mailboxes)
        {
            throw new NotImplementedException();
        }

        internal FolderSyncModel Convert(Folder folder)
        {
            throw new NotImplementedException();
        }
    }
}
