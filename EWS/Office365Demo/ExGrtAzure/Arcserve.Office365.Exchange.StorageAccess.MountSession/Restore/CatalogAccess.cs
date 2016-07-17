using Arcserve.Office365.Exchange.DataProtect.Interface.Restore;
using Arcserve.Office365.Exchange.Thread;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arcserve.Office365.Exchange.Data.Increment;
using System.Threading;
using Arcserve.Office365.Exchange.StorageAccess.MountSession.EF;
using Arcserve.Office365.Exchange.Data.Query;
using Arcserve.Office365.Exchange.StorageAccess.MountSession.EF.SqLite;
using Arcserve.Office365.Exchange.Data;

namespace Arcserve.Office365.Exchange.StorageAccess.MountSession.Restore
{
    public class CatalogAccess : ICatalogAccessForRestore<IJobProgress>, IDisposable
    {
        public CancellationToken CancelToken
        {
            get; set;
        }

        public IJobProgress Progress
        {
            get; set;
        }

        public TaskScheduler Scheduler
        {
            get; set;
        }

        public CatalogAccess(string catalogFile)
        {
            _queryContext = CatalogDbContextBase.NewCatalogContext(catalogFile);
        }

        public CatalogDbContextBase _queryContext = null;

        public IEnumerable<IMailboxDataSync> GetAllMailboxFromCatalog()
        {
            return _queryContext.Mailboxes;
        }

        public ItemList GetFolderItemsFromCatalog(IFolderDataSync folder, int offset, int pageCount)
        {
            var result = (from s in _queryContext.Items where s.ParentFolderId == folder.FolderId orderby s.CreateTime descending select s).Skip(offset).Take(pageCount);

            return new ItemList()
            {
                Items = result,
                MoreAvailable = (offset + pageCount) < folder.ChildItemCount,
                NextOffset = offset + pageCount
            };
        }

        public IEnumerable<IFolderDataSync> GetFoldersFromCatalog(IMailboxDataSync mailboxInfo)
        {
            return from s in _queryContext.Folders where s.MailboxId == mailboxInfo.Id select s;
        }

        public void InitTaskSyncContext(ITaskSyncContext<IJobProgress> mainContext)
        {
            this.CloneSyncContext(mainContext);
        }

        public void Dispose()
        {
            if (_queryContext != null)
            {
                _queryContext.Dispose();
                _queryContext = null;
            }
        }

        public QueryResult<IMailboxDataSync> GetMailboxesForCom(QueryCondition queryCondition, QueryPage queryPage, bool isOnlyGetCount = false)
        {
            // todo order by
            IQueryable<IMailboxDataSync> result = from m in _queryContext.Mailboxes select m;
            if (queryCondition != null && queryCondition.SortFields.Count > 0)
            {
                foreach (var orderCondition in queryCondition.SortFields)
                {

                    if (orderCondition.isDescend)
                    {
                        switch (orderCondition.FieldName.ToLower())
                        {
                            case "displayname":
                                result = result.OrderByDescending(m => m.DisplayName);
                                break;
                        }
                    }
                    else
                    {
                        switch (orderCondition.FieldName.ToLower())
                        {
                            case "displayname":
                                result = result.OrderBy(m => m.DisplayName);
                                break;
                        }
                    }
                }
            }

            if (queryCondition != null && queryCondition.SearchField != null)
            {
                if (!string.IsNullOrEmpty(queryCondition.SearchField.SearchValue))
                {
                    switch (queryCondition.SearchField.FieldName.ToLower())
                    {
                        case "displayname":
                            result = from m in result where m.DisplayName.Contains(queryCondition.SearchField.SearchValue) select m;
                            break;
                        default:
                            throw new NotSupportedException(string.Format("not support {0} [{1}] search", queryCondition.SearchField.FieldName, queryCondition.SearchField.SearchValue));
                    }
                }
            }

            var count = result.Count();
            if (!isOnlyGetCount)
            {
                IEnumerable<IMailboxDataSync> items;
                if (queryPage != null)
                {
                    items = result.Skip(queryPage.StartIndex).Take(queryPage.PageCount);
                    return new QueryResult<IMailboxDataSync>()
                    {
                        TotalCount = count,
                        Condition = queryCondition,
                        PageInfo = queryPage,
                        Items = items,
                        ParentId = CatalogDbInitialize.MailStartIndex
                    };
                }
                else
                {
                    return new QueryResult<IMailboxDataSync>()
                    {
                        TotalCount = count,
                        Condition = queryCondition,
                        PageInfo = queryPage,
                        Items = result,
                        ParentId = CatalogDbInitialize.MailStartIndex
                    };
                }
            }
            else
            {
                return new QueryResult<IMailboxDataSync>()
                {
                    TotalCount = count,
                    Condition = queryCondition,
                    PageInfo = queryPage,
                    ParentId = CatalogDbInitialize.MailStartIndex
                };
            }
        }

