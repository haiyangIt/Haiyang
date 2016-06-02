using Arcserve.Office365.Exchange.Util.Setting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Log.Impl
{
    internal class DefaultEwsTraceLog : DefaultLog
    {
        public DefaultEwsTraceLog() : base()
        {
            
        }

        protected override bool IsLog()
        {
            return CloudConfig.Instance.IsEwsTraceLog;
        }

        protected override string LogFileNameFormat
        {
            get
            {
                return "{0}_{1}_EwsTrace.txt";
            }
        }
    }
}
