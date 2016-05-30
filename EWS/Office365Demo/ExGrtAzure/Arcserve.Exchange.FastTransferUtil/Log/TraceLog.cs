using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace FTStreamParse.Log
{
    public class TraceLog : ITraceLog
    {
        public void WriteError(string message)
        {
            Trace.TraceError(message);
        }

        public void WriteError(string format, params object[] args)
        {
            Trace.TraceError(format, args);   
        }

        public void WriteError(int hResult, string mesesage)
        {
            Trace.TraceError(string.Format("HResult:{0}, Details:{1}", hResult.ToString("X8"), mesesage));
        }

        public void WriteError(int hResult, string format, params object[] args)
        {
            Trace.TraceError(string.Format("HResult:{0}, Details:{1}", hResult.ToString("X8"), format), args);
        }

        public void WriteException(Exception ex)
        {
            Trace.TraceError("The exception is :");
            Trace.Indent();
            Trace.TraceError(string.Format("Exception message is {0}.", ex.Message));
            Trace.TraceError(string.Format("Exception's stacktrace is {0}.", ex.StackTrace));
            Trace.Unindent();
        }

        public void WriteException(Exception ex, string format, params object[] args)
        {
            Trace.TraceError(string.Format("{0}, the exception is :", format), args);
            Trace.Indent();
            Trace.TraceError(string.Format("Exception message is {0}.", ex.Message));
            Trace.TraceError(string.Format("Exception's stacktrace is {0}.", ex.StackTrace));
            Trace.Unindent();
        }

        public void WriteWarning(string message)
        {
            Trace.TraceWarning(message);
        }

        public void WriteWarning(string format, params object[] args)
        {
            Trace.TraceWarning(format, args);   
        }

        public void WriteInformation(string message)
        {
            Trace.TraceInformation(message);
        }

        public void WriteInformation(string format, params object[] args)
        {
            Trace.TraceInformation(format, args);   
        }

        public void WriteDebug(string message)
        {
            Debug.WriteLine(message);
        }

        public void WriteDebug(string format, params object[] args)
        {
            Debug.WriteLine(format, args);   
        }

        public void Indent()
        {
            Trace.Indent();
        }

        public void Unindent()
        {
            Trace.Unindent();
        }

        public int IndentLevel
        {
            get
            {
                return Trace.IndentLevel;
            }
            set
            {
                Trace.IndentLevel = value;
            }
        }

        public int IndentSize
        {
            get
            {
                return Trace.IndentSize;
            }
            set
            {
                Trace.IndentSize = value;
            }
        }
    }
}