        public int QueryCountForCom(Int64 id, QueryCondition queryCondition)
        {
            var idType = CatalogSyncDbContext.GetIdType(id);

            switch (idType)
            {
                case IdType.Root:
                    {
                        var result = GetMailboxesForCom(queryCondition, null, true);
                        return result.TotalCount;
                    }
                case IdType.Mailbox:
                    {
                        var result = GetFoldersForCom(id, queryCondition, null, true);
                        return result.TotalCount;
                    }
                case IdType.Folder:
                    {
                        var result = GetFoldersForCom(id, queryCondition, null, true);
                        return result.TotalCount;
                    }
                default:
                    throw new NotSupportedException();
            }
        }

        public QueryResult<IFolderDataSync> GetFoldersForCom(Int64 id, QueryCondition queryCondition, QueryPage queryPage, bool isOnlyGetCount = false)
        {
            IQueryable<IFolderDataSync> result = from m in _queryContext.Folders select m;

            if (CatalogSyncDbContext.IsInFolder(id))
            {
                var parentFolder = (from m in _queryContext.Folders where m.UniqueId == id select m).FirstOrDefault();
                if (parentFolder == null)
                {
                    return new QueryResult<IFolderDataSync>()
                    {
                        TotalCount = 0,
                        Condition = queryCondition,
                        PageInfo = queryPage,
                        Items = new List<IFolderDataSync>(0),
                        ParentId = id
                    };
                }
                else
                {
                    result = from m in result where m.ParentFolderId == parentFolder.FolderId select m;
                }
            }
            else
            {
                var mailbox = (from m in _queryContext.Mailboxes where m.UniqueId == id select m).FirstOrDefault();
                if (mailbox == null)
                {
                    return new QueryResult<IFolderDataSync>()
                    {
                        TotalCount = 0,
                        Condition = queryCondition,
                        PageInfo = queryPage,
                        Items = new List<IFolderDataSync>(0),
                        ParentId = id
                    };
                }
                else
                {
                    result = from m in result where m.ParentFolderId == mailbox.RootFolderId select m;
                }
            }

            if (queryCondition != null && queryCondition.SortFields.Count > 0)
            {
                foreach (var orderCondition in queryCondition.SortFields)
                {

                    if (orderCondition.isDescend)
                    {
                        switch (orderCondition.FieldName.ToLower())
                        {
                            case "displayname":
                                result = result.OrderByDescending(m => ((IItemBase)m).DisplayName);
                                break;
                        }
                    }
                    else
                    {
                        switch (orderCondition.FieldName.ToLower())
                        {
                            case "displayname":
                                result = result.OrderBy(m => ((IItemBase)m).DisplayName);
                                break;
                        }
                    }
                }
            }

            if (queryCondition != null && queryCondition.SearchField != null)
            {
                if (!string.IsNullOrEmpty(queryCondition.SearchField.SearchValue))
                {
                    switch (queryCondition.SearchField.FieldName.ToLower())
                    {
                        case "displayname":
                            result = from m in result where ((IItemBase)m).DisplayName.Contains(queryCondition.SearchField.SearchValue) select m;
                            break;
                        default:
                            throw new NotSupportedException(string.Format("not support {0} [{1}] search", queryCondition.SearchField.FieldName, queryCondition.SearchField.SearchValue));
                    }
                }
            }

            var totalCount = result.Count();

            if (!isOnlyGetCount)
            {
                if (queryPage != null)
                {
                    return new QueryResult<IFolderDataSync>()
                    {
                        TotalCount = totalCount,
                        Condition = queryCondition,
                        PageInfo = queryPage,
                        Items = result.Skip(queryPage.StartIndex).Take(queryPage.PageCount),
                        ParentId = id
                    };
                }
                else
                {
                    return new QueryResult<IFolderDataSync>()
                    {
                        TotalCount = totalCount,
                        Condition = queryCondition,
                        PageInfo = queryPage,
                        Items = result,
                        ParentId = id
                    };
                }
            }
            else
            {
                return new QueryResult<IFolderDataSync>()
                {
                    TotalCount = totalCount,
                    Condition = queryCondition,
                    PageInfo = queryPage,
                    ParentId = id
                };
            }
        }

