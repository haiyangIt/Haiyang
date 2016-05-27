using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace FTStreamParse.Log
{
    public class TraceSourceLog : ITraceLog
    {
        private readonly static TraceSource _traceSource = new TraceSource("ArcserveTrace");
        public void WriteError(string message)
        {
            _traceSource.TraceEvent(TraceEventType.Error, 0, message);
        }

        public void WriteError(string format, params object[] args)
        {
            _traceSource.TraceEvent(TraceEventType.Error, 0, format, args);
        }

        public void WriteError(int hResult, string mesesage)
        {
            _traceSource.TraceEvent(TraceEventType.Error, hResult, mesesage);
        }

        public void WriteError(int hResult, string format, params object[] args)
        {
            _traceSource.TraceEvent(TraceEventType.Error, hResult, format, args);
        }

        public void WriteException(Exception ex)
        {
            _traceSource.TraceEvent(TraceEventType.Critical, 0, "Exception message:{0}, stacktrace:{1}.", ex.Message, ex.StackTrace);
        }

        public void WriteException(Exception ex, string format, params object[] args)
        {
            _traceSource.TraceEvent(TraceEventType.Critical, 0, string.Format("message:{0}. Exception message:{1}, stacktrace:{2}.", format, ex.Message, ex.StackTrace), args);
        }

        public void WriteWarning(string message)
        {
            _traceSource.TraceEvent(TraceEventType.Warning, 0, message);
        }

        public void WriteWarning(string format, params object[] args)
        {
            _traceSource.TraceEvent(TraceEventType.Warning, 0, format, args);
        }

        public void WriteInformation(string message)
        {
            _traceSource.TraceEvent(TraceEventType.Information, 0, message);
        }

        public void WriteInformation(string format, params object[] args)
        {
            _traceSource.TraceEvent(TraceEventType.Information, 0, format, args);
        }

        public void WriteDebug(string message)
        {
            _traceSource.TraceEvent(TraceEventType.Verbose, 0, message);
        }

        public void WriteDebug(string format, params object[] args)
        {
            _traceSource.TraceEvent(TraceEventType.Verbose, 0, format, args);
        }
    }
}
