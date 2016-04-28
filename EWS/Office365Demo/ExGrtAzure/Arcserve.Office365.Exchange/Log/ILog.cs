using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Log
{
    public interface ILog
    {
        [Obsolete("Please use other api which contains module paramter.")]
        void WriteLog(LogLevel level, string message);
        [Obsolete("Please use other api which contains module paramter.")]
        void WriteException(LogLevel level, string message, Exception exception, string exMsg);
        [Obsolete("Please use other api which contains module paramter.")]
        void WriteLog(LogLevel level, string message, string format, params object[] args);

        void WriteLog(string module, LogLevel level, string message);
        void WriteException(string module, LogLevel level, string message, Exception exception, string exMsg);
        void WriteLog(string module, LogLevel level, string message, string format, params object[] args);

        string GetTotalLog(DateTime date);

        void RegisterLogStream(ILogStreamProvider stream);
        void RemoveLogStream(ILogStreamProvider stream);
    }

    public interface ILogStreamProvider
    {
        Guid StreamId { get; }
        object SyncObj { get; }
        void Write(string information);
        void WriteLine(string information);
        string GetTotalLog(DateTime date);
    }

    public abstract class LogStreamProviderBase : ILogStreamProvider
    {
        private Guid _stream = Guid.NewGuid();
        public virtual Guid StreamId
        {
            get
            {
                return _stream;
            }
        }

        private Object _syncObj = new object();
        public virtual object SyncObj
        {
            get
            {
                return _syncObj;
            }
        }

        public abstract string GetTotalLog(DateTime date);

        public abstract void Write(string information);

        public abstract void WriteLine(string information);
    }
}
