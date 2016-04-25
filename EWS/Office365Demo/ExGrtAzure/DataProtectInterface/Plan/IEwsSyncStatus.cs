using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProtectInterface.Plan
{
    public interface IEwsSyncStatus : IList<IEwsSyncStatusItem>
    {
    }

    public interface IEwsSyncStatusItem
    {
        string Id { get; }
        string SyncStatus { get; }
    }
}
