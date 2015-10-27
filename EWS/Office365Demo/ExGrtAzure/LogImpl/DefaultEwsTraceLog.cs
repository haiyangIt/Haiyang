using LogInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogImpl
{
    public class DefaultEwsTraceLog : DefaultLog
    {
        protected override string LogFileName
        {
            get
            {
                return string.Format("{0}EwsTrace.txt", DateTime.Now.ToString("yyyyMMddHHmmss"));
            }
        }
    }
}
