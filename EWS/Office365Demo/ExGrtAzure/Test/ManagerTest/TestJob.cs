using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Arcserve.Office365.Exchange.Manager.Impl;
using Arcserve.Office365.Exchange.Manager.IF;

namespace ManagerTest
{
    public class TestJob : ArcJobBase
    {
        static Random r = new Random();
        static Random a = new Random();
        public TestJob() : base()
        {
            _sleepSecond = r.Next(10);
            _totalSteps = a.Next(20, 100);
            _currenStep = 0;
        }

        private int _sleepSecond;
        private int _totalSteps;
        private int _currenStep;

        public override ArcJobType JobType
        {
            get
            {
                return ArcJobType.System;
            }
        }

        protected override void InternalRun()
        {
            while (true)
            {
                _currenStep++;
                JobFactoryServer.Instance.ProgressManager.AddProgress(new TestProgressInfo(this, "arcserve", _currenStep, _totalSteps));
                Thread.Sleep(_sleepSecond * 1000);
                if (_currenStep >= _totalSteps)
                    break;
                if (Status == ArcJobStatus.Canceling || Status == ArcJobStatus.Ending)
                    break;
            }
        }

        public override string ToString()
        {
            return string.Format("{0} Status:{1}, Sleep:{4}, Progress:{2}/{3}", JobName, Status, _currenStep, _totalSteps, _sleepSecond);
        }
    }

    class TestProgressInfo : ProgressInfoBase
    {
        public int CurrentStep;
        public int TotalStep;
        
        public TestProgressInfo(IArcJob job, string organization, int currentStep, int totalStep) : base(job, organization, DateTime.Now)
        {
            CurrentStep = currentStep;
            TotalStep = totalStep;
        }
    }
}
