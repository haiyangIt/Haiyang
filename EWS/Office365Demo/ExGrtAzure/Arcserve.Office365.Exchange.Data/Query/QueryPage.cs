using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Data.Query
{
    public class QueryPage
    {
        public int PageIndex { get; set; }
        public int StartIndex { get; set; }
        public int PageCount { get; set; }

        public const int GetTotalStartIndex = -1;
        public const int GetTotalPageCount = -1;
        public const int GetTotalPageIndex = -1;
    }

    
}
