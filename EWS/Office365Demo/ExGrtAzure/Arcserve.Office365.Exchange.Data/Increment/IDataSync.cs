using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Data.Increment
{
    public interface IDataSync
    {
        string SyncStatus { get; set; }

        string ChangeKey { get; }

        Int64 UniqueId { get; }
    }

}
