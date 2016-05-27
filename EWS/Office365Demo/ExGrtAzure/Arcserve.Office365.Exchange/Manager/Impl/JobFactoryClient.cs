using Arcserve.Office365.Exchange.Manager.IF;
using Arcserve.Office365.Exchange.Util;
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

        static JobFactoryClient()
        {
            Instance = new JobFactoryClient();
            SubscriptManager = new SubScriptionManager();

            DisposeManager.RegisterInstance(SubscriptManager);
        }

        /// <summary>
        /// 
        /// </summary>
        public static readonly JobFactoryClient Instance;

        public static readonly ISubScriptionManager SubscriptManager;
    }
}
