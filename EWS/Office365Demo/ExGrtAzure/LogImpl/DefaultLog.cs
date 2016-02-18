using LogInterface;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LogImpl
{
    public class DefaultLog : ILog
    {
        private string _logPath;
        protected virtual string LogPath
        {
            get
            {
                if (string.IsNullOrEmpty(_logPath))
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
                    var logPath = Path.Combine(logFolder, LogFileName);
                    _logPath = logPath;

                }
                return _logPath;
            }
        }

        protected virtual string LogFileName
        {
            get
            {
                return string.Format("{0}.txt", DateTime.Now.ToString("yyyyMMdd"));
            }
        }


        public DefaultLog()
        {

        }


        private readonly Guid _streamKey = Guid.NewGuid();

        private Stream LogStream
        {
            get
            {
                var val = LogThreadManager.Instance.GetStream(_streamKey);
                if (val == null)
                {
                    val = new FileStream(LogPath, FileMode.Append, FileAccess.Write, FileShare.Read);
                    LogThreadManager.Instance.AddStream(val, _streamKey);
                }
                val.Flush();
                return val;
            }
        }

        public void WriteException(LogLevel level, string message, Exception exception, string exMsg)
        {
            lock (LogThreadManager.Instance.SyncLockObj)
            {
                var writer = new StreamWriter(LogStream);
                writer.WriteLine(GetExceptionString(level, message, exception, exMsg));
                writer.Flush();
            }
        }
        const string blank = "\t";

        internal static string GetExceptionString(LogLevel level, string message, Exception exception, string exMsg)
        {
            StringBuilder sb = new StringBuilder();
            var curEx = exception;
            while (curEx != null)
            {
                sb.AppendLine(string.Join(blank, DateTime.Now.ToString("yyyyMMddHHmmss"),
                    LogLevelHelper.GetLevelString(level),
                    message.RemoveRN(),
                    curEx.Message.RemoveRN(),
                    curEx.StackTrace.RemoveRN()));


                curEx = curEx.InnerException;
            }
            sb.AppendLine();
            return sb.ToString();
        }

        public void WriteLog(LogLevel level, string message)
        {
            lock (LogThreadManager.Instance.SyncLockObj)
            {
                var writer = new StreamWriter(LogStream);
                writer.WriteLine(GetLogString(level, message));
                writer.Flush();
            }
        }

        internal static string GetLogString(LogLevel level, string message)
        {
            return string.Join(blank, DateTime.Now.ToString("yyyyMMddHHmmss"),
                LogLevelHelper.GetLevelString(level),
                message.RemoveRN());
        }

        public void WriteLog(LogLevel level, string message, string format, params object[] args)
        {
            lock (LogThreadManager.Instance.SyncLockObj)
            {
                var writer = new StreamWriter(LogStream);
                writer.WriteLine(GetLogString(level, message, format, args));
                writer.Flush();
            }
        }

        internal static string GetLogString(LogLevel level, string message, string format, params object[] args)
        {
            return string.Join(blank, DateTime.Now.ToString("yyyyMMddHHmmss"),
                LogLevelHelper.GetLevelString(level),
                message.RemoveRN(),
                args.Length > 0 ? string.Format(format, args).RemoveRN() : format.RemoveRN());
        }

        public string GetTotalLog(DateTime date)
        {
            if (File.Exists(LogPath))
                using (var stream = new FileStream(LogPath, FileMode.Open, FileAccess.Read))
                {
                    StreamReader reader = new StreamReader(stream);
                    return reader.ReadToEnd();
                }
            else
                return "Log file is not exist.";
        }
    }
    internal static class StringEx
    {
        internal static string RemoveRN(this string message)
        {
            return message.Replace('\r', ' ').Replace('\n', ' ');
        }
    }

    public class LogThreadManager
    {
        private static object instanceLockObj = new object();
        private static LogThreadManager _instance;
        public static LogThreadManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (instanceLockObj)
                    {
                        if (_instance == null)
                        {
                            _instance = new LogThreadManager();
                        }
                    }
                }
                return _instance;
            }
        }
        private static object _lockObj = new object();
        private Thread _thread;

        public readonly object SyncLockObj = new object();
        private readonly Dictionary<Guid, DisposableObj<Stream>> _allStream = new Dictionary<Guid, DisposableObj<Stream>>();
        private readonly Dictionary<Guid, DisposableObj<IDisposable>> _allDisposableObj = new Dictionary<Guid, DisposableObj<IDisposable>>();

        private LogThreadManager()
        {
            _thread = new Thread(Run);
            _thread.Name = "ManagerLogStream";
            _thread.Start();
        }

        private const int ExpireSecond = 120 * 1000;
        private void Run()
        {
            Thread.Sleep(120 * 1000);
            lock (SyncLockObj)
            {
                DateTime now = DateTime.Now;

                Dispose(_allStream, now, (m) => { m.Close(); m.Dispose(); });
                Dispose(_allDisposableObj, now, m => m.Dispose());
            }
        }

        private delegate void DisposeCallFunc<T>(T obj);
        private void Dispose<T>(Dictionary<Guid, DisposableObj<T>> objects, DateTime now, DisposeCallFunc<T> func)
        {
            List<Guid> expireStreams = new List<Guid>(4);
            foreach (var keyValue in objects)
            {
                if (keyValue.Value.IsExpire(now, ExpireSecond))
                {
                    expireStreams.Add(keyValue.Key);
                }
            }

            foreach (var expireItem in expireStreams)
            {
                func.Invoke(objects[expireItem].obj);
                objects.Remove(expireItem);
            }
        }

        public void AddStream(Stream stream, Guid key)
        {
            lock (SyncLockObj)
            {
                _allStream.Add(key, new DisposableObj<Stream>() { obj = stream, addTime = DateTime.Now });
            }
        }

        public void AddDisposal(IDisposable disObj, Guid key)
        {
            lock (SyncLockObj)
            {
                _allDisposableObj.Add(key, new DisposableObj<IDisposable>() { obj = disObj, addTime = DateTime.Now });
            }
        }

        public Stream GetStream(Guid key)
        {
            lock (SyncLockObj)
            {
                DisposableObj<Stream> val;
                if (_allStream.TryGetValue(key, out val))
                {
                    return val.obj;
                }
                return null;
            }
        }


        class DisposableObj<T>
        {
            public T obj;
            public DateTime addTime;

            public bool IsExpire(DateTime now, int expireSecond)
            {
                if ((now - addTime).TotalSeconds > expireSecond)
                    return true;
                return false;
            }
        }
    }
}
