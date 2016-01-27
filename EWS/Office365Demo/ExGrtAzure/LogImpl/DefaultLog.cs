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
                if(string.IsNullOrEmpty(_logPath))
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
                return string.Format("{0}.txt", DateTime.Now.ToString("yyyyMMddHHmmss"));
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
                if(val == null)
                {
                    val = new FileStream(LogPath, FileMode.Append);
                    LogThreadManager.Instance.AddStream(val, _streamKey);
                }
                val.Flush();
                return val;
            }
        }

        public void WriteException(LogLevel level, string message, Exception exception, string exMsg)
        {
            lock(LogThreadManager.Instance.SyncLockObj)
            {
                const string blank = "    ";

                var writer = new StreamWriter(LogStream);
                writer.WriteLine(string.Join(blank, DateTime.Now.ToString("yyyyMMddHHmmss"),
                    LogLevelHelper.GetLevelString(level),
                    message,
                    exception.Message,
                    exception.StackTrace.Replace("\r", "    ").Replace("\n", "    ")));
            }
        }

        public void WriteLog(LogLevel level, string message)
        {
            lock (LogThreadManager.Instance.SyncLockObj)
            {
                const string blank = "    ";

                var writer = new StreamWriter(LogStream);
                writer.WriteLine(string.Join(blank, DateTime.Now.ToString("yyyyMMddHHmmss"),
                    LogLevelHelper.GetLevelString(level),
                    message));
            }
        }

        public void WriteLog(LogLevel level, string message, string format, params object[] args)
        {
            lock (LogThreadManager.Instance.SyncLockObj)
            {
                const string blank = "    ";

                var writer = new StreamWriter(LogStream);
                writer.WriteLine(string.Join(blank, DateTime.Now.ToString("yyyyMMddHHmmss"),
                    LogLevelHelper.GetLevelString(level),
                    message,
                    args.Length >0 ? string.Format(format, args) : format));
            }
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
                if(_instance == null)
                {
                    lock (instanceLockObj)
                    {
                        if(_instance == null)
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

                Dispose(_allStream, now, (m)  => { m.Close(); m.Dispose(); });
                Dispose(_allDisposableObj, now, m => m.Dispose());
            }
        }

        private delegate void DisposeCallFunc<T>(T obj);
        private void Dispose<T>(Dictionary<Guid,DisposableObj<T>> objects, DateTime now, DisposeCallFunc<T> func)
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
            lock(SyncLockObj)
            {
                _allStream.Add(key, new DisposableObj<Stream>() { obj = stream, addTime = DateTime.Now});
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
            lock(SyncLockObj)
            {
                DisposableObj<Stream> val;
                if(_allStream.TryGetValue(key, out val))
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
