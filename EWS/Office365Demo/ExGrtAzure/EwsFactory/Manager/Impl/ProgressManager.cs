using EwsFrame.Manager.Data;
using EwsFrame.Manager.IF;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EwsFrame.Manager.Impl
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
            CheckOtherEventCanExecute();

            lock (addingProgress)
            {
                addingProgress.Enqueue(progressInfo);
            }

            TriggerOtherEvent();
        }

        public string GetLatestProgress()
        {
            return Convert(_progressCache.Last());
        }

        protected override void MethodWhenOtherEventTriggered()
        {
            AddingProgress();
        }

        protected override void MethodWhenTimeOutTriggerd()
        {
            AddingProgress();
        }

        private void AddingProgress()
        {
            lock (addingProgress)
            {
                while(addingProgress.Count > 0)
                {
                    var info = addingProgress.Dequeue();
                    _progressCache.Add(info);
                    ProgressArgs args = new ProgressArgs(info);
                    if(NewProgressEvent != null)
                    {
                        try
                        {
                            NewProgressEvent.Invoke(this, args);
                        }
                        catch(Exception e)
                        {
                            // todo log;
                        }
                    }
                }
            }
        }

        
    }

    public class OrganizationProgressManager : ManagerBase
    {
        public static OrganizationProgressManager Instance = new OrganizationProgressManager();

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
            lock (_progress)
            {
                List<IProgressInfo> result = null;
                if (!_organizationProgress.TryGetValue(organization, out result))
                {
                    return string.Empty;
                }
                return ProgressManager.Convert(result.Last());
            }
        }

        protected override void MethodWhenOtherEventTriggered()
        {
            AddProgress();
        }

        protected override void MethodWhenTimeOutTriggerd()
        {
            AddProgress();
        }

        private void AddProgress()
        {
            lock (_progress)
            {
                while(_progress.Count > 0)
                {
                    var temp = _progress.Dequeue();
                    List<IProgressInfo> result = null;
                    if(!_organizationProgress.TryGetValue(temp.Organization, out result))
                    {
                        result = new List<IProgressInfo>();
                        _organizationProgress.Add(temp.Organization, result);
                    }

                    result.Add(temp);
                }
            }
        }

        protected override void BeforeStart()
        {
            JobFactory.Instance.ProgressManager.NewProgressEvent += ProgressManagerNewProgressEvent;
        }

        private void ProgressManagerNewProgressEvent(object sender, ProgressArgs e)
        {
            CheckOtherEventCanExecute();

            lock (_progress)
            {
                _progress.Enqueue(e.ProgressInfo);
            }

            TriggerOtherEvent();
        }
    }
}
