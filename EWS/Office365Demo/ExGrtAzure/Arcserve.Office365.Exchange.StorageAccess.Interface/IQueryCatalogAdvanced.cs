using Arcserve.Office365.Exchange.Data;
using Arcserve.Office365.Exchange.Data.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.StorageAccess.Interface
{
    public interface IQueryCatalogAdvanced : IQueryCatalogDataAccess
    {
        List<ICatalogJob> QueryCatalogJobByCondition(QueryCondition condition);
        List<IMailboxData> QueryAllMailboxByCondition(QueryCondition condition);
        List<IFolderData> QueryFolderByCondition(QueryCondition condition);
        List<IItemData> QueryItemByCondition(QueryCondition condition);
    }

    public class QueryCondition
    {
        public object Condition { get; set; }
    }
}
