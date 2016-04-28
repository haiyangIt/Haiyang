using Arcserve.Office365.Exchange.Manager.IF;
using Microsoft.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.ServiceBus.Relay
{
    /// <summary>
    /// ref : https://azure.microsoft.com/en-in/documentation/articles/service-bus-dotnet-how-to-use-relay/
    /// </summary>
    public class ServiceHostManager : IManager
    {
        private ServiceHost _serviceHost;
        public void End()
        {
            _serviceHost.Close();
        }

        public void Start()
        {
            _serviceHost = new ServiceHost(typeof(LogSolver)); // todo need modify config file.

            _serviceHost.Open();

        }
    }
}
