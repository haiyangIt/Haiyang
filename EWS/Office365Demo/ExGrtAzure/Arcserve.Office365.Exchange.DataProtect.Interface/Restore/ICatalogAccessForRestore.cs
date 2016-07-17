using Arcserve.Office365.Exchange.Thread;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arcserve.Office365.Exchange.Data.Increment;
using Arcserve.Office365.Exchange.Data.Query;

namespace Arcserve.Office365.Exchange.DataProtect.Interface.Restore
{
    public interface ICatalogAccessForRestore<ProgressType> : ITaskSyncContext<ProgressType>
    {
        ItemList GetFolderItemsFromCatalog(IFolderDataSync folder, int offset, int pageCount);

        IEnumerable<IMailboxDataSync> GetAllMailboxFromCatalog();
        IEnumerable<IFolderDataSync> GetFoldersFromCatalog(IMailboxDataSync mailboxInfo);

        QueryResult<IMailboxDataSync> GetMailboxesForCom(QueryCondition queryCondition, QueryPage queryPage, bool isOnlyGetCount = false);
        QueryResult<IFolderDataSync> GetFoldersForCom(Int64 id, QueryCondition queryCondition, QueryPage queryPage, bool isOnlyGetCount = false);
        QueryResult<IItemDataSync> GetItemsForCom(Int64 folderId, QueryCondition queryCondition, QueryPage queryPage, bool isOnlyGetCount = false);

        int QueryCountForCom(Int64 id, QueryCondition queryCondition);
    }
}
