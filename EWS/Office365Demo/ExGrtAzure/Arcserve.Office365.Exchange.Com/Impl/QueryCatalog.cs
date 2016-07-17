using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Arcserve.Office365.Exchange.StorageAccess.MountSession.Restore;
using Arcserve.Office365.Exchange.Data.Query;
using Arcserve.Office365.Exchange.Data.Increment;

namespace Arcserve.Office365.Exchange.Com.Impl
{
    [Guid("7576DC87-C649-4A81-8892-DDA8DB81AB1C")]
    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true)]
    public class QueryCatalog : IQueryCatalog
    {
        private string _catalogFilePath;
        public IQueryResult Query(IQueryCondition query, int hParentId, int lParentId, uint startIndex, uint pageCount)
        {
            using (CatalogAccess catalogAccess = new CatalogAccess(_catalogFilePath))
            {
                Data.Query.QueryCondition queryCondition;
                QueryPage queryPage;
                Int64 id = (Int64)(hParentId << 32) | lParentId;
                ParseQueryCondition(query, (int)startIndex, (int)pageCount, out queryCondition, out queryPage);
                switch (query.ContentFilter)
                {
                    case ContentFilter.CONTENT_ALL:
                        var allMailbox = catalogAccess.GetMailboxesForCom(queryCondition, queryPage);
                        return ConvertMailboxResult(allMailbox);
                        break;
                    case ContentFilter.CONTENT_FOLDER:
                        var allFolder = catalogAccess.GetFoldersForCom(id, queryCondition, queryPage);
                        return ConvertFolderResult(allFolder);
                        break;
                    case ContentFilter.CONTENT_MAIL:
                        var allMails = catalogAccess.GetItemsForCom(id, queryCondition, queryPage);
                        return ConvertMailsResult(allMails);
                        break;
                    default:
                        throw new NotSupportedException("");
                }
                throw new NotImplementedException();
            }
        }

        private IQueryResult GetZeroCountResult<T>(QueryResult<T> allItems)
        {
            QueryResult result = new QueryResult();
            result.QueryCount = 0;
            result.TotalCount = 0;
            result.Results = new List<IResult>(0);
            return result;
        }
        

        private IQueryResult ConvertMailsResult(QueryResult<IItemDataSync> allMails)
        {
            if (allMails == null || allMails.TotalCount == 0)
                return GetZeroCountResult(allMails);

            QueryResult result = new QueryResult();
            result.QueryCount = (uint)allMails.Count;
            result.TotalCount = (uint)allMails.TotalCount;
            result.Items = allMails;
            return result;
        }

        private IQueryResult ConvertFolderResult(QueryResult<IFolderDataSync> allFolder)
        {
            if (allFolder == null || allFolder.TotalCount == 0)
                return GetZeroCountResult(allFolder);

            QueryResult result = new QueryResult();
            result.QueryCount = (uint)allFolder.Count;
            result.TotalCount = (uint)allFolder.TotalCount;
            result.Folders = allFolder;
            return result;
        }

        private IQueryResult ConvertMailboxResult(QueryResult<IMailboxDataSync> allMailbox)
        {
            if (allMailbox == null || allMailbox.TotalCount == 0)
                return GetZeroCountResult(allMailbox);

            QueryResult result = new QueryResult();
            result.QueryCount = (uint)allMailbox.Count;
            result.TotalCount = (uint)allMailbox.TotalCount;
            result.Mailboxes = allMailbox;
            return result;
        }

        

        private void ParseQueryCondition(IQueryCondition query, int startIndex, int pageCount, out Data.Query.QueryCondition queryCondition, out QueryPage queryPage)
        {
            queryCondition = new Data.Query.QueryCondition();
            queryCondition.SortFields = new List<OrderCondition>();
            if (!string.IsNullOrEmpty(query.SortField))
            {
                if (query.SortField == "subject")
                {
                    var sortField = new OrderCondition();
                    sortField.FieldName = "displayName";
                    sortField.isDescend = false;
                    queryCondition.SortFields = new List<OrderCondition>();
                    queryCondition.SortFields.Add(sortField);
                }
            }

            if (!string.IsNullOrEmpty(query.SearchString))
            {
                
                var search = new SearchCondition();
                search.FieldName = "displayName";
                search.SearchValue = query.SearchString.Trim('*');
                queryCondition.SearchField = search;
            }

            queryPage = new QueryPage();
            queryPage.PageCount = (int)pageCount;
            queryPage.StartIndex = (int)startIndex;
        }


        public int QueryCount(IQueryCondition query, int hParentId, int lParentId)
        {
            using (CatalogAccess catalogAccess = new CatalogAccess(_catalogFilePath))
            {
                Data.Query.QueryCondition queryCondition;
                QueryPage queryPage;
                Int64 id = (Int64)(hParentId << 32) | lParentId;
                ParseQueryCondition(query, QueryPage.GetTotalStartIndex, QueryPage.GetTotalPageCount, out queryCondition, out queryPage);
                return catalogAccess.QueryCountForCom(id, queryCondition);
            }
        }

        public void SetCatalogFile(string catalogFilePath)
        {
            _catalogFilePath = catalogFilePath;
        }
    }
}
