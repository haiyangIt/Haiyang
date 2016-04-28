using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Data
{
    public interface ICatalogJob : ICatalogInfo, IItemBase
    {
        string CatalogJobName { get; }
        string Organization { get; }
    }
}
