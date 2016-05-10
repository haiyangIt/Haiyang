using EwsFrame.Util;
using LogInterface;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LogImpl
{
    public class DefaultLog : ILog
    {
        private string _logFolder;
        protected virtual string LogFolder
        {
            get
            {
                if (string.IsNullOrEmpty(_logFolder))
                {
                    string logFolder = ConfigurationManager.AppSettings["LogPath"];
                    if (string.IsNullOrEmpty(logFolder))
                    {
                        logFolder = AppDomain.CurrentDomain.BaseDirectory;
                        logFolder = Path.Combine(logFolder, "Log");
                    }
                    if (!Directory.Exists(logFolder))
                    {
                        Directory.CreateDirectory(logFolder);
                    }
                    _logFolder = logFolder;
                    //var logPath = Path.Combine(logFolder, LogFileNameFormat);
                    //_logPath = logPath;

                }
                return _logFolder;
            }
        }

        public virtual int LogMaxRecordCount
        {
            get
            {
                return 500;
            }
        }

        protected virtual string LogFileNameFormat
        {
            get
            {
                return "{0}_{1}.txt";
            }
        }


        public DefaultLog()
        {

        }


        private static object _lock = new object();
        private LogToStreamManage _instance = null;
        private LogToStreamManage Instance
        {
            get
            {
                if (_instance == null)
                {
                    using (_lock.LockWhile(() =>
                    {
                        if (_instance == null)
                        {
                            _instance = new LogToStreamManage(LogFolder, LogFileNameFormat, LogMaxRecordCount);
                        }
                    }))
                    { }
                }
                return _instance;
            }
        }

        protected virtual void WriteLog(string msg)
        {
            Instance.WriteToLog(msg);
            TriggerEvent(msg);
        }

        public void WriteException(LogLevel level, string message, Exception exception, string exMsg)
        {
            WriteLog(GetExceptionString(level, message, exception, exMsg));
        }
        const string blank = "\t";

        public event EventHandler<string> WriteLogMsgEvent;

        private void TriggerEvent(string msg)
        {
            try
            {
                if (WriteLogMsgEvent != null)
                {
                    WriteLogMsgEvent.Invoke(null, msg);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.TraceError(e.GetExceptionDetail());
            }
        }

        private static string TimeFormat = "yyyy-MM-dd HH:mm:ss";

        public static string GetExceptionString(LogLevel level, string message, Exception exception, string exMsg)
        {
            StringBuilder sb = new StringBuilder();
            var curEx = exception;
            while (curEx != null)
            {
                if (curEx is AggregateException)
                {
                    sb.AppendLine(GetAggrateException(level, message, curEx as AggregateException, exMsg));
                }
                else
                {
                    sb.AppendLine(string.Join(blank, DateTime.Now.ToString(TimeFormat),
                        LogLevelHelper.GetLevelString(level),
                        message.RemoveRN(),
                        curEx.Message.RemoveRN(),
                        curEx.StackTrace.RemoveRN()));

                    curEx = curEx.InnerException;
                }
            }
            sb.AppendLine();
            return sb.ToString();
        }

        internal static string GetAggrateException(LogLevel level, string message, AggregateException ex, string exMsg)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Join(blank, DateTime.Now.ToString(TimeFormat),
                    LogLevelHelper.GetLevelString(level),
                    message.RemoveRN(),
                    ex.Message.RemoveRN(),
                    ex.StackTrace.RemoveRN()));

            foreach (var innerEx in ex.Flatten().InnerExceptions)
            {
                sb.AppendLine(GetExceptionString(level, message, ex, exMsg));
            }
            return sb.ToString();
        }

        public void WriteLog(LogLevel level, string message)
        {
            WriteLog(GetLogString(level, message));
        }

        internal static string GetLogString(LogLevel level, string message)
        {
            return string.Join(blank, DateTime.Now.ToString(TimeFormat),
                LogLevelHelper.GetLevelString(level),
                message.RemoveRN());
        }

        public void WriteLog(LogLevel level, string message, string format, params object[] args)
        {
            WriteLog(GetLogString(level, message, format, args));
        }

        internal static string GetLogString(LogLevel level, string message, string format, params object[] args)
        {
            return string.Join(blank, DateTime.Now.ToString(TimeFormat),
                LogLevelHelper.GetLevelString(level),
                message.RemoveRN(),
                args.Length > 0 ? string.Format(format, args).RemoveRN() : format.RemoveRN());
        }

        public string GetTotalLog(DateTime date)
        {
            if (File.Exists(LogFolder))
                using (var stream = new FileStream(LogFolder, FileMode.Open, FileAccess.Read))
                {
                    StreamReader reader = new StreamReader(stream);
                    return reader.ReadToEnd();
                }
            else
                return "Log file is not exist.";
        }

        public void Dispose()
        {
            if (Instance != null)
                Instance.Dispose();
        }
    }
    internal static class StringEx
    {
        internal static string RemoveRN(this string message)
        {
            return message.Replace('\r', ' ').Replace('\n', ' ');
        }
    }

    public class LogToStreamManage : ManageBase
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
                            var filePath = Path.Combine(_logFolder, string.Format(_fileNameFormat, DateTime.Now.ToString("yyyyMMdd"), FileIndex++));
                            _fileStream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.Read);
                            _writer = new StreamWriter(_fileStream);
                        }
                    }))
                    { }
                }
                return _writer;
            }
        }
        public LogToStreamManage(string logFolder, string fileNameFormat, int fileMaxCount = 500) : base("LogToStream" + fileNameFormat)
        {
            _logFolder = logFolder;
            _fileNameFormat = fileNameFormat;
            _MaxLogCount = fileMaxCount;
        }

        
        protected override void DoWriteLog(string msg)
        {
            Interlocked.Increment(ref LogCount);
            writer.WriteLine(msg);
            if (LogCount % 20 == 0)
                writer.Flush();
            _fileStream.Flush();
        }

        protected override void DoDispose()
        {
            if (_writer != null)
            {
                _writer.Dispose();
                Debug.WriteLine("writer Disposed");
                _writer = null;

            }
            if (_fileStream != null)
            {
                _fileStream.Dispose();
                Debug.WriteLine("_fileStream Disposed");
                _fileStream = null;
            }
        }

        protected override void Flush()
        {
            writer.Flush();
        }
    }

    public abstract class ManageBase : IDisposable
    {
        private ConcurrentQueue<string> msgQueue = new ConcurrentQueue<string>();
        private AutoResetEvent _logEvent = new AutoResetEvent(false);
        private AutoResetEvent _endEvent = new AutoResetEvent(false);
        private AutoResetEvent _endedEvent;
        private AutoResetEvent[] events;
        private Thread _logThread;
        protected ManageBase(string threadName)
        {
            events = new AutoResetEvent[2] { _logEvent, _endEvent };
            _logThread = new Thread(InternalWriteLog);
            _logThread.Name = threadName;
            _logThread.Start();
        }

        public void WriteToLog(string msg)
        {
            msgQueue.Enqueue(msg);
            _logEvent.Set();
        }
        

        private void InternalWriteLog()
        {
            while (true)
            {
                var result = WaitHandle.WaitAny(events, 1000);
                bool isBreak = false;
                switch (result)
                {
                    case WaitHandle.WaitTimeout:
                    case 0:
                    case 1:
                        if (result == 1)
                            Thread.Sleep(5000);
                        while (true)
                        {
                            try
                            {
                                string msg;
                                bool hasElement = msgQueue.TryDequeue(out msg);
                                if (hasElement)
                                {
                                    DoWriteLog(msg);
                                }
                                else
                                    break;
                            }
                            catch (Exception e)
                            {
                                System.Diagnostics.Trace.TraceError(e.GetExceptionDetail());
                            }
                        }

                        if (result == 1)
                        {
                            try {
                                DoWriteLog("Log end.");
                                isBreak = true;
                            }
                            catch(Exception e)
                            {
                                System.Diagnostics.Trace.TraceError(e.GetExceptionDetail());
                            }
                        }
                        if(result == WaitHandle.WaitTimeout)
                        {
                            try {
                                Flush();
                            }
                            catch(Exception ex1)
                            {
                                System.Diagnostics.Trace.TraceError(ex1.GetExceptionDetail());
                            }
                        }
                        break;
                }
                if (isBreak)
                    break;
            }
            Debug.WriteLine("Manager internal Dispose");
            _logEvent.Dispose();
            _endEvent.Dispose();
            _endedEvent.Set();

        }

        protected abstract void DoWriteLog(string msg);
        protected abstract void Flush();

        public void Dispose()
        {
            _endEvent.Set();
            _endedEvent = new AutoResetEvent(false);
            Debug.WriteLine("Manager Dispose");
            _endedEvent.WaitOne();
            _endedEvent.Dispose();
            DoDispose();
            Debug.WriteLine("Manager Disposed");
        }

        protected abstract void DoDispose();
    }

    //public class LogThreadManager
    //{
    //    private static object instanceLockObj = new object();
    //    private static LogThreadManager _instance;
    //    public static LogThreadManager Instance
    //    {
    //        get
    //        {
    //            if (_instance == null)
    //            {
    //                lock (instanceLockObj)
    //                {
    //                    if (_instance == null)
    //                    {
    //                        _instance = new LogThreadManager();
    //                    }
    //                }
    //            }
    //            return _instance;
    //        }
    //    }
    //    private static object _lockObj = new object();
    //    private Thread _thread;

    //    public readonly object SyncLockObj = new object();
    //    private readonly Dictionary<Guid, DisposableObj<Stream>> _allStream = new Dictionary<Guid, DisposableObj<Stream>>();
    //    private readonly Dictionary<Guid, DisposableObj<IDisposable>> _allDisposableObj = new Dictionary<Guid, DisposableObj<IDisposable>>();

    //    private LogThreadManager()
    //    {
    //        _thread = new Thread(Run);
    //        _thread.Name = "ManagerLogStream";
    //        _thread.Start();
    //    }

    //    private const int ExpireSecond = 120 * 1000;
    //    private void Run()
    //    {
    //        Thread.Sleep(120 * 1000);
    //        lock (SyncLockObj)
    //        {
    //            DateTime now = DateTime.Now;

    //            Dispose(_allStream, now, (m) => { m.Close(); m.Dispose(); });
    //            Dispose(_allDisposableObj, now, m => m.Dispose());
    //        }
    //    }

    //    private delegate void DisposeCallFunc<T>(T obj);
    //    private void Dispose<T>(Dictionary<Guid, DisposableObj<T>> objects, DateTime now, DisposeCallFunc<T> func)
    //    {
    //        List<Guid> expireStreams = new List<Guid>(4);
    //        foreach (var keyValue in objects)
    //        {
    //            if (keyValue.Value.IsExpire(now, ExpireSecond))
    //            {
    //                expireStreams.Add(keyValue.Key);
    //            }
    //        }

    //        foreach (var expireItem in expireStreams)
    //        {
    //            func.Invoke(objects[expireItem].obj);
    //            objects.Remove(expireItem);
    //        }
    //    }

    //    public void AddStream(Stream stream, Guid key)
    //    {
    //        lock (SyncLockObj)
    //        {
    //            _allStream.Add(key, new DisposableObj<Stream>() { obj = stream, addTime = DateTime.Now });
    //        }
    //    }

    //    public void AddDisposal(IDisposable disObj, Guid key)
    //    {
    //        lock (SyncLockObj)
    //        {
    //            _allDisposableObj.Add(key, new DisposableObj<IDisposable>() { obj = disObj, addTime = DateTime.Now });
    //        }
    //    }

    //    public Stream GetStream(Guid key)
    //    {
    //        lock (SyncLockObj)
    //        {
    //            DisposableObj<Stream> val;
    //            if (_allStream.TryGetValue(key, out val))
    //            {
    //                return val.obj;
    //            }
    //            return null;
    //        }
    //    }


    //    class DisposableObj<T>
    //    {
    //        public T obj;
    //        public DateTime addTime;

    //        public bool IsExpire(DateTime now, int expireSecond)
    //        {
    //            if ((now - addTime).TotalSeconds > expireSecond)
    //                return true;
    //            return false;
    //        }
    //    }
    //}
}
