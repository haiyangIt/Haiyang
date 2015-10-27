using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExGrtOnline.Log
{
    public enum LogLevel : byte
    {
        COM = 0,
        ERR = 1,
        WARN = 2,
        INFO = 3,
        DEBUG = 4
    }
}