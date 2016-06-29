using Arcserve.Office365.Exchange.Util;
using Arcserve.Office365.Exchange.Util.Setting;
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

namespace Arcserve.Office365.Exchange.Log.Impl
{
    internal class DefaultLog : ILog
    {

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

        protected virtual int LogMaxRecordCount
        {
            get
            {
                return CloudConfig.Instance.LogFileMaxRecordCount;
            }
        }


        protected virtual string LogFileNameFormat
        {
            get
            {
                return "{0}_{1}.txt";
            }
        }


        private string _logFolder;
        private string LogFolder
        {
            get
            {
                if (string.IsNullOrEmpty(_logFolder))
                {
                    string logFolder = CloudConfig.Instance.LogPath;
                    if (string.IsNullOrEmpty(logFolder))
                    {
                        logFolder = AppDomain.CurrentDomain.BaseDirectory;
                        logFolder = Path.Combine(logFolder, "..");
                        logFolder = Path.Combine(logFolder, "..");
                        logFolder = Path.Combine(logFolder, "Logs");
                        logFolder = Path.Combine(logFolder, "Office365Log");
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

        protected virtual bool IsLog()
        {
            return CloudConfig.Instance.IsLog;
        }

        protected void Write(string logDetail)
        {
            if (!IsLog())
            {
                return;
            }
            Instance.WriteToLog(logDetail);
        }

        private void WriteLine(string logDetail)
        {
            if (!IsLog())
            {
                return;
            }
            Instance.WriteToLog(logDetail);
        }

        public void WriteException(LogLevel level, string message, Exception exception, string exMsg)
        {
            WriteLine(GetExceptionString("", level, message, exception, exMsg));
        }
        const string blank = "\t";

        public event EventHandler<string> WriteLogMsgEvent;

        public void WriteLog(LogLevel level, string message)
        {
            WriteLine(GetLogString(level, message));
        }

        internal static string GetLogString(LogLevel level, string message)
        {
            return string.Join(blank, DateTime.Now.ToString("yyyyMMddHHmmss"),
                LogLevelHelper.GetLevelString(level),
                message.RemoveRN());
        }

        public void WriteLog(LogLevel level, string message, string format, params object[] args)
        {
            WriteLine(GetLogString(level, message, format, args));
        }

        internal static string GetLogString(LogLevel level, string message, string format, params object[] args)
        {
            return string.Join(blank, DateTime.Now.ToString("yyyyMMddHHmmss"),
                LogLevelHelper.GetLevelString(level),
                message.RemoveRN(),
                args.Length > 0 ? string.Format(format, args).RemoveRN() : format.RemoveRN());
        }

        public void WriteLog(string module, LogLevel level, string message)
        {
            WriteLine(GetLogString(module, level, message));
        }

        public void WriteException(string module, LogLevel level, string message, Exception exception, string exMsg)
        {
            WriteLine(GetExceptionString(module, level, message, exception, exMsg));
        }

        public void WriteLog(string module, LogLevel level, string message, string format, params object[] args)
        {
            WriteLine(GetLogString(module, level, message, format, args));
        }

        private const string TimeFormat = "yyyy-MM-dd HH:mm:ss";
        public static string GetExceptionString(string module, LogLevel level, string message, Exception exception, string exMsg, int innerLevel = 0)
        {
            StringBuilder sb = new StringBuilder();
            var curEx = exception;
            while (curEx != null)
            {
                if (curEx is AggregateException)
                {
                    sb.AppendLine(GetAggrateException(module, level, message, curEx as AggregateException, exMsg, innerLevel));
                }
                else
                {
                    sb.AppendLine(string.Join(blank, DateTime.Now.ToString(TimeFormat), innerLevel.ToString(string.Format("D{0}", innerLevel * 2)), module, System.Threading.Thread.CurrentThread.ManagedThreadId.ToString("D4"), Task.CurrentId.HasValue ? Task.CurrentId.Value.ToString("D4") : "0000",
                        LogLevelHelper.GetLevelString(level),
                        string.IsNullOrEmpty(message) ? "" : message.RemoveRN(),
                         string.IsNullOrEmpty(curEx.Message) ? "" : curEx.Message.RemoveRN(),
                         curEx.HResult.ToString("X8"),
                         string.IsNullOrEmpty(curEx.StackTrace) ? "" : curEx.StackTrace.RemoveRN(), curEx.GetType().FullName, exMsg));

                    curEx = curEx.InnerException;
                    innerLevel += 1;
                }
            }
            sb.AppendLine();
            return sb.ToString();
        }

        internal static string GetAggrateException(string module, LogLevel level, string message, AggregateException ex, string exMsg, int innerLevel = 0)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Join(blank, module, DateTime.Now.ToString(TimeFormat), innerLevel.ToString(string.Format("D{0}", innerLevel * 2)), System.Threading.Thread.CurrentThread.ManagedThreadId.ToString("D4"), Task.CurrentId.HasValue ? Task.CurrentId.Value.ToString("D4") : "0000",
                    LogLevelHelper.GetLevelString(level),
                    string.IsNullOrEmpty(message) ? "" : message.RemoveRN(),
                    string.IsNullOrEmpty(ex.Message) ? "" : ex.Message.RemoveRN(),
                    ex.HResult.ToString("X8"),
                    string.IsNullOrEmpty(ex.StackTrace) ? "" : ex.StackTrace.RemoveRN(), ex.GetType().FullName, exMsg));

            foreach (var innerEx in ex.Flatten().InnerExceptions)
            {
                sb.AppendLine(GetExceptionString(module, level, message, ex, exMsg, innerLevel + 1));
            }
            return sb.ToString();
        }
        internal static string GetLogString(string module, LogLevel level, string message)
        {
            return string.Join(blank, DateTime.Now.ToString("yyyyMMddHHmmss"), module,
                LogLevelHelper.GetLevelString(level),
                message.RemoveRN());
        }

        internal static string GetLogString(string module, LogLevel level, string message, string format, params object[] args)
        {
            return string.Join(blank, DateTime.Now.ToString("yyyyMMddHHmmss"), module,
                LogLevelHelper.GetLevelString(level),
                message.RemoveRN(),
                args.Length > 0 ? string.Format(format, args).RemoveRN() : format.RemoveRN());
        }

        public void Dispose()
        {
            if (_instance != null)
            {
                _instance.Dispose();
                _instance = null;
            }
        }
    }
    internal static class StringEx
    {
        internal static string RemoveRN(this string message)
        {
            if (message == null)
                return "";
            return message.Replace('\r', ' ').Replace('\n', ' ');
        }
    }

    internal class LogToStreamManage : ManageBaseForLog
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
            {
                writer.Flush();
                _fileStream.Flush();
                Trace.Flush();
            }
        }

