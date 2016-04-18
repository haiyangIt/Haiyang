using EwsFrame.Manager.IF;
using EwsFrame.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EwsFrame.Manager.Impl
{
    internal class JobManager : ManagerBase, IJobManager
    {
        protected override void BeforeStart()
        {
            // todo Get un-completed job from db.
            base.BeforeStart();
        }

        public void AddJob(IArcJob job)
        {
            CheckOtherEventCanExecute();

            lock (_JobBuffer)
            {
                _JobBuffer.Add(job.JobId, job);
                arcJobQueue.Enqueue(job);
            }

            TriggerOtherEvent(0);
        }

        Dictionary<Guid, IArcJob> _JobBuffer = new Dictionary<Guid, IArcJob>();
        Deque<IArcJob> arcJobQueue = new Deque<IArcJob>();

        public override string ManagerName
        {
            get
            {
                return "JobManager";
            }
        }

        public IArcJob GetJob(Guid jobId)
        {
            throw new NotImplementedException();
        }

        protected override void MethodWhenOtherEventTriggered(int eventIndex)
        {
            AddJobInInternalThread();
        }

        protected override void MethodWhenTimeOutTriggerd()
        {
            AddJobInInternalThread();
        }

        private void AddJobInInternalThread()
        {
            lock (_JobBuffer)
            {
                var job = arcJobQueue.Dequeue();
                if (job != null)
                {
                    var threadName = string.Format("thread-{0}-{1}", job.JobType, job.JobId);
                    var threadObj = JobFactoryServer.Instance.ThreadManager.NewThread(threadName);
                    if (threadObj == null)
                    {
                        arcJobQueue.EnqueueLast(job);
                    }
                    else
                    {
                        threadObj.Run(job);
                    }
                }
            }
        }
    }

    //public class JobManager : IJobManager
    //{
    //    public static JobManager Instance = new JobManager();

    //    private AutoResetEvent addJobEvent = new AutoResetEvent(false);

    //    private AutoResetEvent endEvent = new AutoResetEvent(false);
    //    private AutoResetEvent endWaitingEvent = new AutoResetEvent(false);


    //    private AutoResetEvent[] events;
    //    private const int AddEventIndex = 0;
    //    private const int EndEventIndex = 1;
    //    private const int WaitTimeOut = 2000;

    //    internal JobManager()
    //    {
    //        events = new AutoResetEvent[2] { addJobEvent, endEvent }; // related AddEventIndex, EndEventIndex;
    //    }

    //    private int isExited = 0;

    //    public IArcJob GetJob(Guid jobId)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void Start()
    //    {
    //        // 1. need get the un-completed job from db, file or other storage.
    //        // 2. and init the  dictionary and queue.

    //        var internalRunning = JobFactory.Instance.ThreadManager.StartThread("JobThreadManager");
    //        var jobManagerInternalJob = new ArcSystemJob(InternalThread, "ThreadManagerInternalJob");
    //        internalRunning.Run(jobManagerInternalJob);
    //    }



    //    public void End()
    //    {
    //        Interlocked.Exchange(ref isExited, 1);
    //        endEvent.Set();
    //        endWaitingEvent.WaitOne();

    //        addJobEvent.Dispose();
    //        endEvent.Dispose();
    //        endWaitingEvent.Dispose();
    //    }

    //    public void AddJob(IArcJob job)
    //    {
    //        if(Interlocked.CompareExchange(ref isExited, 1, 1) == 1)
    //        {
    //            throw new ThreadStateException("Job manager exited, can't add job.");
    //        }

    //        lock (_JobBuffer)
    //        {
    //            _JobBuffer.Add(job.JobId, job);
    //            arcJobQueue.Enqueue(job);
    //        }

    //        addJobEvent.Set();
    //    }

    //    Dictionary<Guid, IArcJob> _JobBuffer = new Dictionary<Guid, IArcJob>();
    //    Deque<IArcJob> arcJobQueue = new Deque<IArcJob>();
    //    private void InternalThread(IArcJob jobManagerJob)
    //    {
    //        bool isLoop = true;
    //        while (isLoop)
    //        {
    //            var waitResult = WaitHandle.WaitAny(events, WaitTimeOut); // waittimeout: loop to try get the new thread.

    //            switch (waitResult)
    //            {
    //                case EndEventIndex:
    //                    isLoop = false;
    //                    break;
    //                case AddEventIndex:
    //                case WaitHandle.WaitTimeout:
    //                    lock (_JobBuffer)
    //                    {
    //                        var job = arcJobQueue.Dequeue();
    //                        if (job != null)
    //                        {
    //                            var threadName = string.Format("thread-{0}-{1}", job.JobType, job.JobId);
    //                            var threadObj = JobFactory.Instance.ThreadManager.NewThread(threadName);
    //                            if (threadObj == null)
    //                            {
    //                                arcJobQueue.EnqueueLast(job);
    //                            }
    //                            else
    //                            {
    //                                threadObj.Run(job);
    //                            }
    //                        }
    //                    }
    //                    break;
    //                default:
    //                    throw new NotSupportedException();
    //            }
    //        }
    //        endWaitingEvent.Set();
    //    }

    //    class Deque<T> : IEnumerable<T>, ICollection, IEnumerable
    //    {
    //        public Deque()
    //        {
    //            _lists = new LinkedList<T>();
    //        }

    //        private LinkedList<T> _lists;
    //        public void Enqueue(T item)
    //        {
    //            lock (_lists)
    //            {
    //                _lists.AddFirst(item);
    //            }
    //        }
    //        public T Dequeue()
    //        {
    //            lock (_lists)
    //            {
    //                var result = _lists.Last;
    //                _lists.RemoveLast();
    //                return result.Value;
    //            }
    //        }

    //        public void EnqueueLast(T item)
    //        {
    //            lock (_lists)
    //            {
    //                _lists.AddLast(item);
    //            }
    //        }

    //        public int Count
    //        {
    //            get
    //            {
    //                return _lists.Count;
    //            }
    //        }

    //        public bool IsReadOnly
    //        {
    //            get
    //            {
    //                return false;
    //            }
    //        }

    //        public object SyncRoot
    //        {
    //            get
    //            {
    //                return _lists;
    //            }
    //        }

    //        public bool IsSynchronized
    //        {
    //            get
    //            {
    //                return false;
    //            }
    //        }

    //        public void Add(T item)
    //        {
    //            Enqueue(item);
    //        }

    //        public void Clear()
    //        {
    //            lock (_lists)
    //            {
    //                _lists.Clear();
    //            }
    //        }

    //        public void CopyTo(Array array, int index)
    //        {
    //            lock (_lists)
    //            {
    //                T[] des = new T[Count - index];
    //                _lists.CopyTo(des, index);
    //                des.CopyTo(array, index);
    //            }
    //        }

    //        public IEnumerator GetEnumerator()
    //        {
    //            return _lists.GetEnumerator();
    //        }

    //        IEnumerator<T> IEnumerable<T>.GetEnumerator()
    //        {
    //            return _lists.GetEnumerator();
    //        }
    //    }
    //}

}
