using Arcserve.Office365.Exchange.Manager.Impl;
using Arcserve.Office365.Exchange.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
        event EventHandler<string> WriteLogMsgEvent;

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
        

        public abstract string GetTotalLog(DateTime date);

        public abstract void Write(string information);

        public abstract void WriteLine(string information);
    }

    public class LogToStreamManage : ManagerBase, ILogStreamProvider
    {
        private static object _lock = new object();
        private int LogCount = 0;
        private int _MaxLogCount = 500;
        private int FileIndex = 0;
        private string _logFolder;
        private string _fileNameFormat;
        private FileStream _fileStream = null;
        private StreamWriter _writer = null;
        private StreamWriter writer
        {
            get
            {
                if (_writer == null || LogCount > _MaxLogCount)
                {
                    using (_lock.LockWhile(() =>
                    {
                        if (LogCount > _MaxLogCount)
                        {
                            DoDispose();
                            LogCount = 0;
                        }

                        if (_writer == null)
                        {
                            var filePath = string.Empty;
                            do
                            {
                                filePath = Path.Combine(_logFolder, string.Format(_fileNameFormat, DateTime.Now.ToString("yyyyMMdd"), FileIndex++));
                            } while (File.Exists(filePath));
                            _fileStream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.Read);
                            _writer = new StreamWriter(_fileStream);
                        }
                    }))
                    { }
                }
                return _writer;
            }
        }

        public override string ManagerName
        {
            get
            {
                return "LogFileManager";
            }
        }

        private Guid _stream = Guid.NewGuid();
        public virtual Guid StreamId
        {
            get
            {
                return _stream;
            }
        }
        

        public LogToStreamManage(string logFolder, string fileNameFormat, int fileMaxCount = 500)
        {
            _logFolder = logFolder;
            _fileNameFormat = fileNameFormat;
            _MaxLogCount = fileMaxCount;
        }

        Queue<string> _queueMsg = new Queue<string>();
        public void WriteLog(string msg)
        {
            CheckOtherEventCanExecute();

            using (_queueMsg.LockWhile(() =>
            {
                _queueMsg.Enqueue(msg);
            }))
            { }

            TriggerOtherEvent(0);
        }

        public void Write(string information)
        {
            WriteLog(information);
        }

        public void WriteLine(string information)
        {
            WriteLog(information);
        }

        public string GetTotalLog(DateTime date)
        {
            throw new NotImplementedException();
        }


        protected void DoWriteLog(string msg)
        {
            Interlocked.Increment(ref LogCount);
            writer.WriteLine(msg);
            if (LogCount % 20 == 0)
            {
                writer.Flush();
                _fileStream.Flush();
                Trace.Flush();
            }
        }

        protected void DoDispose()
        {
            if (_writer != null)
            {
                _writer.Dispose();
                Trace.WriteLine("writer Disposed");
                _writer = null;

            }
            if (_fileStream != null)
            {
                _fileStream.Dispose();
                Trace.WriteLine("_fileStream Disposed");
                _fileStream = null;
            }
            Trace.Flush();
        }

        protected void Flush()
        {
            writer.Flush();
            _fileStream.Flush();
            Trace.Flush();
        }

        protected override void MethodWhenOtherEventTriggered(int eventIndex = 0)
        {
            InternalWriteLog();
        }

        protected override void MethodWhenTimeOutTriggerd()
        {
            InternalWriteLog();
        }

        private void InternalWriteLog()
        {
            List<string> msg = new List<string>();
            using (_queueMsg.LockWhile(() =>
            {
                while(_queueMsg.Count > 0)
                {
                    msg.Add(_queueMsg.Dequeue());
                }
            }))
            { }

            foreach(var m in msg)
            {
                DoWriteLog(m);
            } 
        }

        protected override void AfterEnd()
        {
            base.AfterEnd();
            DoDispose();
        }

        
    }
}
