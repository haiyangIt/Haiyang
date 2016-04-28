using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Manager.IF
{
    public interface IJobManager : IManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="job"></param>
        /// <exception cref="">if job end, will throw exception.</exception>
        void AddJob(IArcJob job);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns>if not find, return null.</returns>
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
