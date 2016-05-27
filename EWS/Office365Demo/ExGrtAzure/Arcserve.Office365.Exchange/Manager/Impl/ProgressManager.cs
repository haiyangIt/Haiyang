using Arcserve.Office365.Exchange.Manager.Data;
using Arcserve.Office365.Exchange.Manager.IF;
using Arcserve.Office365.Exchange.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Manager.Impl
{
    internal class ProgressManager : ManagerBase, IProgressManager
    {
        public override string ManagerName
        {
            get
            {
                return "ProgressManager";
            }
        }

        public event EventHandler<ProgressArgs> NewProgressEvent;

        List<IProgressInfo> _progressCache = new List<IProgressInfo>();
        Queue<IProgressInfo> addingProgress = new Queue<IProgressInfo>();

        public void AddProgress(IProgressInfo progressInfo)
        {
            if (!CheckOtherEventCanExecute())
            {
                return;
            }

            using (addingProgress.LockWhile(() =>
            {
                addingProgress.Enqueue(progressInfo);
            }))
            { }

            TriggerOtherEvent(0);
        }

        public string GetLatestProgress()
        {
            return JobFactoryServer.Convert(_progressCache.Last());
        }

        protected override void MethodWhenOtherEventTriggered(int eventIndex)
        {
            AddingProgress();
        }

        protected override void MethodWhenTimeOutTriggerd()
        {
            AddingProgress();
        }

        private void AddingProgress()
        {
            using (addingProgress.LockWhile(() =>
            {
                while (addingProgress.Count > 0)
                {
                    var info = addingProgress.Dequeue();
                    _progressCache.Add(info);
                    ProgressArgs args = new ProgressArgs(info);
                    if (NewProgressEvent != null)
                    {
                        try
                        {
                            NewProgressEvent.Invoke(this, args);
                        }
                        catch (Exception e)
                        {
                            // todo log;
                        }
                    }
                }
            }))
            { }
        }


    }

    public class OrganizationProgressManager : ManagerBase
    {
        static OrganizationProgressManager()
        {
            Instance = new OrganizationProgressManager();

            DisposeManager.RegisterInstance(Instance);
        }
        public static readonly OrganizationProgressManager Instance;

        private OrganizationProgressManager() : base()
        {

        }

        Dictionary<string, List<IProgressInfo>> _organizationProgress = new Dictionary<string, List<IProgressInfo>>();
        Queue<IProgressInfo> _progress = new Queue<IProgressInfo>();

        public override string ManagerName
        {
            get
            {
                return "OrganizationProgressManager";
            }
        }

        public string GetLatestProgressInfo(string organization)
        {
            var temp = string.Empty;
            using (_progress.LockWhile(() =>
            {
                List<IProgressInfo> result = null;
                if (!_organizationProgress.TryGetValue(organization, out result))
                {
                    temp = string.Empty;
                }
                else
                    temp = JobFactoryServer.Convert(result.Last());
            }))
            { }
            return temp;
        }

        protected override void MethodWhenOtherEventTriggered(int index)
        {
            AddProgress();
        }

        protected override void MethodWhenTimeOutTriggerd()
        {
            AddProgress();
        }

        private void AddProgress()
        {
            using (_progress.LockWhile(() =>
            {
                while (_progress.Count > 0)
                {
                    var temp = _progress.Dequeue();
                    List<IProgressInfo> result = null;
                    if (!_organizationProgress.TryGetValue(temp.Organization, out result))
                    {
                        result = new List<IProgressInfo>();
                        _organizationProgress.Add(temp.Organization, result);
                    }

                    result.Add(temp);
                }
            }))
            { }
        }

        protected override void BeforeStart()
        {
            JobFactoryServer.Instance.ProgressManager.NewProgressEvent += ProgressManagerNewProgressEvent;
        }

        private void ProgressManagerNewProgressEvent(object sender, ProgressArgs e)
        {
            if (!CheckOtherEventCanExecute())
                return;

            using (_progress.LockWhile(() =>
            {
                _progress.Enqueue(e.ProgressInfo);
            }))
            { }

            TriggerOtherEvent(0);
        }
    }
}
