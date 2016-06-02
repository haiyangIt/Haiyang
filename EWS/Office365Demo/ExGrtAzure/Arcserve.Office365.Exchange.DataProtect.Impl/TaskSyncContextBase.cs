using Arcserve.Office365.Exchange.Thread;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Arcserve.Office365.Exchange.DataProtect.Impl
{
    internal class TaskSyncContextBase : ITaskSyncContext<IJobProgress>
    {
        public CancellationToken CancelToken
        {
            get; set;
        }

        public IJobProgress Progress
        {
            get; set;
        }

        public TaskScheduler Scheduler
        {
            get; set;
        }

        public TaskSyncContextBase()
        {
            CancellationTokenSource source = new CancellationTokenSource();
            CancelToken = source.Token;
            Progress = new JobProgress();
            Scheduler = TaskScheduler.Default;
        }

        public void InitTaskSyncContext(ITaskSyncContext<IJobProgress> mainContext)
        {
            this.CloneSyncContext(mainContext);
        }
    }
}
