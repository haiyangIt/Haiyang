using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExGrtOnline.Log
{
    internal interface ILog
    {
        void WriteLog(LogLevel level, string message);
        void WriteException(LogLevel level, Exception exception, string exMsg);
    }
}
