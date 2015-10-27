using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProtectInterface
{
    public interface IDataAccess
    {
        void BeginTransaction();
        void EndTransaction(bool isCommit);

        void ResetAllStorage(string organization);

        void ResetAllStorage();
    }
}
