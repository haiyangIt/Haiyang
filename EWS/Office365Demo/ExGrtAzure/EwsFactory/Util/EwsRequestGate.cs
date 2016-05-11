using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EwsFrame.Util
{
    

    public class EwsRequestGate
    {
        private static object _lockObj = new object();
        private static EwsRequestGate _instance;
        public static EwsRequestGate Instance
        {
            get
            {
                if (_instance == null)
                {
                    using (_lockObj.LockWhile(() =>
                    {
                        if (_instance == null)
                        {
                            _instance = new EwsRequestGate();
                        }
                    }))
                    { }
                }
                return _instance;
            }
        }

        private static ManualResetEventSlim manualReset = new ManualResetEventSlim(true);
        public void Enter()
        {
            manualReset.Wait();
        }

        ConcurrentDictionary<Type, OperationForFailBeforeRun> _actions = new ConcurrentDictionary<Type, OperationForFailBeforeRun>();
        private int WaitingTime = actionWaitingMaxTime;
        private DateTime WaitingStartTime;

        private const int actionWaitingMaxTime = 60;
        private bool isInWaiting = false;
        public void Close(KeyValuePair<Type, OperationForFailBeforeRun> actionToOpen)
        {
            using (_lockObj.LockWhile(() =>
            {
                _actions[actionToOpen.Key] = actionToOpen.Value;
                if (WaitingTime < actionToOpen.Value.WaitTimeSecond)
                    WaitingTime = actionToOpen.Value.WaitTimeSecond;
                if (isInWaiting)
                    return;
                else
                {
                    isInWaiting = true;
                    manualReset.Reset();
                    ThreadPool.QueueUserWorkItem(InternalWaiting);
                    LogFactory.LogInstance.WriteLog(LogInterface.LogLevel.INFO, "suspend all ews request. will resume after little second ");
                }
            }))
            { }
        }

        /// <summary>
        /// to wait some second to avoid re-close.
        /// </summary>
        private void InternalWaiting(object args)
        {
            WaitingStartTime = DateTime.Now;
            bool isBreak = false;
            while (!isBreak)
            {
                Thread.Sleep(1000);
                using (_lockObj.LockWhile(() => {
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
                                    try {
                                        LogFactory.LogInstance.WriteException(LogInterface.LogLevel.ERR, string.Format("For exception {0} failure, the restore operation is failed", operatorForRunAgain.ExceptionType.FullName), e, e.Message);
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
                        isInWaiting = false;
                        isBreak = true;
                        Open();
                        LogFactory.LogInstance.WriteLog(LogInterface.LogLevel.INFO, "resume ews request.");
                    }
                }))
                { }
            }
        }

        public void Open()
        {
            manualReset.Set();
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
