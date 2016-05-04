using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

        protected virtual OutT RetryFunc<OutT>(Func<OutT> func)
        {
            throw new NotImplementedException();
        }

        protected virtual OutT RetryFunc<ArgT, OutT>(Func<ArgT, OutT> func, ArgT argument1)
        {
            throw new NotImplementedException();
        }

        protected virtual OutT RetryFunc<ArgT1, ArgT2, OutT>(Func<ArgT1, ArgT2, OutT> func, ArgT1 argument1, ArgT2 argument2)
        {
            throw new NotImplementedException();
        }

        protected virtual OutT RetryFunc<ArgT1, ArgT2, ArgT3, OutT>(Func<ArgT1, ArgT2, ArgT3, OutT> func, ArgT1 argument1, ArgT2 argument2, ArgT3 argument3)
        {
            throw new NotImplementedException();
        }

        protected virtual OutT RetryFunc<ArgT1, ArgT2, ArgT3, ArgT4, OutT>(Func<ArgT1, ArgT2, ArgT3, ArgT4, OutT> func, 
            ArgT1 argument1, ArgT2 argument2, ArgT3 argument3, ArgT4 argument4)
        {
            throw new NotImplementedException();
        }

        protected virtual OutT RetryFunc<ArgT1, ArgT2, ArgT3, ArgT4, ArgT5, OutT>(Func<ArgT1, ArgT2, ArgT3, ArgT4, ArgT5, OutT> func,
            ArgT1 argument1, ArgT2 argument2, ArgT3 argument3, ArgT4 argument4, ArgT5 argument5)
        {
            throw new NotImplementedException();
        }

        protected virtual OutT RetryFunc<ArgT1, ArgT2, ArgT3, ArgT4, ArgT5, ArgT6, OutT>(Func<ArgT1, ArgT2, ArgT3, ArgT4, ArgT5, ArgT6, OutT> func,
            ArgT1 argument1, ArgT2 argument2, ArgT3 argument3, ArgT4 argument4, ArgT5 argument5, ArgT6 argument6)
        {
            throw new NotImplementedException();
        }

        protected virtual OutT RetryFunc<ArgT1, ArgT2, ArgT3, ArgT4, ArgT5, ArgT6, ArgT7, OutT>(Func<ArgT1, ArgT2, ArgT3, ArgT4, ArgT5, ArgT6, ArgT7, OutT> func,
            ArgT1 argument1, ArgT2 argument2, ArgT3 argument3, ArgT4 argument4, ArgT5 argument5, ArgT6 argument6, ArgT7 argument7)
        {
            throw new NotImplementedException();
        }

        protected virtual OutT RetryFunc<ArgT1, ArgT2, ArgT3, ArgT4, ArgT5, ArgT6, ArgT7, ArgT8, OutT>(Func<ArgT1, ArgT2, ArgT3, ArgT4, ArgT5, ArgT6, ArgT7, ArgT8, OutT> func,
            ArgT1 argument1, ArgT2 argument2, ArgT3 argument3, ArgT4 argument4, ArgT5 argument5, ArgT6 argument6, ArgT7 argument7, ArgT8 argument8)
        {
            throw new NotImplementedException();
        }

        protected virtual void RetryAction<OutT>(Action func)
        {

        }

        protected virtual void RetryAction<ArgT>(Action<ArgT> func, ArgT argument1)
        {

        }

        protected virtual void RetryAction<ArgT1, ArgT2>(Action<ArgT1, ArgT2> func, ArgT1 argument1, ArgT2 argument2)
        {

        }

        protected virtual void RetryAction<ArgT1, ArgT2, ArgT3>(Action<ArgT1, ArgT2, ArgT3> func, ArgT1 argument1, ArgT2 argument2, ArgT3 argument3)
        {

        }

        protected virtual void RetryAction<ArgT1, ArgT2, ArgT3, ArgT4>(Action<ArgT1, ArgT2, ArgT3, ArgT4> func,
            ArgT1 argument1, ArgT2 argument2, ArgT3 argument3, ArgT4 argument4)
        {

        }

        protected virtual void RetryAction<ArgT1, ArgT2, ArgT3, ArgT4, ArgT5>(Action<ArgT1, ArgT2, ArgT3, ArgT4, ArgT5> func,
            ArgT1 argument1, ArgT2 argument2, ArgT3 argument3, ArgT4 argument4, ArgT5 argument5)
        {

        }

        protected virtual void RetryAction<ArgT1, ArgT2, ArgT3, ArgT4, ArgT5, ArgT6>(Action<ArgT1, ArgT2, ArgT3, ArgT4, ArgT5, ArgT6> func,
            ArgT1 argument1, ArgT2 argument2, ArgT3 argument3, ArgT4 argument4, ArgT5 argument5, ArgT6 argument6)
        {

        }

        protected virtual void RetryAction<ArgT1, ArgT2, ArgT3, ArgT4, ArgT5, ArgT6, ArgT7>(Action<ArgT1, ArgT2, ArgT3, ArgT4, ArgT5, ArgT6, ArgT7> func,
            ArgT1 argument1, ArgT2 argument2, ArgT3 argument3, ArgT4 argument4, ArgT5 argument5, ArgT6 argument6, ArgT7 argument7)
        {

        }

        protected virtual void RetryAction<ArgT1, ArgT2, ArgT3, ArgT4, ArgT5, ArgT6, ArgT7, ArgT8>(Action<ArgT1, ArgT2, ArgT3, ArgT4, ArgT5, ArgT6, ArgT7, ArgT8> func,
            ArgT1 argument1, ArgT2 argument2, ArgT3 argument3, ArgT4 argument4, ArgT5 argument5, ArgT6 argument6, ArgT7 argument7, ArgT8 argument8)
        {

        }
    }
}
