using EwsFrame.Manager.IF;
using LogInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EwsFrame.Manager.Impl
{
    internal class ThreadManager : IThreadManager
    {
        internal ThreadManager() { }

        Dictionary<string, IThreadObj> _allThread = new Dictionary<string, IThreadObj>();
        // todo public int MaxThreadCount = 10;

        public string ManagerName
        {
            get
            {
                return "ThreadManager";
            }
        }

        public void Start()
        {
        }

        public void End()
        {
            LogFactory.LogInstance.WriteLog(ManagerName, LogInterface.LogLevel.DEBUG, string.Format("Manager [{0}] ending.", ManagerName));
            if (_allThread.Count > 0)
            {
                LogFactory.LogInstance.WriteLog(ManagerName, LogInterface.LogLevel.DEBUG, string.Format("Manager [{0}] ending after [{1}] thread ended.", ManagerName, _allThread.Count));
                AutoResetEvent[] allAutoResultEvent = new AutoResetEvent[_allThread.Count];
                int index = 0;
                foreach (var thread in _allThread)
                {
                    var autoResultEvent = new AutoResetEvent(false);
                    allAutoResultEvent[index] = autoResultEvent;
                    thread.Value.EndThread(autoResultEvent);
                    index++;
                }
                WaitHandle.WaitAll(allAutoResultEvent);
                foreach (var e in allAutoResultEvent)
                {
                    e.Dispose();
                }
            }
            LogFactory.LogInstance.WriteLog(ManagerName, LogInterface.LogLevel.DEBUG, string.Format("Manager [{0}] ended.", ManagerName));
        }

        public IThreadObj NewThread()
        {
            return NewThread(string.Empty);
        }

        public IThreadObj NewThread(string threadName)
        {

            // todo if any thread is idle, can return the thread.
            // now create new as a temp solution.
            lock (_allThread)
            {
                LogFactory.LogInstance.WriteLog(ManagerName, LogInterface.LogLevel.DEBUG, string.Format("Manager [{0}] new thread with name [{1}].", ManagerName, threadName));
                var obj = JobFactoryServer.Instance.NewThreadObj(threadName);
                _allThread.Add(obj.ThreadName, obj);
                LogFactory.LogInstance.WriteLog(ManagerName, LogInterface.LogLevel.DEBUG, string.Format("Manager [{0}] new thread finished with name [{1}].", ManagerName, obj.ThreadName));
                return obj;
            }

        }
        public IThreadObj StartThread(string threadName)
        {
            lock (_allThread)
            {
                LogFactory.LogInstance.WriteLog(ManagerName, LogInterface.LogLevel.DEBUG, string.Format("Manager [{0}] start thread with name [{1}].", ManagerName, threadName));
                var obj = JobFactoryServer.Instance.NewThreadObj(threadName);
                _allThread.Add(obj.ThreadName, obj);
                LogFactory.LogInstance.WriteLog(ManagerName, LogInterface.LogLevel.DEBUG, string.Format("Manager [{0}] start thread finished with name [{1}].", ManagerName, obj.ThreadName));
                return obj;
            }
        }
    }

    public class ThreadObj : IThreadObj
    {
        private Thread _thread;
        private AutoResetEvent _runEvent = new AutoResetEvent(false);
        private AutoResetEvent _endEvent = new AutoResetEvent(false);
        private const int _jobEventIndex = 0;
        private const int _endEventIndex = 1;
        private readonly object _sync = new object();
        private AutoResetEvent[] _events;
        private IArcJob _job;
        private string _threadId;
        public string ThreadName { get { return _threadId; } }

        private ArcThreadState _state;
        public ArcThreadState State
        {
            get
            {
                return _state;
            }
        }

        public ThreadObj() : this(string.Empty)
        {

        }

        public ThreadObj(string threadName)
        {
            _events = new AutoResetEvent[2] { _runEvent, _endEvent };
            _thread = new Thread(InternalRun);

            _state = ArcThreadState.Idle;
            if (string.IsNullOrEmpty(threadName))
            {
                _threadId = Guid.NewGuid().ToString();
            }
            else
                _threadId = threadName;

            _thread.Name = threadName;
            _thread.Start();

        }
        public void CancelThread()
        {
            DoJob(_job, Operator.Cancel);
        }

        public void Run(IArcJob job)
        {
            DoJob(job, Operator.Run);
        }

        public void InternalRun()
        {
            while (true)
            {
                var result = WaitHandle.WaitAny(_events);
                if (result == 0)
                {
                    if (_state == ArcThreadState.Running)
                    {
                        LogFactory.LogInstance.WriteLog(ThreadName, LogLevel.DEBUG, "thread job run.", "Job [{0}] in thread [{1}] run.", _job.JobId, ThreadName);
                        try
                        {
                            _job.Run();
                        }
                        catch (Exception ex)
                        {
                            LogFactory.LogInstance.WriteException(ThreadName, LogLevel.ERR, string.Format("Job [{0}] in thread [{1}] run exception.", _job.JobId, ThreadName),
                                ex, ex.Message);
                        }
                        DoJob(_job, Operator.Finished);
                    }
                }
                else
                {
                    if (_state == ArcThreadState.Ended)
                    {
                        break;
                    }
                }
            }


            _runEvent.Dispose();
            _endEvent.Dispose();
            if (_finishEvent != null)
                _finishEvent.Set();
        }

        private AutoResetEvent _finishEvent;
        public void EndThread(AutoResetEvent finishEvent)
        {
            _finishEvent = finishEvent;
            DoJob(_job, Operator.End);
        }

        private void DoJob(IArcJob job, Operator oper)
        {
            lock (_sync)
            {
                var oldStatus = _state;
                switch (oper)
                {
                    case Operator.Run:
                        if (_state != ArcThreadState.Idle)
                        {
                            throw new InvalidProgramException("State must be idle.");
                        }
                        _state = ArcThreadState.Running;
                        _job = job;
                        _runEvent.Set();
                        break;
                    case Operator.Cancel:
                        switch (_state)
                        {
                            case ArcThreadState.Idle:
                                _state = ArcThreadState.Idle;
                                break;
                            case ArcThreadState.Canceling:
                                _state = ArcThreadState.Canceling;
                                break;
                            case ArcThreadState.Running:
                                _state = ArcThreadState.Canceling;
                                _job.CancelJob();
                                _job.JobCanceledEvent += JobCanceledEvent;
                                break;
                            case ArcThreadState.Ending:
                                _state = ArcThreadState.Ending;
                                break;
                            case ArcThreadState.Ended:
                                throw new InvalidOperationException("the thread ended, can't cancel.");
                            default:
                                throw new NotSupportedException("In cancel, not support state.");
                        }
                        break;
                    case Operator.End:
                        switch (_state)
                        {
                            case ArcThreadState.Idle:
                                _state = ArcThreadState.Ended;
                                _endEvent.Set();
                                break;
                            case ArcThreadState.Canceling:
                                _state = ArcThreadState.Ending;
                                break;
                            case ArcThreadState.Running:
                                _state = ArcThreadState.Ending;
                                _job.EndJob();
                                _job.JobCanceledEvent += JobEndedEvent;
                                break;
                            case ArcThreadState.Ending:
                                _state = ArcThreadState.Ending;
                                break;
                            case ArcThreadState.Ended:
                                throw new InvalidOperationException("the thread ended, can't end.");
                            default:
                                throw new NotSupportedException("In End, not support state.");
                        }
                        break;
                    case Operator.Finished:
                        switch (_state)
                        {
                            case ArcThreadState.Idle:
                                throw new InvalidOperationException("the thread idle, can't finish.");
                            case ArcThreadState.Canceling:
                                _state = ArcThreadState.Idle;
                                break;
                            case ArcThreadState.Running:
                                _state = ArcThreadState.Idle;
                                break;
                            case ArcThreadState.Ending:
                                _state = ArcThreadState.Ended;
                                _endEvent.Set();
                                break;
                            case ArcThreadState.Ended:
                                throw new InvalidOperationException("the thread ended, can't finish.");
                            default:
                                throw new NotSupportedException("In Finished, not support state.");
                        }
                        break;
                    default:
                        throw new NotSupportedException("Not support the operator.");
                }

                LogFactory.LogInstance.WriteLog(ThreadName, LogLevel.DEBUG, "thread job state changed.", "Job [{0}] in thread [{1}] state change from [{2}] to [{3}].", 
                    _job.JobId, ThreadName, oldStatus, _state);
            }
        }

        private void JobEndedEvent(object sender, EventArgs e)
        {

        }

        private void JobCanceledEvent(object sender, EventArgs e)
        {

        }


    }

    public enum Operator
    {
        Run,
        Cancel,
        End,
        Finished
    }
}
