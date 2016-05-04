using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.TransientFaultHandling;
using Arcserve.Office365.Exchange.Topaz;

namespace Arcserve.Office365.Exchange.Thread
{
    public abstract class TaskSyncContextBase : ITaskSyncContext<IJobProgress>
    {
        public CancellationToken CancelToken
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public IJobProgress Progress
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public TaskScheduler Scheduler
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public void InitTaskSyncContext(ITaskSyncContext<IJobProgress> mainContext)
        {
            throw new NotImplementedException();
        }

        protected virtual void CheckCanceled()
        {
            if (CancelToken.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }
        }

        protected virtual void WaitForPerformanceLimit()
        {

        }

        private RetryPolicy GetRetryPolicy(RetryContext retryContext)
        {
            var retryStrategy = TopazManager.Instance.GetRetryStrategy(retryContext);

            var retryDetectionPolicy = TopazManager.Instance.GetDetectionStrategy(retryContext);
            var retryPolicy = new RetryPolicy(retryDetectionPolicy, retryStrategy);

            retryPolicy.Retrying += (sender, e) =>
            {
                retryContext.RetryEventHandler(sender, e);
            };
            return retryPolicy;
        }

        protected virtual OutT RetryFunc<OutT>(RetryContext retryContext, Func<OutT> func)
        {
            var retryPolicy = GetRetryPolicy(retryContext);
            try
            {
                OutT result = default(OutT);
                retryPolicy.ExecuteAction(()=> {
                    result = func.Invoke();
                });
                return result;
            }
            catch (Exception e)
            {
                throw retryContext.RetryFailureHandler(e);
            }
        }

        protected virtual void RetryAction<OutT>(RetryContext retryContext, Action action)
        {
            var retryPolicy = GetRetryPolicy(retryContext);
            try
            {
                retryPolicy.ExecuteAction(() => {
                    action.Invoke();
                });
            }
            catch (Exception e)
            {
                throw retryContext.RetryFailureHandler(e);
            }
        }

        //protected virtual OutT RetryFunc<OutT>(RetryContext retryContext, Func<OutT> func)
        //{
        //    return DoRetryFunc<OutT>(retryContext, () =>
        //    {
        //        return func.Invoke();
        //    });
        //}


        //protected virtual OutT RetryFunc<ArgT, OutT>(RetryContext retryContext, Func<ArgT, OutT> func, ArgT argument1)
        //{
        //    return DoRetryFunc<OutT>(retryContext, () =>
        //    {
        //        return func.Invoke(argument1);
        //    });
        //}

        //protected virtual OutT RetryFunc<ArgT1, ArgT2, OutT>(RetryContext retryContext, Func<ArgT1, ArgT2, OutT> func, ArgT1 argument1, ArgT2 argument2)
        //{
        //    return DoRetryFunc<OutT>(retryContext, () =>
        //    {
        //        return func.Invoke(argument1, argument2);
        //    });
        //}

        //protected virtual OutT RetryFunc<ArgT1, ArgT2, ArgT3, OutT>(RetryContext retryContext, Func<ArgT1, ArgT2, ArgT3, OutT> func, ArgT1 argument1, ArgT2 argument2, ArgT3 argument3)
        //{
        //    return DoRetryFunc<OutT>(retryContext, () =>
        //    {
        //        return func.Invoke(argument1, argument2, argument3);
        //    });
        //}

        //protected virtual OutT RetryFunc<ArgT1, ArgT2, ArgT3, ArgT4, OutT>(RetryContext retryContext, Func<ArgT1, ArgT2, ArgT3, ArgT4, OutT> func, 
        //    ArgT1 argument1, ArgT2 argument2, ArgT3 argument3, ArgT4 argument4)
        //{
        //    return DoRetryFunc<OutT>(retryContext, () =>
        //    {
        //        return func.Invoke(argument1, argument2, argument3, argument4);
        //    });
        //}

        //protected virtual OutT RetryFunc<ArgT1, ArgT2, ArgT3, ArgT4, ArgT5, OutT>(RetryContext retryContext, Func<ArgT1, ArgT2, ArgT3, ArgT4, ArgT5, OutT> func,
        //    ArgT1 argument1, ArgT2 argument2, ArgT3 argument3, ArgT4 argument4, ArgT5 argument5)
        //{
        //    return DoRetryFunc<OutT>(retryContext, () =>
        //    {
        //        return func.Invoke(argument1, argument2, argument3, argument4, argument5);
        //    });
        //}

