using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTStreamParse.Log
{
    public interface ITraceLog
    {
        public void WriteError(string message);
        public void WriteError(string format, params object[] args);
        public void WriteError(int hResult, string mesesage);
        public void WriteError(int hResult, string format, params object[] args);
        public void WriteException(Exception ex);
        public void WriteException(Exception ex, string format, params object[] args);
        public void WriteWarning(string message);
        public void WriteWarning(string format, params object[] args);
        public void WriteInformation(string message);
        public void WriteInformation(string format, params object[] args);
        public void WriteDebug(string message);
        public void WriteDebug(string format, params object[] args);
    }
}
