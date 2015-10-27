using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LogInterface
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
        private static Dictionary<LogLevel, string> _Dic;
        private static Dictionary<LogLevel, string> Dic
        {
            get
            {
                if(_Dic == null)
                {
                    _Dic = new Dictionary<LogLevel, string>(8);
                    _Dic[LogLevel.COM] = "C";
                    _Dic[LogLevel.ERR] = "E";
                    _Dic[LogLevel.WARN] = "W";
                    _Dic[LogLevel.INFO] = "I";
                    _Dic[LogLevel.DEBUG] = "D";
                }
                return _Dic;
            }
        }
        public static string GetLevelString(LogLevel level)
        {
            return Dic[level];
        }
    }
}