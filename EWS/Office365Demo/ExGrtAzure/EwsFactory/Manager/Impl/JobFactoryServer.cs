using EwsFrame.Manager.IF;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EwsFrame.Manager.Impl
{
    /// <summary>
    /// Please note: The factory instance and all manager instance must be used in work role not web role.
    /// work role and web role communicate each other by service bus, please use classes in EwsFrame.ServiceBus namespace.
    /// </summary>
    public class JobFactoryServer
    {
        private JobFactoryServer() { }

        /// <summary>
        /// 
        /// </summary>
        public static JobFactoryServer Instance = new JobFactoryServer();

        private static IJobManager _jobManager;
        public IJobManager JobManager
        {
            get
            {
                System.Threading.Interlocked.CompareExchange(ref _jobManager, new JobManager(), null);
                return _jobManager;
            }
        }

        private static IProgressManager _progressManager;
        public IProgressManager ProgressManager
        {
            get
            {
                System.Threading.Interlocked.CompareExchange(ref _progressManager, new ProgressManager(), null);
                return _progressManager;
            }
        }

        private static IThreadManager _threadManager;
        public IThreadManager ThreadManager
        {
            get
            {
                System.Threading.Interlocked.CompareExchange(ref _threadManager, new ThreadManager(), null);
                return _threadManager;
            }
        }

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
    }
}
