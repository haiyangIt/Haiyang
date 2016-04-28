using Arcserve.Office365.Exchange.Data.Impl.Mail;
using Arcserve.Office365.Exchange.StorageAccess.Interface;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Arcserve.Office365.Exchange.StorageAccess.Azure
{
    public abstract class DataAccessBase : IDataAccess
    {
        public abstract void BeginTransaction();

        public abstract void EndTransaction(bool isCommit);

        public void ResetAllStorage(string organization)
        {
            SqlDbResetHelper helper = new SqlDbResetHelper();
            helper.ResetBlobData(organization, string.Empty);
            helper.DeleteDatabase(organization);
        }

        public void ResetAllStorage()
        {
            SqlDbResetHelper helper = new SqlDbResetHelper();
            helper.ResetBlobData(string.Empty, string.Empty);
        }
    }

    public abstract class CatalogDataAccessBase : DataAccessBase
    {
        public virtual string Organization { get; set; }
        protected CatalogDataAccessBase(string organization)
        {
            Organization = organization;
        }

        protected CatalogDataAccessBase()
        {

        }

        #region Transaction
        private SqlConnection _sqlConn;
        /// <summary>
        /// Get sql connect and start transaction.
        /// </summary>
        protected SqlConnection SqlConn
        {
            get
            {
                if (_sqlConn == null)
                {
                    _sqlConn = new SqlConnection(CatalogDbContext.GetConnectString(Organization));
                    _sqlConn.Open();
                    var scope = TransactionScope;
                }
                return _sqlConn;
            }
        }

        private TransactionScope _transactionScope;
        private TransactionScope TransactionScope
        {
            get
            {
                if (_transactionScope == null)
                {
                    _transactionScope = new TransactionScope();
                }
                return _transactionScope;
            }
        }

        public override void BeginTransaction()
        {

        }

        public override void EndTransaction(bool isCommit)
        {
            if (_transactionScope != null)
            {
                if (isCommit)
                    TransactionScope.Complete();

                TransactionScope.Dispose();
                SqlConn.Dispose();
            }
        }

        protected CatalogDbContext NewCatalogDbContext(bool isInTransaction)
        {
            if (isInTransaction)
            {
                return new CatalogDbContext(new OrganizationModel() { Name = Organization }, SqlConn, false);
            }
            else
            {
                return new CatalogDbContext(new OrganizationModel() { Name = Organization });
            }
        }
        #endregion

        #region Write Data
        protected void BatchSaveModel<IT, TImpl>(IT data, string keyName, int batchSaveCount, AddToDbSet<TImpl> delegateFunc, bool isInTransaction = true) where TImpl : class, IT
        {
            if (data == null)
            {
                throw new ArgumentException("argument type is not right or argument is null", "folder");
            }

            TImpl model = (TImpl)data;
            SaveModelCache(model, false, keyName, batchSaveCount, delegateFunc, isInTransaction);
        }

        [ThreadStatic]
        private Dictionary<string, object> _otherInformation;
        private Dictionary<string, object> OtherInformation
        {
            get
            {
                if (_otherInformation == null)
                {
                    _otherInformation = new Dictionary<string, object>();
                }
                return _otherInformation;
            }
        }

        protected void SaveModel<IT, TImpl>(IT data, AddToDbSet<TImpl> delegateFunc, bool isInTransaction = true) where TImpl : class, IT
        {
            List<TImpl> modelList = new List<TImpl>(1);
            modelList.Add((TImpl)data);
            using (var context = NewCatalogDbContext(isInTransaction))
            {
                delegateFunc(context, modelList);
                context.SaveChanges();
            }
        }

        protected delegate void AddToDbSet<TEntity>(CatalogDbContext context, List<TEntity> lists) where TEntity : class;

        /// <summary>
        /// batch save informatioin.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="modelData"></param>
        /// <param name="isEnd">is the last data.</param>
        /// <param name="keyName"></param>
        /// <param name="batchSaveCount">the count in each batch save operation.</param>
        /// <param name="delegateFunc"></param>
        protected void SaveModelCache<T>(T modelData, bool isEnd, string keyName, int batchSaveCount, AddToDbSet<T> delegateFunc, bool isInTransaction = true) where T : class
        {
            object modelListObject;
            if (!OtherInformation.TryGetValue(keyName, out modelListObject))
            {
                modelListObject = new List<T>(batchSaveCount);
                OtherInformation.Add(keyName, modelListObject);
            }
            List<T> modelList = modelListObject as List<T>;


            if (modelData != null)
                modelList.Add(modelData);

            if (modelList.Count >= batchSaveCount || isEnd)
            {
                //HashSet<string> ids = new HashSet<string>();
                //bool isMultiItem = false;
                //foreach (var item in modelList)
                //{
                //    IItemData temp = item as IItemData;
                //    if (temp != null)
                //    {
                //        if (ids.Contains(temp.Id))
                //        {
                //            isMultiItem = true;
                //        }
                //        else
                //            ids.Add(temp.Id);
                //    }
                //}


                using (var context = NewCatalogDbContext(isInTransaction))
                {
                    delegateFunc(context, modelList);
                    context.SaveChanges();
                }

                modelList.Clear();
            }
        }
        #endregion

        #region Read Data
        protected delegate IQueryable<T> QueryFunc<T>(CatalogDbContext context);
        protected delegate int QueryCountFunc<T>(CatalogDbContext context);
        protected List<T> QueryDatas<T>(QueryFunc<T> funcObj)
        {
            using (var context = NewCatalogDbContext(false))
            {
                IQueryable<T> query = funcObj(context);
                return query.ToList();
            }
        }

        protected T QueryData<T>(QueryFunc<T> funcObj)
        {
            using (var context = NewCatalogDbContext(false))
            {
                IQueryable<T> query = funcObj(context);
                return query.FirstOrDefault();
            }
        }

        protected int QueryData<T>(QueryCountFunc<T> funcObj)
        {
            using (var context = NewCatalogDbContext(false))
            {
                return funcObj(context);
            }
        }

        protected List<T> QueryData<T>(QueryFunc<T> funcObj, int pageIndex, int pageCount)
        {
            using (var context = NewCatalogDbContext(false))
            {
                var skip = pageCount * pageIndex;
                IQueryable<T> query = funcObj(context).Skip(skip).Take(pageCount);
                return query.ToList();
            }
        }
        #endregion
    }
}
