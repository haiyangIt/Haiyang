using LogInterface;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LogImpl
{
    public class DefaultEwsTraceLog : DefaultLog
    {
        protected override string LogFileNameFormat
        {
            get
            {
                return "{0}_{1}_EwsTrace.txt";
            }
        }

        public override int LogMaxRecordCount
        {
            get
            {
                int result = 500;
                if (int.TryParse(ConfigurationManager.AppSettings["FileMaxRecordCount"], out result))
                    return result;
                return 500;
            }
        }

        private int _logCount = 0;
        private bool IsWriteLog = true;

        protected override void WriteLog(string msg)
        {
            Interlocked.Increment(ref _logCount);
            if (_logCount > 20)
            {
                IsWriteLog = ConfigurationManager.AppSettings["WriteEWSTrace"] == "1";
                Interlocked.Exchange(ref _logCount, 0);
            }

            if (IsWriteLog)
            {
                base.WriteLog(msg);
            }
        }
    }
}
