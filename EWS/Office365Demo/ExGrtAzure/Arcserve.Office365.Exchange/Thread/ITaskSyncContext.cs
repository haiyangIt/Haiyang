using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Thread
{
    public interface ITaskSyncContext<ProgressType>
    {
        JobProgress Progress { get; set; }
        CancellationToken CancelToken { get; set; }
        TaskScheduler Scheduler { get; set; }

        void InitTaskSyncContext(ITaskSyncContext<ProgressType> mainContext);
    }

    public static class TaskSyncContextExtension
    {
        public static void CloneSyncContext(this ITaskSyncContext<JobProgress> copyTo, ITaskSyncContext<JobProgress> copyFrom)
        {
            copyTo.Progress = copyFrom.Progress;
            copyTo.CancelToken = copyFrom.CancelToken;
            copyTo.Scheduler = copyFrom.Scheduler;
        }
    }

}
