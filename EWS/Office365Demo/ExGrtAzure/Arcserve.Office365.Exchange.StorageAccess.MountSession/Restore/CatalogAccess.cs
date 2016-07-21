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
using System.Data.SQLite;
using Arcserve.Office365.Exchange.StorageAccess.MountSession.EF.Data;

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

            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.Append(" select * from mailboxsync ");
            List<SQLiteParameter> paramters = new List<SQLiteParameter>();


            if (queryCondition != null && queryCondition.SearchField != null)
            {
                if (!string.IsNullOrEmpty(queryCondition.SearchField.SearchValue))
                {
                    switch (queryCondition.SearchField.FieldName.ToLower())
                    {
                        case "displayname":
                            var paramterName = string.Format("@{0}", queryCondition.SearchField.FieldName);
                            sqlBuilder.Append(" where displayname like %").Append(paramterName).Append("%");
                            paramters.Add(new SQLiteParameter(paramterName, queryCondition.SearchField.SearchValue));
                            break;
                    }
                }
            }


            if (queryCondition != null && queryCondition.SortFields.Count > 0)
            {
                sqlBuilder.Append(" order by ");
                string[] orderbyArray = new string[queryCondition.SortFields.Count];
                int i = 0;
                foreach (var orderCondition in queryCondition.SortFields)
                {
                    switch (orderCondition.FieldName.ToLower())
                    {
                        case "displayname":
                            orderbyArray[i] = string.Format(" {0} {1} ", orderCondition.FieldName, orderCondition.isDescend ? "desc" : "asc");
                            break;
                    }
                }

                sqlBuilder.Append(string.Join(",", orderbyArray));
            }
            else
            {
                sqlBuilder.Append(" order by displayname asc ");
            }

            string countSql = string.Format(" select count(*) from ({0})", sqlBuilder);

            var count = _queryContext.Database.SqlQuery<int>(countSql, paramters.ToArray()).FirstOrDefault();

            if (!isOnlyGetCount)
            {
                IEnumerable<MailboxSyncModel> items;
                if (queryPage != null)
                {
                    string pageSql = string.Format("select * from ({0}) LIMIT {1} OFFSET {2} ", sqlBuilder, queryPage.PageCount, queryPage.StartIndex);

                    items = _queryContext.Database.SqlQuery<MailboxSyncModel>(pageSql, paramters.ToArray());
                    return Convert<MailboxSyncModel, IMailboxDataSync>(new QueryResult<MailboxSyncModel>()
                    {
                        TotalCount = count,
                        Condition = queryCondition,
                        PageInfo = queryPage,
                        Items = items,
                        ParentId = CatalogDbInitialize.MailboxStartIndex
                    });
                }
                else
                {
                    items = _queryContext.Database.SqlQuery<MailboxSyncModel>(sqlBuilder.ToString(), paramters.ToArray());
                    return Convert<MailboxSyncModel, IMailboxDataSync>(new QueryResult<MailboxSyncModel>()
                    {
                        TotalCount = count,
                        Condition = queryCondition,
                        PageInfo = queryPage,
                        Items = items,
                        ParentId = CatalogDbInitialize.MailboxStartIndex
                    });
                }
            }
            else
            {
                return Convert<MailboxSyncModel, IMailboxDataSync>(new QueryResult<MailboxSyncModel>()
                {
                    TotalCount = count,
                    Condition = queryCondition,
                    PageInfo = queryPage,
                    ParentId = CatalogDbInitialize.MailboxStartIndex
                });
            }


            //IQueryable<IMailboxDataSync> result = from m in _queryContext.Mailboxes select m;

            //if (queryCondition != null && queryCondition.SearchField != null)
            //{
            //    if (!string.IsNullOrEmpty(queryCondition.SearchField.SearchValue))
            //    {
            //        switch (queryCondition.SearchField.FieldName.ToLower())
            //        {
            //            case "displayname":
            //                result = from m in result where m.DisplayName.Contains(queryCondition.SearchField.SearchValue) select m;
            //                break;
            //            default:
            //                throw new NotSupportedException(string.Format("not support {0} [{1}] search", queryCondition.SearchField.FieldName, queryCondition.SearchField.SearchValue));
            //        }
            //    }
            //}

            //if (queryCondition != null && queryCondition.SortFields.Count > 0)
            //{
            //    foreach (var orderCondition in queryCondition.SortFields)
            //    {

            //        if (orderCondition.isDescend)
            //        {
            //            switch (orderCondition.FieldName.ToLower())
            //            {
            //                case "displayname":
            //                    result = result.OrderByDescending(m => m.DisplayName);
            //                    break;
            //            }
            //        }
            //        else
            //        {
            //            switch (orderCondition.FieldName.ToLower())
            //            {
            //                case "displayname":
            //                    result = result.OrderBy(m => m.DisplayName);
            //                    break;
            //            }
            //        }
            //    }
            //}



            //var count = result.Count();
            //if (!isOnlyGetCount)
            //{
            //    IEnumerable<IMailboxDataSync> items;
            //    if (queryPage != null)
            //    {
            //        items = result.Skip(queryPage.StartIndex).Take(queryPage.PageCount);
            //        return new QueryResult<IMailboxDataSync>()
            //        {
            //            TotalCount = count,
            //            Condition = queryCondition,
            //            PageInfo = queryPage,
            //            Items = items,
            //            ParentId = CatalogDbInitialize.MailboxStartIndex
            //        };
            //    }
            //    else
            //    {
            //        return new QueryResult<IMailboxDataSync>()
            //        {
            //            TotalCount = count,
            //            Condition = queryCondition,
            //            PageInfo = queryPage,
            //            Items = result,
            //            ParentId = CatalogDbInitialize.MailboxStartIndex
            //        };
            //    }
            //}
            //else
            //{
            //    return new QueryResult<IMailboxDataSync>()
            //    {
            //        TotalCount = count,
            //        Condition = queryCondition,
            //        PageInfo = queryPage,
            //        ParentId = CatalogDbInitialize.MailboxStartIndex
            //    };
            //}
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
            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.Append(" select * from foldersync ");
            List<SQLiteParameter> paramters = new List<SQLiteParameter>();

            List<string> whereSql = new List<string>();
            if (CatalogSyncDbContext.IsInFolder(id))
            {
                var parentFolder = (from m in _queryContext.Folders where m.UniqueId == id select m).FirstOrDefault();
                if (parentFolder == null)
                {
                    return Convert<FolderSyncModel, IFolderDataSync>(new QueryResult<FolderSyncModel>()
                    {
                        TotalCount = 0,
                        Condition = queryCondition,
                        PageInfo = queryPage,
                        Items = new List<FolderSyncModel>(0),
                        ParentId = id
                    });
                }
                else
                {
                    var pName = string.Format("@ParentFolderId");
                    var p = new SQLiteParameter(pName, parentFolder.FolderId);
                    paramters.Add(p);
                    whereSql.Add(string.Format("{0} = {1}", "ParentFolderId", pName));
                }
            }
            else
            {
                var mailbox = (from m in _queryContext.Mailboxes where m.UniqueId == id select m).FirstOrDefault();
                if (mailbox == null)
                {
                    return Convert<FolderSyncModel, IFolderDataSync>(new QueryResult<FolderSyncModel>()
                    {
                        TotalCount = 0,
                        Condition = queryCondition,
                        PageInfo = queryPage,
                        Items = new List<FolderSyncModel>(0),
                        ParentId = id
                    });
                }
                else
                {
                    var pName = string.Format("@ParentFolderId");
                    var p = new SQLiteParameter(pName, mailbox.RootFolderId);
                    whereSql.Add(string.Format("{0} = {1}", "ParentFolderId", pName));
                    paramters.Add(p);
                }
            }

            if (queryCondition != null && queryCondition.SearchField != null)
            {
                if (!string.IsNullOrEmpty(queryCondition.SearchField.SearchValue))
                {
                    switch (queryCondition.SearchField.FieldName.ToLower())
                    {
                        case "displayname":
                            var paramterName = string.Format("@{0}", queryCondition.SearchField.FieldName);
                            whereSql.Add(string.Format(" displayname like %{0}% ", paramterName));
                            var p = new SQLiteParameter(paramterName, queryCondition.SearchField.SearchValue);
                            paramters.Add(p);
                            break;
                    }
                }
            }

            if (whereSql.Count > 0)
            {
                sqlBuilder.Append(" where ").Append(string.Join(" and ", whereSql)).Append(" ");
            }


            if (queryCondition != null && queryCondition.SortFields.Count > 0)
            {
                sqlBuilder.Append(" order by ");
                string[] orderbyArray = new string[queryCondition.SortFields.Count];
                int i = 0;
                foreach (var orderCondition in queryCondition.SortFields)
                {
                    switch (orderCondition.FieldName.ToLower())
                    {
                        case "displayname":
                            orderbyArray[i] = string.Format(" {0} {1} ", orderCondition.FieldName, orderCondition.isDescend ? "desc" : "asc");
                            break;
                    }
                }

                sqlBuilder.Append(string.Join(",", orderbyArray));
            }
            else
            {
                sqlBuilder.Append(" order by displayname asc ");
            }

            string countSql = string.Format(" select count(*) from ({0})", sqlBuilder);

            var count = _queryContext.Database.SqlQuery<int>(countSql, paramters.ToArray()).FirstOrDefault();

            if (!isOnlyGetCount)
            {
                IEnumerable<FolderSyncModel> items;
                if (queryPage != null)
                {
                    string pageSql = string.Format("select * from ({0}) LIMIT {1} OFFSET {2} ", sqlBuilder, queryPage.PageCount, queryPage.StartIndex);

                    items = _queryContext.Database.SqlQuery<FolderSyncModel>(pageSql, paramters.ToArray());
                    return Convert<FolderSyncModel, IFolderDataSync>(new QueryResult<FolderSyncModel>()
                    {
                        TotalCount = count,
                        Condition = queryCondition,
                        PageInfo = queryPage,
                        Items = items,
                        ParentId = id
                    });
                }
                else
                {
                    items = _queryContext.Database.SqlQuery<FolderSyncModel>(sqlBuilder.ToString(), paramters.ToArray());
                    return Convert<FolderSyncModel, IFolderDataSync>(new QueryResult<FolderSyncModel>()
                    {
                        TotalCount = count,
                        Condition = queryCondition,
                        PageInfo = queryPage,
                        Items = items,
                        ParentId = id
                    });
                }
            }
            else
            {
                return Convert<FolderSyncModel, IFolderDataSync>(new QueryResult<FolderSyncModel>()
                {
                    TotalCount = count,
                    Condition = queryCondition,
                    PageInfo = queryPage,
                    ParentId = id
                });
            }


            //IQueryable<IFolderDataSync> result = from m in _queryContext.Folders select m;

            //if (CatalogSyncDbContext.IsInFolder(id))
            //{
            //    var parentFolder = (from m in _queryContext.Folders where m.UniqueId == id select m).FirstOrDefault();
            //    if (parentFolder == null)
            //    {
            //        return new QueryResult<IFolderDataSync>()
            //        {
            //            TotalCount = 0,
            //            Condition = queryCondition,
            //            PageInfo = queryPage,
            //            Items = new List<IFolderDataSync>(0),
            //            ParentId = id
            //        };
            //    }
            //    else
            //    {
            //        result = from m in result where m.ParentFolderId == parentFolder.FolderId select m;
            //    }
            //}
            //else
            //{
            //    var mailbox = (from m in _queryContext.Mailboxes where m.UniqueId == id select m).FirstOrDefault();
            //    if (mailbox == null)
            //    {
            //        return new QueryResult<IFolderDataSync>()
            //        {
            //            TotalCount = 0,
            //            Condition = queryCondition,
            //            PageInfo = queryPage,
            //            Items = new List<IFolderDataSync>(0),
            //            ParentId = id
            //        };
            //    }
            //    else
            //    {
            //        result = from m in result where m.ParentFolderId == mailbox.RootFolderId select m;
            //    }
            //}

            //if (queryCondition != null && queryCondition.SearchField != null)
            //{
            //    if (!string.IsNullOrEmpty(queryCondition.SearchField.SearchValue))
            //    {
            //        switch (queryCondition.SearchField.FieldName.ToLower())
            //        {
            //            case "displayname":
            //                result = from m in result where ((IItemBase)m).DisplayName.Contains(queryCondition.SearchField.SearchValue) select m;
            //                break;
            //            default:
            //                throw new NotSupportedException(string.Format("not support {0} [{1}] search", queryCondition.SearchField.FieldName, queryCondition.SearchField.SearchValue));
            //        }
            //    }
            //}

            //if (queryCondition != null && queryCondition.SortFields.Count > 0)
            //{
            //    foreach (var orderCondition in queryCondition.SortFields)
            //    {
            //        if (orderCondition.isDescend)
            //        {
            //            switch (orderCondition.FieldName.ToLower())
            //            {
            //                case "displayname":
            //                    result = from m in result orderby ((IItemBase)m).DisplayName descending select m;
            //                    break;
            //            }
            //        }
            //        else
            //        {
            //            switch (orderCondition.FieldName.ToLower())
            //            {
            //                case "displayname":
            //                    result = from m in result orderby ((IItemBase)m).DisplayName select m;
            //                    break;
            //            }
            //        }
            //    }
            //}



            //var totalCount = result.Count();

            //if (!isOnlyGetCount)
            //{
            //    if (queryPage != null)
            //    {
            //        return new QueryResult<IFolderDataSync>()
            //        {
            //            TotalCount = totalCount,
            //            Condition = queryCondition,
            //            PageInfo = queryPage,
            //            Items = result.Skip(queryPage.StartIndex).Take(queryPage.PageCount),
            //            ParentId = id
            //        };
            //    }
            //    else
            //    {
            //        return new QueryResult<IFolderDataSync>()
            //        {
            //            TotalCount = totalCount,
            //            Condition = queryCondition,
            //            PageInfo = queryPage,
            //            Items = result,
            //            ParentId = id
            //        };
            //    }
            //}
            //else
            //{
            //    return new QueryResult<IFolderDataSync>()
            //    {
            //        TotalCount = totalCount,
            //        Condition = queryCondition,
            //        PageInfo = queryPage,
            //        ParentId = id
            //    };
            //}
        }

        private QueryResult<IT> Convert<T, IT>(QueryResult<T> impl) where T : IT
        {
            var result = new QueryResult<IT>()
            {
                TotalCount = impl.TotalCount,
                Condition = impl.Condition,
                PageInfo = impl.PageInfo,
                ParentId = impl.ParentId
            };

            var resultItems = new List<IT>();
            foreach (var item in impl.Items)
            {
                resultItems.Add(item);
            }
            result.Items = resultItems;
            return result;
        }

        public QueryResult<IItemDataSync> GetItemsForCom(Int64 id, QueryCondition queryCondition, QueryPage queryPage, bool isOnlyGetCount = false)
        {
            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.Append(" select * from itemsync ");
            List<SQLiteParameter> paramters = new List<SQLiteParameter>();

            List<string> whereSql = new List<string>();
            var parentFolder = (from m in _queryContext.Folders where m.UniqueId == id select m).FirstOrDefault();
            if (parentFolder == null)
            {
                return Convert<ItemSyncModel, IItemDataSync>(new QueryResult<ItemSyncModel>()
                {
                    TotalCount = 0,
                    Condition = queryCondition,
                    PageInfo = queryPage,
                    Items = new List<ItemSyncModel>(0),
                    ParentId = id
                });
            }
            else
            {
                var pName = string.Format("@ParentFolderId");
                var p = new SQLiteParameter(pName, parentFolder.FolderId);
                paramters.Add(p);
                whereSql.Add(string.Format("{0} = {1}", "ParentFolderId", pName));
            }

            if (queryCondition != null && queryCondition.SearchField != null)
            {
                if (!string.IsNullOrEmpty(queryCondition.SearchField.SearchValue))
                {
                    switch (queryCondition.SearchField.FieldName.ToLower())
                    {
                        case "displayname":
                            var paramterName = string.Format("@{0}", queryCondition.SearchField.FieldName);
                            whereSql.Add(string.Format(" displayname like %{0}% ", paramterName));
                            var p = new SQLiteParameter(paramterName, queryCondition.SearchField.SearchValue);
                            paramters.Add(p);
                            break;
                    }
                }
            }

            if (whereSql.Count > 0)
            {
                sqlBuilder.Append(" where ").Append(string.Join(" and ", whereSql)).Append(" ");
            }


            if (queryCondition != null && queryCondition.SortFields.Count > 0)
            {
                sqlBuilder.Append(" order by ");
                string[] orderbyArray = new string[queryCondition.SortFields.Count];
                int i = 0;
                foreach (var orderCondition in queryCondition.SortFields)
                {
                    switch (orderCondition.FieldName.ToLower())
                    {
                        case "displayname":
                            orderbyArray[i] = string.Format(" {0} {1} ", orderCondition.FieldName, orderCondition.isDescend ? "desc" : "asc");
                            break;
                    }
                }

                sqlBuilder.Append(string.Join(",", orderbyArray));
            }
            else
            {
                sqlBuilder.Append(" order by displayname asc ");
            }

            string countSql = string.Format(" select count(*) from ({0})", sqlBuilder);

            var count = _queryContext.Database.SqlQuery<int>(countSql, paramters.ToArray()).FirstOrDefault();

            if (!isOnlyGetCount)
            {
                IEnumerable<ItemSyncModel> items;
                if (queryPage != null)
                {
                    string pageSql = string.Format("select * from ({0}) LIMIT {1} OFFSET {2} ", sqlBuilder, queryPage.PageCount, queryPage.StartIndex);

                    items = _queryContext.Database.SqlQuery<ItemSyncModel>(pageSql, paramters.ToArray());
                    return Convert<ItemSyncModel, IItemDataSync>(new QueryResult<ItemSyncModel>()
                    {
                        TotalCount = count,
                        Condition = queryCondition,
                        PageInfo = queryPage,
                        Items = items,
                        ParentId = id
                    });
                }
                else
                {
                    items = _queryContext.Database.SqlQuery<ItemSyncModel>(sqlBuilder.ToString(), paramters.ToArray());
                    return Convert<ItemSyncModel, IItemDataSync>(new QueryResult<ItemSyncModel>()
                    {
                        TotalCount = count,
                        Condition = queryCondition,
                        PageInfo = queryPage,
                        Items = items,
                        ParentId = id
                    });
                }
            }
            else
            {
                return Convert<ItemSyncModel, IItemDataSync>(new QueryResult<ItemSyncModel>()
                {
                    TotalCount = count,
                    Condition = queryCondition,
                    PageInfo = queryPage,
                    ParentId = id
                });
            }



            //var folder = (from m in _queryContext.Folders where m.UniqueId == folderId select m).FirstOrDefault();
            //if (folder == null)
            //{
            //    return new QueryResult<IItemDataSync>()
            //    {
            //        TotalCount = 0,
            //        Condition = queryCondition,
            //        PageInfo = queryPage,
            //        Items = new List<IItemDataSync>(0),
            //        ParentId = folderId
            //    };
            //}

            //var result = from m in _queryContext.Items where m.ParentFolderId == folder.ParentFolderId select m;

            //if (queryCondition != null && queryCondition.SearchField != null)
            //{
            //    if (!string.IsNullOrEmpty(queryCondition.SearchField.SearchValue))
            //    {
            //        switch (queryCondition.SearchField.FieldName.ToLower())
            //        {
            //            case "displayname":
            //                result = from m in result where ((IItemBase)m).DisplayName.Contains(queryCondition.SearchField.SearchValue) select m;
            //                break;
            //            default:
            //                throw new NotSupportedException(string.Format("not support {0} [{1}] search", queryCondition.SearchField.FieldName, queryCondition.SearchField.SearchValue));
            //        }
            //    }
            //}

            //if (queryCondition != null && queryCondition.SortFields.Count > 0)
            //{
            //    foreach (var orderCondition in queryCondition.SortFields)
            //    {

            //        if (orderCondition.isDescend)
            //        {
            //            switch (orderCondition.FieldName.ToLower())
            //            {
            //                case "displayname":
            //                    result = from m in result orderby ((IItemBase)m).DisplayName descending select m;
            //                    break;
            //            }
            //        }
            //        else
            //        {
            //            switch (orderCondition.FieldName.ToLower())
            //            {
            //                case "displayname":
            //                    result = from m in result orderby ((IItemBase)m).DisplayName select m;
            //                    break;
            //            }
            //        }
            //    }
            //}

            //var totalCount = result.Count();

            //if (!isOnlyGetCount)
            //{
            //    if (queryPage != null)
            //    {
            //        return new QueryResult<IItemDataSync>()
            //        {
            //            TotalCount = totalCount,
            //            Condition = queryCondition,
            //            PageInfo = queryPage,
            //            Items = result.Skip(queryPage.StartIndex).Take(queryPage.PageCount),
            //            ParentId = folderId
            //        };
            //    }
            //    else
            //    {
            //        return new QueryResult<IItemDataSync>()
            //        {
            //            TotalCount = totalCount,
            //            Condition = queryCondition,
            //            PageInfo = queryPage,
            //            Items = result,
            //            ParentId = folderId
            //        };
            //    }
            //}
            //else
            //{
            //    return new QueryResult<IItemDataSync>()
            //    {
            //        TotalCount = totalCount,
            //        Condition = queryCondition,
            //        PageInfo = queryPage,
            //        ParentId = CatalogDbInitialize.MailboxStartIndex
            //    };
            //}
        }

        public int ReadDataFromStorage(IItemDataSync item, byte[] buffer, int offset, int length)
        {
            throw new NotImplementedException();
        }

        public void ImportItemError(EwsResponseException ewsResponseError)
        {
            throw new NotImplementedException();
        }
    }
}
