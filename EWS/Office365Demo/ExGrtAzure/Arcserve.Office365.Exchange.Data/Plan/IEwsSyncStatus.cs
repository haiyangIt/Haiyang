using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Data.Plan
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
