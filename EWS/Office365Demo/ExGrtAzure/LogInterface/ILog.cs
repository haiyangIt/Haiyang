using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogInterface
{
    public interface ILog : IDisposable
    {
        void WriteLog(LogLevel level, string message);
        void WriteException(LogLevel level, string message, Exception exception, string exMsg);
        void WriteLog(LogLevel level, string message, string format, params object[] args);
        event EventHandler<string> WriteLogMsgEvent;

        string GetTotalLog(DateTime date);
    }
}
