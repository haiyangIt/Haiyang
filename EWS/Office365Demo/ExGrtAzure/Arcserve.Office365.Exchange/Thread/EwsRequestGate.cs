using Arcserve.Office365.Exchange.Log;
using Arcserve.Office365.Exchange.Manager;
using Arcserve.Office365.Exchange.Topaz;
using Arcserve.Office365.Exchange.Util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Thread
{
    public class EwsRequestGate : IDisposable
    {
        static EwsRequestGate()
        {
            Instance = new EwsRequestGate();

            DisposeManager.RegisterInstance(Instance);
        }
        private static readonly object _lockObj = new object();
        public static readonly EwsRequestGate Instance;

        private ManualResetEventSlim manualReset = new ManualResetEventSlim(true);
        private object _lockManual = new object();
        public void Enter()
        {
            if (manualReset != null)
            {
                manualReset.Wait();
            }
        }

        ConcurrentDictionary<Type, OperationForFailBeforeRun> _actions = new ConcurrentDictionary<Type, OperationForFailBeforeRun>();
        private int WaitingTime = actionWaitingMaxTime;
        private DateTime WaitingStartTime;

        private const int actionWaitingMaxTime = 60;
        private bool isInSuspending = false;
        public void Close(KeyValuePair<Type, OperationForFailBeforeRun> actionToOpen)
        {
            using (_lockObj.LockWhile(() =>
            {
                _actions[actionToOpen.Key] = actionToOpen.Value;
                if (WaitingTime < actionToOpen.Value.WaitTimeSecond)
                    WaitingTime = actionToOpen.Value.WaitTimeSecond;
                if (isInSuspending)
                    return;
                else
                {
                    isInSuspending = true;
                    manualReset.Reset();
                    ThreadPool.QueueUserWorkItem(InternalSuspending);
                    LogFactory.LogInstance.WriteLog(LogLevel.WARN, "suspend all ews request. will resume after little second ");
                }
            }))
            { }
        }

        /// <summary>
        /// to wait some second to avoid re-close.
        /// </summary>
        private void InternalSuspending(object args)
        {
            WaitingStartTime = DateTime.Now;
            bool isBreak = false;
            while (!isBreak)
            {
                System.Threading.Thread.Sleep(1000);
                using (_lockObj.LockWhile(() =>
                {
                    if ((DateTime.Now - WaitingStartTime).TotalSeconds > WaitingTime)
                    {
                        foreach (var operatorForRunAgain in _actions.Values)
                        {
                            if (operatorForRunAgain.Action != null)
                            {
                                try
                                {
                                    operatorForRunAgain.Action.Invoke();
                                }
                                catch (Exception e)
                                {
                                    try
                                    {
                                        LogFactory.LogInstance.WriteException(LogLevel.ERR, string.Format("For exception {0} failure, the restore operation is failed", operatorForRunAgain.ExceptionType.FullName), e, e.Message);
                                    }
                                    catch
                                    {

                                    }
                                }
                            }
                        }

                        WaitingStartTime = DateTime.Now;
                        WaitingTime = actionWaitingMaxTime;
                        _actions.Clear();
                        isInSuspending = false;
                        isBreak = true;
                        LogFactory.LogInstance.WriteLog(LogLevel.INFO, string.Format("resume ews request. operationCount:{0}.", OperatorCtrlBase.OperationCount));
                        Open();
                    }
                }))
                { }
            }
        }

        public void Open()
        {
            if (manualReset != null)
            {
                manualReset.Set();
            }
        }

        public void Dispose()
        {
            if (manualReset != null)
            {
                manualReset.Dispose();
                manualReset = null;
            }
        }
    }

    public class OperationForFailBeforeRun
    {
        public int WaitTimeSecond;
        public Action Action;
        public Type ExceptionType;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="waitTimeSecond">wait time seconds.</param>
        /// <param name="action"></param>
        /// <param name="exceptionType"></param>
        public OperationForFailBeforeRun(int waitTimeSecond, Action action, Type exceptionType)
        {
            WaitTimeSecond = waitTimeSecond;
            Action = action;
            ExceptionType = exceptionType;
        }
    }
}
