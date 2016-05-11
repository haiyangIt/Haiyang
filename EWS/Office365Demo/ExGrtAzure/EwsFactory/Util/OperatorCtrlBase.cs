using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EwsFrame.Util
{
    public abstract class OperatorCtrlBase
    {
        private OperatorCtrlBase _other;
        public OperatorCtrlBase(OperatorCtrlBase other)
        {
            _other = other;
        }

        public virtual void DoAction(Action action)
        {
            _other.DoAction(action);
        }
    }

    public class OperatorCtrlBaseImpl : OperatorCtrlBase
    {
        public OperatorCtrlBaseImpl() : base(null)
        {
        }

        public override void DoAction(Action action)
        {
            action.Invoke();
        }
    }

    public class TimeOutOperatorCtrl : OperatorCtrlBase
    {
        public TimeOutOperatorCtrl(OperatorCtrlBase other) : base(other)
        {
        }

        public override void DoAction(Action action)
        {
            LogFactory.LogInstance.WriteLog(LogInterface.LogLevel.DEBUG, "Enter timeout operator.");
            AutoResetEvent ev = null;
            Exception exception = null;
            try
            {
                ev = new AutoResetEvent(false);
                

                ThreadPool.QueueUserWorkItem((args) =>
                {
                    try
                    {
                        base.DoAction(action);
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                    }

                    if (ev != null)
                        ev.Set();
                }, null);

                if (!ev.WaitOne(TimeOut))
                {
                    exception = new TimeoutException();
                    LogFactory.LogInstance.WriteException(LogInterface.LogLevel.ERR, "time out", exception, "time out");
                }
            }
            finally
            {
                LogFactory.LogInstance.WriteLog(LogInterface.LogLevel.DEBUG, "Exit timeout operator.");
                if (ev != null)
                {
                    ev.Dispose();
                    ev = null;
                }
            }

            if (exception != null)
            {
                throw new ApplicationException("exception in time out", exception);
            }
        }

        private static object _lockObj = new object();
        private static int _timeOut = 0;
        public static int TimeOut
        {
            get
            {
                if (_timeOut == 0)
                {
                    using (_lockObj.LockWhile(() =>
                    {
                        if (_timeOut == 0)
                        {
                            int result = 0;
                            if (!int.TryParse(ConfigurationManager.AppSettings["ExportItemTimeOut"], out result))
                            {
                                result = 120;
                            }
                            _timeOut = result * 1000;
                        }
                    }))
                    { }
                }
                return _timeOut;
            }
        }
    }

    public class RetryOperator : OperatorCtrlBase
    {
        private const int MaxRetryCount = 3;
        private int retryCount = 0;
        private Action<Exception> _afterFail;
        private Action _beforeRun;
        public RetryOperator(OperatorCtrlBase other, Action beforeRun, Action<Exception> afterFail) : base(other)
        {
            _afterFail = afterFail;
            _beforeRun = beforeRun;
        }

        public override void DoAction(Action action)
        {
            bool isSuccess = false;
            Exception ex = null;
            do
            {
                retryCount++;
                LogFactory.LogInstance.WriteLog(LogInterface.LogLevel.DEBUG, "Enter retry operator", "Entry retry operator, the [{0}]th time.", retryCount);
                try
                {
                    _beforeRun.Invoke();
                    base.DoAction(action);
                    isSuccess = true;
                }
                catch (Exception e)
                {
                    LogFactory.LogInstance.WriteException(LogInterface.LogLevel.WARN,
                        string.Format("[{0}]th operation failed.{1}", retryCount, retryCount < 3 ? string.Format("will do [{0}]th try", retryCount + 1) : ""),
                        e, e.Message);
                    ex = e;
                    if (retryCount < MaxRetryCount)
                    {
                        try
                        {
                            _afterFail.Invoke(e);
                        }
                        catch (Exception ex1)
                        {

                        }
                    }
                }
            } while (retryCount < MaxRetryCount && !isSuccess);

            LogFactory.LogInstance.WriteLog(LogInterface.LogLevel.DEBUG, string.Format("Exit retry operator run count is {0}.", retryCount));
            if (retryCount >= MaxRetryCount && !isSuccess)
            {
                throw new ApplicationException(string.Format("after {0} retry, the operation still failed.", retryCount), ex);
            }
        }
    }
}
