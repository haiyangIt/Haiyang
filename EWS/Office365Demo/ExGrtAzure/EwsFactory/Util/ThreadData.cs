using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EwsFrame.Util
{
    public class ThreadData
    {
        [ThreadStatic]
        private static string _information;
        public static string Information
        {
            get
            {
                if (string.IsNullOrEmpty(_information))
                {
                    _information = (-1).ToString("D8");
                }
                return _information;
            }
            set
            {
                _information = value;
            }
        }
    }
}
