using Arcserve.Office365.Exchange.EwsApi.Increment;
using Arcserve.Office365.Exchange.Thread;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.DataProtect.Interface.Backup.Increment
{
    public interface IExchangeAccess<ProgressType>
    {
        ICatalogAccess<ProgressType> CatalogAccess { get; set; }
        IEwsServiceAdapter<ProgressType> EwsServiceAdapter { get; set; }
        IDataFromClient<ProgressType> DataFromClient { get; set; }
    }
}
