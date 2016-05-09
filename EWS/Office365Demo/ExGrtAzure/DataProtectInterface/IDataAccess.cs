using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProtectInterface
{
    public interface IDataAccess: IDisposable
    {
        void BeginTransaction();
        void EndTransaction(bool isCommit);

        void ResetAllStorage(string mailboxAddress, string organization);

        void ResetAllStorage(string mailboxAddress);

        DbContext DbContext { get; }
        void SaveChanges();
    }
}
