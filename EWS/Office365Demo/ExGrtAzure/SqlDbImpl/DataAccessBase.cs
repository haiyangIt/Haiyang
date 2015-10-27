using DataProtectInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDbImpl
{
    public abstract class DataAccessBase : IDataAccess
    {
        public abstract void BeginTransaction();

        public abstract void EndTransaction(bool isCommit);

        public void ResetAllStorage(string organization)
        {
            SqlDbResetHelper helper = new SqlDbResetHelper();
            helper.ResetBlobData(organization);
            helper.DeleteDatabase(organization);
        }

        public void ResetAllStorage()
        {
            SqlDbResetHelper helper = new SqlDbResetHelper();
            helper.ResetBlobData(string.Empty, true);
        }
    }
}
