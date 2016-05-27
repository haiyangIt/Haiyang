using Arcserve.Office365.Exchange.Log;
using Arcserve.Office365.Exchange.Manager.IF;
using Arcserve.Office365.Exchange.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Manager.Impl
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
            if (string.IsNullOrEmpty(jobName))
                throw new ArgumentNullException("jobName");

            JobName = jobName;
        }

        public Guid JobId { get; private set; }
        public virtual string JobName { get; set; }

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
        public event EventHandler<JobStatusChangedEventArgs> JobStatusChangedEvent;

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
            catch (Exception e)
            {
                LogFactory.LogInstance.WriteException(JobName, LogLevel.ERR, "run exception", e, e.Message);
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
            using (SyncObj.LockWhile(() =>
            {
                var oldStatus = Status;
                LogFactory.LogInstance.WriteLog(JobName, LogLevel.DEBUG, "ChangeOperator start", "Old Status:{0}, operator:{1}.", Status, oper);
                switch (oper)
                {
                    case Operator.Run:
                        if (Status != ArcJobStatus.Waiting)
                        {
                            throw new InvalidProgramException("State must be Waiting.");
                        }
                        Status = ArcJobStatus.Running;
                        break;
                    case Operator.Cancel:
                        switch (Status)
                        {
                            case ArcJobStatus.Waiting:
                                Status = ArcJobStatus.Waiting;
                                break;
                            case ArcJobStatus.Canceling:
                                Status = ArcJobStatus.Canceling;
                                break;
                            case ArcJobStatus.Running:
                                Status = ArcJobStatus.Canceling;
                                break;
                            case ArcJobStatus.Ending:
                                Status = ArcJobStatus.Ending;
                                break;
                            case ArcJobStatus.Ended:
                            case ArcJobStatus.Canceled:
                            case ArcJobStatus.Success:
                                throw new InvalidOperationException("the job completed, can't cancel.");
                            default:
                                throw new NotSupportedException("when cancel, not support state.");
                        }
                        break;
                    case Operator.End:
                        switch (Status)
                        {
                            case ArcJobStatus.Waiting:
                                Status = ArcJobStatus.Ended;
                                break;
                            case ArcJobStatus.Canceling:
                                Status = ArcJobStatus.Ending;
                                break;
                            case ArcJobStatus.Running:
                                Status = ArcJobStatus.Ending;
                                break;
                            case ArcJobStatus.Ending:
                                Status = ArcJobStatus.Ending;
                                break;
                            case ArcJobStatus.Ended:
                            case ArcJobStatus.Canceled:
                            case ArcJobStatus.Success:
                                throw new InvalidOperationException("the job completed, can't end.");
                            default:
                                throw new NotSupportedException("when end, not support state.");
                        }
                        break;
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
                                    catch (Exception ex)
                                    {
                                        LogFactory.LogInstance.WriteException(JobName, LogLevel.ERR, "cancel event exception", ex, ex.Message);
                                    }
                                    finally
                                    {

                                    }
                                }
                                break;
                            case ArcJobStatus.Running:
                                Status = ArcJobStatus.Success;
                                break;
                            case ArcJobStatus.Ending:
                                Status = ArcJobStatus.Ended;
                                if (JobEndedEvent != null)
                                {
                                    try
                                    {
                                        JobEndedEvent.Invoke(this, null); // todo catch exception.
                                    }
                                    catch (Exception ex)
                                    {
                                        LogFactory.LogInstance.WriteException(JobName, LogLevel.ERR, "end event exception", ex, ex.Message);
                                    }
                                    finally
                                    {

                                    }
                                }
                                break;
                            case ArcJobStatus.Ended:
                            case ArcJobStatus.Canceled:
                            case ArcJobStatus.Success:
                                throw new InvalidOperationException("the job completed, can't finish.");
                            default:
                                throw new NotSupportedException("when finished, not support state.");
                        }
                        break;
                    default:
                        throw new NotSupportedException("Not support the operator.");
                }
                if (oldStatus != Status && JobStatusChangedEvent != null)
                {
                    try
                    {
                        JobStatusChangedEvent.Invoke(this, new JobStatusChangedEventArgs(this, oldStatus, Status));
                    }
                    catch (Exception ex)
                    {
                        LogFactory.LogInstance.WriteException(JobName, LogLevel.ERR, "status changed exception", ex, ex.Message);
                    }
                    finally
                    {

                    }
                }

                LogFactory.LogInstance.WriteLog(JobName, LogLevel.DEBUG, "ChangeOperator end", "Old Status:{0}, New Status:{1}, operator:{2}.", oldStatus, Status, oper);
            }))
            { }
        }

        protected bool IsJobNeedCanceledOrEnded()
        {
            return Status == ArcJobStatus.Canceling || Status == ArcJobStatus.Ending;
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