        protected override void DoDispose()
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

        protected override void Flush()
        {
            writer.Flush();
            _fileStream.Flush();
            Trace.Flush();
        }
    }

    internal abstract class ManageBaseForLog : IDisposable
    {
        private ConcurrentQueue<string> msgQueue = new ConcurrentQueue<string>();
        private AutoResetEvent _logEvent = new AutoResetEvent(false);
        private AutoResetEvent _endEvent = new AutoResetEvent(false);
        private AutoResetEvent _endedEvent;
        private AutoResetEvent[] events;
        private object _lockObj = new object();
        private System.Threading.Thread _logThread;
        protected ManageBaseForLog(string threadName)
        {
            events = new AutoResetEvent[2] { _logEvent, _endEvent };
            _logThread = new System.Threading.Thread(InternalWriteLog);
            _logThread.Name = threadName;
            _logThread.Start();
        }

        public void WriteToLog(string msg)
        {
            Trace.WriteLine(msg);
            msgQueue.Enqueue(msg);
            using (_lockObj.LockWhile(() =>
            {
                if (_logEvent != null)
                {
                    _logEvent.Set();
                }
            }))
            { }
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
                            System.Threading.Thread.Sleep(5000);
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
                            try
                            {
                                DoWriteLog("Log end.");
                                isBreak = true;
                            }
                            catch (Exception e)
                            {
                                System.Diagnostics.Trace.TraceError(e.GetExceptionDetail());
                            }
                        }
                        if (result == WaitHandle.WaitTimeout)
                        {
                            try
                            {
                                Flush();
                            }
                            catch (Exception ex1)
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
            using (_lockObj.LockWhile(() =>
            {
                if (_logEvent != null)
                {
                    _logEvent.Dispose();
                    _logEvent = null;
                }
                if (_endEvent != null)
                {
                    _endEvent.Dispose();
                    _endEvent = null;
                }

            }))
            { }
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
            _endedEvent = null;
            DoDispose();
            Debug.WriteLine("Manager Disposed");
            Trace.Flush();
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
    //    private System.Threading.Thread _thread;

    //    public readonly object SyncLockObj = new object();
    //    private readonly Dictionary<Guid, DisposableObj<Stream>> _allStream = new Dictionary<Guid, DisposableObj<Stream>>();
    //    private readonly Dictionary<Guid, DisposableObj<IDisposable>> _allDisposableObj = new Dictionary<Guid, DisposableObj<IDisposable>>();

    //    private LogThreadManager()
    //    {
    //        _thread = new System.Threading.Thread(Run);
    //        _thread.Name = "ManagerLogStream";
    //        _thread.Start();
    //    }

    //    private const int ExpireSecond = 120 * 1000;
    //    private void Run()
    //    {
    //        // todo if process stop, this must be disposed. So we can use sleep.
    //        System.Threading.Thread.Sleep(120 * 1000);
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

    //public class DefaultLogStream : ILogStreamProvider
    //{
    //    public DefaultLogStream() { }
    //    public DefaultLogStream(string logPath)
    //    {
    //        _logPath = LogPath;
    //    }

    //    public static string GetLogPath(string logFileName)
    //    {
    //        return Path.Combine(GetSystemLogFolder(), logFileName);
    //    }

    //    public static string GetSystemLogFolder()
    //    {
    //        string logFolder = CloudConfig.Instance.LogPath;
    //        if (string.IsNullOrEmpty(logFolder))
    //        {
    //            logFolder = AppDomain.CurrentDomain.BaseDirectory;
    //            logFolder = Path.Combine(logFolder, "Log");
    //        }
    //        if (!Directory.Exists(logFolder))
    //        {
    //            Directory.CreateDirectory(logFolder);
    //        }
    //        return logFolder;
    //    }

    //    private string _logPath;
    //    protected virtual string LogPath
    //    {
    //        get
    //        {
    //            if (string.IsNullOrEmpty(_logPath))
    //            {
    //                string logFolder = GetSystemLogFolder();
    //                var logPath = Path.Combine(logFolder, LogFileName);
    //                _logPath = logPath;

    //            }
    //            return _logPath;
    //        }
    //    }

    //    protected virtual string LogFileName
    //    {
    //        get
    //        {
    //            return string.Format("{0}.txt", DateTime.Now.ToString("yyyyMMdd"));
    //        }
    //    }

    //    private readonly Guid _streamKey = Guid.NewGuid();


    //    public Guid StreamId
    //    {
    //        get
    //        {
    //            return _streamKey;
    //        }
    //    }

    //    public void Write(string information)
    //    {
    //        var writer = new StreamWriter(LogStream);
    //        writer.Write(information);
    //        writer.Flush();
    //    }

    //    public void WriteLine(string information)
    //    {
    //        var writer = new StreamWriter(LogStream);
    //        writer.WriteLine(information);
    //        writer.Flush();
    //    }

    //    public string GetTotalLog(DateTime date)
    //    {
    //        if (File.Exists(LogPath))
    //            using (var stream = new FileStream(LogPath, FileMode.Open, FileAccess.Read))
    //            {
    //                StreamReader reader = new StreamReader(stream);
    //                return reader.ReadToEnd();
    //            }
    //        return "Log file is not exist.";
    //    }
    //}


    //public class DebugOutput : ILogStreamProvider
    //{
    //    public Guid StreamId
    //    {
    //        get
    //        {
    //            return _id;
    //        }
    //    }

    //    private Guid _id = Guid.NewGuid();
    //    private object _syncObj = new object();
    //    public object SyncObj
    //    {
    //        get
    //        {
    //            return _syncObj;
    //        }
    //    }

    //    public string GetTotalLog(DateTime date)
    //    {
    //        return "";
    //    }

    //    public void Write(string information)
    //    {
    //        Debug.Write(information);
    //    }

    //    public void WriteLine(string information)
    //    {
    //        Debug.WriteLine(information);
    //    }
    //}
}
