using EwsFrame.Manager.IF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EwsFrame.Manager.Impl
{

    public abstract class ArcJobBase : IArcJob
    {
        public ArcJobBase()
        {
            Status = ArcJobStatus.Waiting;
            JobId = Guid.NewGuid();
        }

        public ArcJobBase(string jobName) : this()
        {
            JobName = jobName;
        }

        public Guid JobId { get; private set; }
        public string JobName { get; set; }

        protected object SyncObj = new object();

        protected AutoResetEvent _cancelEvent = new AutoResetEvent(false);
        protected AutoResetEvent _endEvent = new AutoResetEvent(false);

        public abstract ArcJobType JobType
        {
            get;
        }

        public ArcJobStatus Status
        {
            get; private set;
        }

        public event EventHandler JobCanceledEvent;
        public event EventHandler JobEndedEvent;

        public bool CancelJob()
        {
            ChangeOperator(Operator.Cancel);
            _cancelEvent.Set();
            return true;
        }

        public bool EndJob()
        {
            ChangeOperator(Operator.End);
            _endEvent.Set();
            return true;
        }

        public void Run()
        {
            ChangeOperator(Operator.Run);
            try
            {
                InternalRun(); //todo catch exception.
            }
            finally
            {
                ChangeOperator(Operator.Finished);
                _cancelEvent.Dispose();
                _endEvent.Dispose();
            }
        }

        protected abstract void InternalRun();

        private void ChangeOperator(Operator oper)
        {
            lock (SyncObj)
            {
                switch (oper)
                {
                    case Operator.Run:
                        if (Status != ArcJobStatus.Waiting)
                        {
                            throw new InvalidProgramException("State must be Waiting.");
                        }
                        Status = ArcJobStatus.Running;
                        return;
                    case Operator.Cancel:
                        switch (Status)
                        {
                            case ArcJobStatus.Waiting:
                                Status = ArcJobStatus.Waiting;
                                return;
                            case ArcJobStatus.Canceling:
                                Status = ArcJobStatus.Canceling;
                                return;
                            case ArcJobStatus.Running:
                                Status = ArcJobStatus.Canceling;
                                return;
                            case ArcJobStatus.Ending:
                                Status = ArcJobStatus.Ending;
                                return;
                            case ArcJobStatus.Ended:
                            case ArcJobStatus.Canceled:
                            case ArcJobStatus.Success:
                                throw new InvalidOperationException("the job completed, can't cancel.");
                            default:
                                throw new NotSupportedException("when cancel, not support state.");
                        }

                    case Operator.End:
                        switch (Status)
                        {
                            case ArcJobStatus.Waiting:
                                Status = ArcJobStatus.Ended;
                                return;
                            case ArcJobStatus.Canceling:
                                Status = ArcJobStatus.Ending;
                                return;
                            case ArcJobStatus.Running:
                                Status = ArcJobStatus.Ending;
                                return;
                            case ArcJobStatus.Ending:
                                Status = ArcJobStatus.Ending;
                                return;
                            case ArcJobStatus.Ended:
                            case ArcJobStatus.Canceled:
                            case ArcJobStatus.Success:
                                throw new InvalidOperationException("the job completed, can't end.");
                            default:
                                throw new NotSupportedException("when end, not support state.");
                        }

                    case Operator.Finished:
                        switch (Status)
                        {
                            case ArcJobStatus.Waiting:
                                throw new InvalidOperationException("the job is waiting, can't finish.");
                            case ArcJobStatus.Canceling:
                                Status = ArcJobStatus.Canceled;
                                if (JobCanceledEvent != null)
                                {
                                    try
                                    {
                                        JobCanceledEvent.Invoke(this, null); // todo catch exception.
                                    }
                                    finally
                                    {

                                    }
                                }
                                return;
                            case ArcJobStatus.Running:
                                Status = ArcJobStatus.Success;
                                return;
                            case ArcJobStatus.Ending:
                                Status = ArcJobStatus.Ended;
                                if (JobEndedEvent != null)
                                {
                                    try
                                    {
                                        JobEndedEvent.Invoke(this, null); // todo catch exception.
                                    }
                                    finally
                                    {

                                    }
                                }
                                return;
                            case ArcJobStatus.Ended:
                            case ArcJobStatus.Canceled:
                            case ArcJobStatus.Success:
                                throw new InvalidOperationException("the job completed, can't finish.");
                            default:
                                throw new NotSupportedException("when finished, not support state.");
                        }
                    default:
                        throw new NotSupportedException("Not support the operator.");
                }
            }
        }
    }


    internal class ArcSystemJob : ArcJobBase
    {
        private Action<IArcJob> _internalAction;
        internal ArcSystemJob(Action<IArcJob> internalFun, string jobName) : base(jobName)
        {
            if (internalFun == null)
            {
                throw new ArgumentNullException("internalFun.");
            }
            _internalAction = internalFun;
        }

        public override ArcJobType JobType
        {
            get
            {
                return ArcJobType.System;
            }
        }

        protected override void InternalRun()
        {
            _internalAction.Invoke(this);
        }
    }
}
