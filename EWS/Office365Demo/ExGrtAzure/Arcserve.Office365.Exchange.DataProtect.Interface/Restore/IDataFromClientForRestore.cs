using Arcserve.Office365.Exchange.Thread;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arcserve.Office365.Exchange.Data.Increment;

namespace Arcserve.Office365.Exchange.DataProtect.Interface.Restore
{
    public interface IDataFromClientForRestore<ProgressType> : ITaskSyncContext<ProgressType>
    {
        IEnumerable<IMailboxDataSync> GetAllMailboxFromPlan(Func<IEnumerable<IMailboxDataSync>> funcGetAllMailboxFromCatalog);
        IEnumerable<IFolderDataSync> GetAllFoldersFromPlan(IMailboxDataSync mailboxInfo, Func<IMailboxDataSync, IEnumerable<IFolderDataSync>> funcGetFolderFromCatalog);
        ItemList GetFolderItems(IFolderDataSync folder, int offset, int pageCount, Func<IFolderDataSync, int, int, ItemList> getFolderItemsFromCatalog);
    }
}
