using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTStreamParse.Log
{
    public interface ITraceLog
    {
        void WriteError(string message);
        void WriteError(string format, params object[] args);
        void WriteError(int hResult, string mesesage);
        void WriteError(int hResult, string format, params object[] args);
        void WriteException(Exception ex);
        void WriteException(Exception ex, string format, params object[] args);
        void WriteWarning(string message);
        void WriteWarning(string format, params object[] args);
        void WriteInformation(string message);
        void WriteInformation(string format, params object[] args);
        void WriteDebug(string message);
        void WriteDebug(string format, params object[] args);
    }
}
