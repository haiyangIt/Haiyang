using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EwsFrame.Manager.IF
{
    public interface IArcJob
    {
        string JobName { get; }
        Guid JobId { get; }
        ArcJobType JobType { get; }
        void Run();

        /// <summary>
        /// only modify the state, can't waiting util job canceled.
        /// </summary>
        /// <returns></returns>
        bool CancelJob();

        ArcJobStatus Status { get; }

        /// <summary>
        /// only modify the state, can't waiting util job canceled.
        /// </summary>
        /// <returns></returns>
        bool EndJob();
        event EventHandler JobCanceledEvent;
        event EventHandler JobEndedEvent;
    }

    public enum ArcJobStatus
    {
        Waiting,
        Running,
        Canceling,
        Ending,
        Canceled,
        Ended,
        Success
    }

    public enum ArcJobType
    {
        Backup,
        Restore,
        System
    }
}
