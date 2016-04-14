using EwsFrame.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace EwsFrame.ServiceBus.Relay
{
    public class ServiceClientHelper : ILogSolver
    {
        public static ServiceClientHelper Instance = new ServiceClientHelper();

        private ServiceClientHelper() { }

        public string GetBackupLatestProgress(string organization)
        {
            var cf = new ChannelFactory<ILogSolverChannel>("solver");
            using (var ch = cf.CreateChannel())
            {
                return ch.GetBackupLatestProgress(organization);
            }
        }
    }
}
