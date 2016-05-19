using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Thread
{
    public class ThreadData
    {
        [ThreadStatic]
        private static string _information;
        public static string Information
        {
            get
            {
                return _information;
            }
            set
            {
                _information = value;
            }
        }
    }
}
