using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Data.Increment
{
    public class EwsResponseException : Exception
    {
        public string ResponseCode { get; set; }
        public string Detail { get; set; }
        public IItemDataSync Item { get; set; }
        public string ResponseClass { get; set; }
        

        public EwsResponseException(IItemDataSync item, string message) : base(message) {
            Item = item;
        }

        public EwsResponseException(IItemDataSync item, string message, string responseClass, string detail, string responseCode) : this(item, message)
        {
            Detail = detail;
            ResponseClass = responseClass;
            ResponseCode = responseCode;
        }
    }
}
