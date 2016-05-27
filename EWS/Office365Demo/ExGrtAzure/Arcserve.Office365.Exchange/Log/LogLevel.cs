using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Arcserve.Office365.Exchange.Log
{
    public enum LogLevel : byte
    {
        COM = 0,
        ERR = 1,
        WARN = 2,
        INFO = 3,
        DEBUG = 4
    }

    public class LogLevelHelper
    {
        static LogLevelHelper()
        {
            Dic = new Dictionary<LogLevel, string>(8);
            Dic[LogLevel.COM] = "C";
            Dic[LogLevel.ERR] = "E";
            Dic[LogLevel.WARN] = "W";
            Dic[LogLevel.INFO] = "I";
            Dic[LogLevel.DEBUG] = "D";
        }
        private static readonly Dictionary<LogLevel, string> Dic;
        public static string GetLevelString(LogLevel level)
        {
            return Dic[level];
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
                        curEx.GetType().FullName,
                        curEx.Message,
                        curEx.HResult.ToString("X8"),
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
                ex.GetType().FullName,
                    ex.Message,
                    ex.HResult.ToString("X8"),
                    ex.StackTrace));

            foreach (var innerEx in ex.Flatten().InnerExceptions)
            {
                sb.AppendLine(GetExceptionString(ex));
            }
            return sb.ToString();
        }
    }
}