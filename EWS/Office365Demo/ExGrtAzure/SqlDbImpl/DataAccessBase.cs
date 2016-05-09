using DataProtectInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace SqlDbImpl
{
    public abstract class DataAccessBase : IDataAccess
    {
        public abstract DbContext DbContext
        {
            get;
        }

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

        public virtual void SaveChanges()
        {
            bool saveFailed;
            do
            {
                saveFailed = false;

                try
                {
                    DbContext.SaveChanges();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    saveFailed = true;

                    // Update the values of the entity that failed to save from the store 
                    ex.Entries.Single().Reload();
                }

            } while (saveFailed);
        }
    }
}
