using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Data.Query
{
    public class QueryCondition
    {
        public List<OrderCondition> SortFields { get; set; }
        public SearchCondition SearchField { get; set; }
    }

    public class OrderCondition
    {
        public string FieldName { get; set; }
        public bool isDescend { get; set; }
    }

    public class SearchCondition
    {
        public string FieldName { get; set; }
        public string SearchValue { get; set; }
    }
}
