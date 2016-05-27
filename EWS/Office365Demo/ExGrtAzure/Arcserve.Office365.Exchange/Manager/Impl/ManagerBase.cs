using Arcserve.Office365.Exchange.Log;
using Arcserve.Office365.Exchange.Manager.IF;
using Arcserve.Office365.Exchange.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Manager.Impl
{

    public abstract class ManagerBase : IManager
    {
        public abstract string ManagerName { get; }

        private AutoResetEvent endEvent = new AutoResetEvent(false);
        private AutoResetEvent endWaitingEvent = new AutoResetEvent(false);

        private object _lockObj = new object();
        private AutoResetEvent[] events;
        private int EndEventIndex;
        private const int WaitTimeOut = 2000;
        private int isExited = 0;

        protected ManagerBase(int otherEventCount = 1)
        {
            if (otherEventCount <= 0)
                throw new ArgumentException();

            events = new AutoResetEvent[otherEventCount + 1];
            for (int i = 0; i < otherEventCount; i++)
            {
                AutoResetEvent ev = new AutoResetEvent(false);
                events[i] = ev;
            }

            events[otherEventCount] = endEvent; // related AddEventIndex, EndEventIndex;
            EndEventIndex = otherEventCount;
        }

        protected void InternalThread(IArcJob job)
        {
            bool isLoop = true;
            while (isLoop)
            {
                try
                {
                    var waitResult = WaitHandle.WaitAny(events, WaitTimeOut); // waittimeout: loop to try get the new thread.

                    if (waitResult == EndEventIndex)
                    {
                        isLoop = false;

                    }
                    else if (waitResult >= 0 && waitResult < EndEventIndex)
                    {
                        MethodWhenOtherEventTriggered(waitResult);
                    }
                    else if (waitResult == WaitHandle.WaitTimeout)
                    {
                        MethodWhenTimeOutTriggerd();
                    }
                    else
                    {
                        throw new NotSupportedException("Invalid wait result");
                    }
                }
                catch (Exception e)
                {
                    LogFactory.LogInstance.WriteException(ManagerName, LogLevel.ERR, "Internal loop exception", e, e.Message);
                }
            }
            endWaitingEvent.Set();
        }

        protected abstract void MethodWhenOtherEventTriggered(int eventIndex = 0);

        protected abstract void MethodWhenTimeOutTriggerd();

        protected bool CheckOtherEventCanExecute()
        {
            bool isInEnding = false;
            using (_lockObj.LockWhile(() =>
            {
                isInEnding = isExited > 0;
            }))
            { }

            return !isInEnding;
            //if (isInEnding)
            //{
            //    //throw new ThreadStateException(ManagerName + " exited, can't execute.");
            //}
        }

        protected void TriggerOtherEvent(int eventIndex)
        {
            if (eventIndex < 0 || eventIndex > EndEventIndex)
            {
                throw new ArgumentException("eventIndex is invalid.");
            }
            using (_lockObj.LockWhile(() =>
            {
                if (events[eventIndex] != null)
                    events[eventIndex].Set();
            }))
            { }
        }

        protected virtual void BeforeEnd() { }
        protected virtual void AfterEnd() { }

        public void End()
        {
            bool isInEnding = false;
            using (_lockObj.LockWhile(() =>
            {
                if (isExited > 0)
                    isInEnding = true;
                isExited++;
            }))
            { }

            if (isInEnding)
            {
                return;
            }

            LogFactory.LogInstance.WriteLog(ManagerName, LogLevel.DEBUG, string.Format("Manager [{0}] ending.", ManagerName));

            BeforeEnd();
            endEvent.Set();
            endWaitingEvent.WaitOne();

            using (_lockObj.LockWhile(() =>
            {
                int i = 0;
                foreach (var ev in events)
                {
                    ev.Dispose();
                    events[i++] = null;
                }

                endWaitingEvent.Dispose();
                endWaitingEvent = null;
                endEvent = null;
            }))
            { }
            AfterEnd();

            LogFactory.LogInstance.WriteLog(ManagerName, LogLevel.DEBUG, string.Format("Manager [{0}] ended.", ManagerName));
        }

        protected virtual void BeforeStart() { }
        protected virtual void AfterStart() { }

        private bool IsStart = false;
        public void Start()
        {
            bool isGoOn = true;
            using (_lockObj.LockWhile(() =>
             {
                 if (IsStart)
                 {
                     isGoOn = false;
                 }
                 IsStart = true;
             }))
            { }
            if (!isGoOn)
                return;
            if (LogFactory.IsInited)
                LogFactory.LogInstance.WriteLog(ManagerName, LogLevel.DEBUG, string.Format("Manager [{0}] starting.", ManagerName));
            else
                Trace.TraceInformation(string.Format("Manager [{0}] starting.", ManagerName));

            BeforeStart();
            var internalRunning = JobFactoryServer.Instance.ThreadManager.StartThread("Thread" + ManagerName);
            var jobManagerInternalJob = new ArcSystemJob(InternalThread, ManagerName + "Job");
            internalRunning.Run(jobManagerInternalJob);
            AfterStart();

            if (LogFactory.IsInited)
                LogFactory.LogInstance.WriteLog(ManagerName, LogLevel.DEBUG, string.Format("Manager [{0}] started.", ManagerName));
            else
                Trace.TraceInformation(string.Format("Manager [{0}] started.", ManagerName));
        }

        public virtual void Dispose()
        {
            End();
        }
    }
}
