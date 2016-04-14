using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EwsFrame.Manager.IF
{
    public interface IJobManager : IManager
    {
        void AddJob(IArcJob job);
        IArcJob GetJob(Guid jobId);
    }

    

    //public abstract class ArcJob
    //{
    //    public Guid JobId { get; private set; }
    //    public object[] JobParam { get; set; }

    //    public abstract ArcJobType JobType { get; }
    //    public abstract ArcJobStatus JobStatus { get; }
    //    public void Run()
    //    {
    //        BeforeRun();
    //        InternalRun();
    //        AfterRun();
    //    }

    //    protected abstract void BeforeRun();
    //    protected abstract void AfterRun();
    //    protected abstract void InternalRun();

    //    public abstract string GetLatestProgress();

    //    protected void UpdateEvent()
    //    {

    //    }

    //    protected ArcJob()
    //    {
    //        JobId = Guid.NewGuid();
    //    }
    //}

}
