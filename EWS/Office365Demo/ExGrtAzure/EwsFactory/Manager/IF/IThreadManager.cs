using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EwsFrame.Manager.IF
{
    public interface IThreadManager : IManager
    {
        IThreadObj NewThread();
        IThreadObj NewThread(string threadName);
        IThreadObj StartThread(string threadName);
    }

    public interface IThreadObj
    {
        void Run(IArcJob job);
        ArcThreadState State { get; }
        string ThreadName { get; }
        void CancelThread();
        void EndThread(AutoResetEvent waitingHandle);
    }

    public enum ArcThreadState
    {
        Idle = 0,
        Running = 1,
        Canceling = 2,
        Ending = 3,
        Ended = 4
    }
}
