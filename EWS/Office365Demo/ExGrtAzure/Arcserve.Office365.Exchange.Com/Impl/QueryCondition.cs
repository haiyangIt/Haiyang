using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Arcserve.Office365.Exchange.Com.Impl
{
    [Guid("02D41FE2-D7DC-4174-97A9-5D5C50D2139D")]
    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true)]
    public class QueryCondition : IQueryCondition
    {
        public ContentFilter ContentFilter
        {
            get; set;
        }

        public string SortField
        {
            get; set;
        }

        public string SearchString
        {
            get; set;
        }
    }
}
