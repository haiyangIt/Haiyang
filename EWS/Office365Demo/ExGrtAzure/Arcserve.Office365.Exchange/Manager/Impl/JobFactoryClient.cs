using Arcserve.Office365.Exchange.Manager.IF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Manager.Impl
{
    public class JobFactoryClient
    {
        private JobFactoryClient() { }

        /// <summary>
        /// 
        /// </summary>
        public static JobFactoryClient Instance = new JobFactoryClient();

        private static ISubScriptionManager _subscriptManager;
        public ISubScriptionManager SubscriptManager
        {
            get
            {
                System.Threading.Interlocked.CompareExchange(ref _subscriptManager, new SubScriptionManager(), null);
                return _subscriptManager;
            }
        }
    }
}
