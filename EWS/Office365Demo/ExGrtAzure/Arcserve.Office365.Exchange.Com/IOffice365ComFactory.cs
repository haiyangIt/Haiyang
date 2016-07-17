using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Arcserve.Office365.Exchange.Com
{
    [Guid("3AAC0172-1508-44BF-8DDD-0E5C10F107E8")]
    [ComVisible(true)]
    public interface IOffice365ComFactory
    {
        IQueryCatalog CreateQueryCatalog();
        IQueryCondition CreateQueryCondition();
    }

    
}
