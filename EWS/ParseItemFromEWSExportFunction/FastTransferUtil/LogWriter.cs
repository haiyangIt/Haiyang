using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;

namespace FTStreamUtil
{
    public class LogWriter : IDisposable
    {
        private static LogWriter _instance = null;

        public static LogWriter Instance
        {
            get
            {
                if (_instance == null)
                {
                    {
                        _instance = new LogWriter();
                    }
                }
                return _instance;
            }
        }

        private Timer _timer;
        private LogWriter()
        {
            //try
            //{
            //    var logFile = FTStreamUtil.Build.Implement.BuildConst.LogFileName;
            //    Debug.WriteLine(logFile);

            //    _logwriter = new StreamWriter(FTStreamUtil.Build.Implement.BuildConst.LogFileName, true);
            //    Debug.WriteLine("Create logwriter success.");
            //    _timer = new Timer(30 * 1000);
            //    _timer.Elapsed += _timer_Elapsed;
            //    _timer.Enabled = true;
            //    _timer.Start();
            //}
            //catch (Exception e)
            //{
            //    Debug.WriteLine(e.Message);
            //    Debug.WriteLine(e.StackTrace);
            //}
        }

        void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (_logwriter != null)
                    _logwriter.Flush();
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.GetExceptionDetail());
            }
        }

        #region Output Log
        private int _indentForLog = 0;
        private Dictionary<int, string> _indentString = new Dictionary<int, string>();
        public void IncrementIndent()
        {
            _indentForLog += 4;
        }

        public void ResetIndent()
        {
            _indentForLog -= 4;
        }

        public StringBuilder OutputStringBuilder;

        public string GetIndent()
        {
            string indentStr;
            if (!_indentString.TryGetValue(_indentForLog, out indentStr))
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < _indentForLog; i++)
                {
                    sb.Append(" ");
                }

                indentStr = sb.ToString();
                _indentString.Add(_indentForLog, indentStr);
            }

            return indentStr;
        }
        private StreamWriter _logwriter;
        public void Write(string msg)
        {
            try
            {
                if (_logwriter != null)
                {
                    _logwriter.Write(msg);
                }
                else
                {
                    Debug.Write(msg);
                    //Console.Write(msg);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(e.GetExceptionDetail());
            }
        }

        public void WriteLine(string msg)
        {
            try
            {
                if (_logwriter != null)
                {
                    _logwriter.WriteLine(msg);
                }
                else
                {
                    Debug.WriteLine(msg);
                    //Console.WriteLine(msg);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(e.GetExceptionDetail());
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }
        }

        public void WriteException(object obj, Exception ex)
        {
            try
            {
                if (_logwriter != null)
                {
                    _logwriter.WriteLine("-----------------Exception:-----------------------");
                    _logwriter.Write("type:");
                    _logwriter.WriteLine(obj.GetType().FullName);
                    _logwriter.WriteLine(ex.Message);
                    _logwriter.WriteLine(ex.StackTrace);
                }
                else
                {
                    Debug.WriteLine(obj.GetType().FullName);
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine(ex.StackTrace);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(e.GetExceptionDetail());
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }
        }

        public void Flush()
        {
            try
            {
                if (_logwriter != null)
                    _logwriter.Flush();
            }
            catch (Exception e)
            {
                Trace.TraceError(e.GetExceptionDetail());
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }
        }
        #endregion



        public void Dispose()
        {
            try
            {
                if (_timer != null)
                {
                    _timer.Stop();
                    _timer.Close();
                    _timer = null;
                }
                if (_logwriter != null)
                {
                    _logwriter.Close();
                    _logwriter = null;
                }
                _instance = null;
            }
            catch (Exception e)
            {
                Trace.TraceError(e.GetExceptionDetail());
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }
        }
    }

    public static class LogExtension
    {
        public static string GetExceptionDetail(this Exception ex)
        {
            return GetExceptionString(ex);
        }

        public static string GetExceptionString(Exception exception)
        {
            StringBuilder sb = new StringBuilder();
            var curEx = exception;
            while (curEx != null)
            {
                if (curEx is AggregateException)
                {
                    sb.AppendLine(GetAggrateException(curEx as AggregateException));
                }
                else
                {
                    sb.AppendLine(string.Join("  ",
                        curEx.Message,
                        curEx.StackTrace));

                    curEx = curEx.InnerException;
                }
            }
            sb.AppendLine();
            return sb.ToString();
        }

        internal static string GetAggrateException(AggregateException ex)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Join("  ",
                    ex.Message,
                    ex.StackTrace));

            foreach (var innerEx in ex.Flatten().InnerExceptions)
            {
                sb.AppendLine(GetExceptionString(ex));
            }
            return sb.ToString();
        }
    }
}
