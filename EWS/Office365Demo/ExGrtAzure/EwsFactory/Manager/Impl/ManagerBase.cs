using EwsFrame.Manager.IF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EwsFrame.Manager.Impl
{

    public abstract class ManagerBase : IManager
    {
        public abstract string ManagerName { get; }

        private AutoResetEvent endEvent = new AutoResetEvent(false);
        private AutoResetEvent endWaitingEvent = new AutoResetEvent(false);


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
                var waitResult = WaitHandle.WaitAny(events, WaitTimeOut); // waittimeout: loop to try get the new thread.

                if (waitResult == EndEventIndex)
                {
                    isLoop = false;

                }
                else if (waitResult >= 0 && waitResult < EndEventIndex)
                {
                    MethodWhenOtherEventTriggered(waitResult);
                }
                else if(waitResult == WaitHandle.WaitTimeout)
                {
                    MethodWhenTimeOutTriggerd();
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            endWaitingEvent.Set();
        }

        protected abstract void MethodWhenOtherEventTriggered(int eventIndex = 0);

        protected abstract void MethodWhenTimeOutTriggerd();

        protected void CheckOtherEventCanExecute()
        {
            if (Interlocked.CompareExchange(ref isExited, 1, 1) == 1)
            {
                throw new ThreadStateException(ManagerName + " exited, can't execute.");
            }
        }

        protected void TriggerOtherEvent(int eventIndex)
        {
            if(eventIndex < 0 || eventIndex > EndEventIndex)
            {
                throw new ArgumentException("eventIndex is invalid.");
            }
            events[eventIndex].Set();
        }

        protected virtual void BeforeEnd() { }
        protected virtual void AfterEnd() { }

        public void End()
        {
            BeforeEnd();
            Interlocked.Exchange(ref isExited, 1);
            endEvent.Set();
            endWaitingEvent.WaitOne();

            foreach(var ev in events)
            {
                ev.Dispose();
            }

            endWaitingEvent.Dispose();
            AfterEnd();
        }

        protected virtual void BeforeStart() { }
        protected virtual void AfterStart() { }

        public void Start()
        {
            BeforeStart();
            var internalRunning = JobFactory.Instance.ThreadManager.StartThread(ManagerName);
            var jobManagerInternalJob = new ArcSystemJob(InternalThread, ManagerName + "Job");
            internalRunning.Run(jobManagerInternalJob);
            AfterStart();
        }
    }
}
