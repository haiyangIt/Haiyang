using Arcserve.Office365.Exchange.Log;
using Arcserve.Office365.Exchange.Thread;
using Arcserve.Office365.Exchange.Util;
using Arcserve.Office365.Exchange.Util.Setting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;



namespace Arcserve.Office365.Exchange.Topaz
{
    public abstract class OperatorCtrlBase
    {
        private OperatorCtrlBase _other;
        protected string _operationName;
        protected string _threadDataInformation;
        public OperatorCtrlBase(OperatorCtrlBase other, string operationName)
        {
            _other = other;
            _operationName = operationName;
            _threadDataInformation = ThreadData.Information;
        }

        protected string GetMessage(string debugInfo)
        {
            return string.Format("{0:D8}\t{1,-15}\t{2}", _threadDataInformation, _operationName, debugInfo);
        }

        public virtual void DoAction(Action action)
        {
            _other.DoAction(action);
        }

        #region Retry Time out Operation


        public static long OperationCount = 0;

        public static void DoActionWithRetryTimeOut(Action operation, string operationName, Func<Exception, bool> funcIsExceptionNeedSuspendRequest, Func<Exception, bool> funcIsRetry)
        {
            var operatorCtrl = NewOperatorCtrlBase(operationName, funcIsExceptionNeedSuspendRequest, funcIsRetry);
            Interlocked.Increment(ref OperationCount);
            operatorCtrl.DoAction(operation);
        }


        private static OperatorCtrlBase NewOperatorCtrlBase(string operationName, Func<Exception, bool> funcIsExceptionNeedSuspendRequest, Func<Exception, bool> funcIsRetry)
        {
            var b = new OperatorCtrlBaseImpl(operationName);
            var timeOut = new TimeOutOperatorCtrl(b, operationName);
            var retry = new RetryOperator(timeOut, operationName,
                () =>
                {
                    EwsRequestGate.Instance.Enter();
                },
                (e) =>
                {
                    var type = e.GetType();
                    if (funcIsExceptionNeedSuspendRequest(e))
                    {
                        EwsRequestGate.Instance.Close(new KeyValuePair<Type, OperationForFailBeforeRun>(type, new OperationForFailBeforeRun(CloudConfig.Instance.SuspendRequestTimeAfterThrowSpecificException,
                            () =>
                            {
                                //DoNewExchangeService(Mailbox, EwsArgument, true);
                            }, type)));
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(5 * 1000);
                    }
                }, funcIsRetry);
            return retry;
        }

        #endregion
    }

    public class OperatorCtrlBaseImpl : OperatorCtrlBase
    {
        public OperatorCtrlBaseImpl(string operationName) : base(null, operationName)
        {
        }

        public override void DoAction(Action action)
        {
            action.Invoke();
        }
    }

    public class TimeOutOperatorCtrl : OperatorCtrlBase
    {
        public TimeOutOperatorCtrl(OperatorCtrlBase other, string operationName) : base(other, operationName)
        {
        }

        public static string RetryMessage = string.Format("Time out, the operation cost more than {0} seconds", CloudConfig.Instance.RequestTimeOut);
        private object evLock = new object();
        public override void DoAction(Action action)
        {
            //LogFactory.LogInstance.WriteLog(LogInterface.LogLevel.DEBUG, GetMessage("Enter timeout operator."));
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

                    using (evLock.LockWhile(() =>
                    {
                        if (ev != null)
                            ev.Set();
                    }))
                    { }

                }, null);

                if (!ev.WaitOne(CloudConfig.Instance.RequestTimeOut))
                {
                    exception = new TimeoutException(RetryMessage);
                    LogFactory.LogInstance.WriteException(LogLevel.ERR, GetMessage("time out"), exception, "time out");
                }
            }
            finally
            {
                //LogFactory.LogInstance.WriteLog(LogInterface.LogLevel.DEBUG, GetMessage("Exit timeout operator."));
                using (evLock.LockWhile(() =>
                {
                    if (ev != null)
                    {
                        ev.Dispose();
                        ev = null;
                    }
                }))
                { }
            }

            if (exception != null)
            {
                LogFactory.LogInstance.WriteException(LogLevel.ERR, GetMessage("throw exception in timeout operator"), exception, exception.Message);
                throw exception;
            }
        }
    }

    public class RetryOperator : OperatorCtrlBase
    {
        private const int MaxRetryCount = 3;
        private int retryCount = 0;
        private Action<Exception> _afterFail;
        private Action _beforeRun;
        private Func<Exception, bool> _isRetry;
        public RetryOperator(OperatorCtrlBase other, string operationName, Action beforeRun, Action<Exception> afterFail, Func<Exception, bool> isRetry) : base(other, operationName)
        {
            _afterFail = afterFail;
            _beforeRun = beforeRun;
            _isRetry = isRetry;
        }

        public override void DoAction(Action action)
        {
            bool isSuccess = false;
            Exception ex = null;
            do
            {
                retryCount++;
                //LogFactory.LogInstance.WriteLog(LogInterface.LogLevel.DEBUG, GetMessage("Enter retry operator"), "Entry retry operator, the [{0}]th time.", retryCount);
                try
                {
                    _beforeRun.Invoke();
                    base.DoAction(action);
                    isSuccess = true;
                }
                catch (OperationCanceledException cancelEx)
                {
                    LogFactory.LogInstance.WriteException(LogLevel.WARN,
                        GetMessage(string.Format("[{0}]th operation canceled.", retryCount)),
                        cancelEx, cancelEx.Message);
                    throw cancelEx;
                }
                catch (ThreadAbortException abortException)
                {
                    LogFactory.LogInstance.WriteException(LogLevel.WARN,
                        GetMessage(string.Format("[{0}]th operation thread abort.", retryCount)),
                        abortException, abortException.Message);
                    throw abortException;
                }
                catch (Exception e)
                {

                    ex = e;

                    if (_isRetry(e))
                    {
                        if (retryCount < MaxRetryCount)
                        {
                            LogFactory.LogInstance.WriteException(LogLevel.WARN,
                        GetMessage(string.Format("[{0}]th operation failed.{1}", retryCount, retryCount < 3 ? string.Format("will do [{0}]th try", retryCount + 1) : "")),
                        e, e.Message);
                            try
                            {
                                _afterFail.Invoke(e);
                            }
                            catch (Exception ex1)
                            {

                            }
                        }
                    }
                    else
                    {
                        LogFactory.LogInstance.WriteException(LogLevel.ERR,
                        GetMessage(string.Format("[{0}]th operation failed.", retryCount)),
                        e, e.Message);
                        throw e;
                    }
                }
            } while (retryCount < MaxRetryCount && !isSuccess);

            //LogFactory.LogInstance.WriteLog(LogInterface.LogLevel.DEBUG, GetMessage(string.Format("Exit retry operator run count is {0}.", retryCount)));
            if (retryCount >= MaxRetryCount && !isSuccess)
            {
                throw new ApplicationException(string.Format("after {0} retry, the operation still failed.", retryCount), ex);
            }
        }
    }
}
