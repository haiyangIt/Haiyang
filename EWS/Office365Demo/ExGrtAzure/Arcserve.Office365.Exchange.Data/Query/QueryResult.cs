using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Data.Query
{
    public class QueryResult<T>
    {
        public int TotalCount { get; set; }
        public QueryPage PageInfo { get; set; }
        public QueryCondition Condition { get; set; }
        public IEnumerable<T> Items { get; set; }
        public Int64 ParentId { get; set; }

        private int _count = -1;
        public int Count
        {
            get
            {
                if (_count == -1)
                {
                    if (TotalCount == 0)
                        _count = 0;
                    _count = Items.Count();
                }
                return _count;
            }
        }
    }
}
