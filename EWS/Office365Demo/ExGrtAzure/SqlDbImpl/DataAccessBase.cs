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

        public void ResetAllStorage(string mailboxAddress, string organization)
        {
            SqlDbResetHelper helper = new SqlDbResetHelper();
            helper.ResetBlobData(mailboxAddress, organization);
            helper.DeleteDatabase(organization);
        }

        public void ResetAllStorage(string mailboxAddress)
        {
            SqlDbResetHelper helper = new SqlDbResetHelper();
            helper.ResetBlobData(mailboxAddress, string.Empty, true);
        }

        public abstract void Dispose();
    }
}
