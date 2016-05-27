using Arcserve.Office365.Exchange.Manager.IF;
using Arcserve.Office365.Exchange.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Manager.Impl
{
    /// <summary>
    /// Please note: The factory instance and all manager instance must be used in work role not web role.
    /// work role and web role communicate each other by service bus, please use classes in EwsFrame.ServiceBus namespace.
    /// </summary>
    public class JobFactoryServer
    {
        static JobFactoryServer()
        {
            Instance = new JobFactoryServer();
        }
        private JobFactoryServer() {
            JobManager = new JobManager();
            ProgressManager = new ProgressManager();
            ThreadManager = new ThreadManager();

            DisposeManager.RegisterInstance(ThreadManager);
            DisposeManager.RegisterInstance(JobManager);
            DisposeManager.RegisterInstance(ProgressManager);
        }
        
        public static readonly JobFactoryServer Instance;

        public readonly IJobManager JobManager;

        public readonly IProgressManager ProgressManager;

        public readonly IThreadManager ThreadManager;

        public IThreadObj NewThreadObj()
        {
            return new ThreadObj();
        }
        public IThreadObj NewThreadObj(string threadName)
        {
            return new ThreadObj(threadName);
        }

        public static T Convert<T>(string progressInfo) where T : IProgressInfo
        {
            return JsonConvert.DeserializeObject<T>(progressInfo);
        }

        public static IProgressInfo Convert(string progressInfo)
        {
            throw new NotImplementedException();
        }

        public static string Convert(IProgressInfo progressInfo)
        {
            return JsonConvert.SerializeObject(progressInfo);
        }

        public static void OnStart()
        {
            JobFactoryServer.Instance.ThreadManager.Start();
            JobFactoryServer.Instance.JobManager.Start();
            JobFactoryServer.Instance.ProgressManager.Start();

            OrganizationProgressManager.Instance.Start();
        }

        public static void OnStop()
        {
            OrganizationProgressManager.Instance.End();

            JobFactoryServer.Instance.ProgressManager.End();
            JobFactoryServer.Instance.JobManager.End();
            JobFactoryServer.Instance.ThreadManager.End();
        }
    }
}
