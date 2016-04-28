using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Log.Impl
{
    public class DefaultEwsTraceLog : DefaultLog
    {
        public DefaultEwsTraceLog()
        {
            RegisterLogStream(new DefaultLogStream(DefaultLogStream.GetLogPath(LogFileName)));
        }

        protected string LogFileName
        {
            get
            {
                return string.Format("{0}EwsTrace.txt", DateTime.Now.ToString("yyyyMMdd"));
            }
        }
    }
}