        public QueryResult<IItemDataSync> GetItemsForCom(Int64 folderId, QueryCondition queryCondition, QueryPage queryPage, bool isOnlyGetCount = false)
        {
            var folder = (from m in _queryContext.Folders where m.UniqueId == folderId select m).FirstOrDefault();
            if (folder == null)
            {
                return new QueryResult<IItemDataSync>()
                {
                    TotalCount = 0,
                    Condition = queryCondition,
                    PageInfo = queryPage,
                    Items = new List<IItemDataSync>(0),
                    ParentId = folderId
                };
            }

            var result = from m in _queryContext.Items where m.ParentFolderId == folder.ParentFolderId select m;
            if (queryCondition != null && queryCondition.SortFields.Count > 0)
            {
                foreach (var orderCondition in queryCondition.SortFields)
                {

                    if (orderCondition.isDescend)
                    {
                        switch (orderCondition.FieldName.ToLower())
                        {
                            case "displayname":
                                result = result.OrderByDescending(m => m.DisplayName);
                                break;
                        }
                    }
                    else
                    {
                        switch (orderCondition.FieldName.ToLower())
                        {
                            case "displayname":
                                result = result.OrderBy(m => m.DisplayName);
                                break;
                        }
                    }
                }
            }

            if (queryCondition != null && queryCondition.SearchField != null)
            {
                if (!string.IsNullOrEmpty(queryCondition.SearchField.SearchValue))
                {
                    switch (queryCondition.SearchField.FieldName.ToLower())
                    {
                        case "displayname":
                            result = from m in result where ((IItemBase)m).DisplayName.Contains(queryCondition.SearchField.SearchValue) select m;
                            break;
                        default:
                            throw new NotSupportedException(string.Format("not support {0} [{1}] search", queryCondition.SearchField.FieldName, queryCondition.SearchField.SearchValue));
                    }
                }
            }

            var totalCount = result.Count();

            if (!isOnlyGetCount)
            {
                if (queryPage != null)
                {
                    return new QueryResult<IItemDataSync>()
                    {
                        TotalCount = totalCount,
                        Condition = queryCondition,
                        PageInfo = queryPage,
                        Items = result.Skip(queryPage.StartIndex).Take(queryPage.PageCount),
                        ParentId = folderId
                    };
                }
                else
                {
                    return new QueryResult<IItemDataSync>()
                    {
                        TotalCount = totalCount,
                        Condition = queryCondition,
                        PageInfo = queryPage,
                        Items = result,
                        ParentId = folderId
                    };
                }
            }
            else
            {
                return new QueryResult<IItemDataSync>()
                {
                    TotalCount = totalCount,
                    Condition = queryCondition,
                    PageInfo = queryPage,
                    ParentId = CatalogDbInitialize.MailStartIndex
                };
            }
        }
    }
}