        //protected virtual OutT RetryFunc<ArgT1, ArgT2, ArgT3, ArgT4, ArgT5, ArgT6, OutT>(RetryContext retryContext, Func<ArgT1, ArgT2, ArgT3, ArgT4, ArgT5, ArgT6, OutT> func,
        //    ArgT1 argument1, ArgT2 argument2, ArgT3 argument3, ArgT4 argument4, ArgT5 argument5, ArgT6 argument6)
        //{
        //    return DoRetryFunc<OutT>(retryContext, () =>
        //    {
        //        return func.Invoke(argument1, argument2, argument3, argument4, argument5, argument6);
        //    });
        //}

        //protected virtual OutT RetryFunc<ArgT1, ArgT2, ArgT3, ArgT4, ArgT5, ArgT6, ArgT7, OutT>(RetryContext retryContext, Func<ArgT1, ArgT2, ArgT3, ArgT4, ArgT5, ArgT6, ArgT7, OutT> func,
        //    ArgT1 argument1, ArgT2 argument2, ArgT3 argument3, ArgT4 argument4, ArgT5 argument5, ArgT6 argument6, ArgT7 argument7)
        //{
        //    return DoRetryFunc<OutT>(retryContext, () =>
        //    {
        //        return func.Invoke(argument1, argument2, argument3, argument4, argument5, argument6, argument7);
        //    });
        //}

        //protected virtual OutT RetryFunc<ArgT1, ArgT2, ArgT3, ArgT4, ArgT5, ArgT6, ArgT7, ArgT8, OutT>(RetryContext retryContext, Func<ArgT1, ArgT2, ArgT3, ArgT4, ArgT5, ArgT6, ArgT7, ArgT8, OutT> func,
        //    ArgT1 argument1, ArgT2 argument2, ArgT3 argument3, ArgT4 argument4, ArgT5 argument5, ArgT6 argument6, ArgT7 argument7, ArgT8 argument8)
        //{
        //    return DoRetryFunc<OutT>(retryContext, () =>
        //    {
        //        return func.Invoke(argument1, argument2, argument3, argument4, argument5, argument6, argument7, argument8);
        //    });
        //}

        //protected virtual void RetryAction<OutT>(RetryContext retryContext, Action func)
        //{

        //}

        //protected virtual void RetryAction<ArgT>(RetryContext retryContext, Action<ArgT> func, ArgT argument1)
        //{

        //}

        //protected virtual void RetryAction<ArgT1, ArgT2>(RetryContext retryContext, Action<ArgT1, ArgT2> func, ArgT1 argument1, ArgT2 argument2)
        //{

        //}

        //protected virtual void RetryAction<ArgT1, ArgT2, ArgT3>(RetryContext retryContext, Action<ArgT1, ArgT2, ArgT3> func, ArgT1 argument1, ArgT2 argument2, ArgT3 argument3)
        //{

        //}

        //protected virtual void RetryAction<ArgT1, ArgT2, ArgT3, ArgT4>(RetryContext retryContext, Action<ArgT1, ArgT2, ArgT3, ArgT4> func,
        //    ArgT1 argument1, ArgT2 argument2, ArgT3 argument3, ArgT4 argument4)
        //{

        //}

        //protected virtual void RetryAction<ArgT1, ArgT2, ArgT3, ArgT4, ArgT5>(RetryContext retryContext, Action<ArgT1, ArgT2, ArgT3, ArgT4, ArgT5> func,
        //    ArgT1 argument1, ArgT2 argument2, ArgT3 argument3, ArgT4 argument4, ArgT5 argument5)
        //{

        //}

        //protected virtual void RetryAction<ArgT1, ArgT2, ArgT3, ArgT4, ArgT5, ArgT6>(RetryContext retryContext, Action<ArgT1, ArgT2, ArgT3, ArgT4, ArgT5, ArgT6> func,
        //    ArgT1 argument1, ArgT2 argument2, ArgT3 argument3, ArgT4 argument4, ArgT5 argument5, ArgT6 argument6)
        //{

        //}

        //protected virtual void RetryAction<ArgT1, ArgT2, ArgT3, ArgT4, ArgT5, ArgT6, ArgT7>(RetryContext retryContext, Action<ArgT1, ArgT2, ArgT3, ArgT4, ArgT5, ArgT6, ArgT7> func,
        //    ArgT1 argument1, ArgT2 argument2, ArgT3 argument3, ArgT4 argument4, ArgT5 argument5, ArgT6 argument6, ArgT7 argument7)
        //{

        //}

        //protected virtual void RetryAction<ArgT1, ArgT2, ArgT3, ArgT4, ArgT5, ArgT6, ArgT7, ArgT8>(RetryContext retryContext, Action<ArgT1, ArgT2, ArgT3, ArgT4, ArgT5, ArgT6, ArgT7, ArgT8> func,
        //    ArgT1 argument1, ArgT2 argument2, ArgT3 argument3, ArgT4 argument4, ArgT5 argument5, ArgT6 argument6, ArgT7 argument7, ArgT8 argument8)
        //{

        //}
    }


}
