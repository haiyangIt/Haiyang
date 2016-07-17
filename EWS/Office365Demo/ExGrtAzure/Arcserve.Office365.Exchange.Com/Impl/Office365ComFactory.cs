using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Arcserve.Office365.Exchange.Com.Impl
{
    [Guid("261BE673-55BA-4E5A-BE34-AF0830EC4D84")]
    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true)]
    public class Office365ComFactory : IOffice365ComFactory
    {
        public IQueryCatalog CreateQueryCatalog()
        {
            return new QueryCatalog();
        }

        public IQueryCondition CreateQueryCondition()
        {
            return new QueryCondition();
        }
    }
}
