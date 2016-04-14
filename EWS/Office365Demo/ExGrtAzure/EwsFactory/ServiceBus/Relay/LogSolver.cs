using EwsFrame.Manager.Impl;
using EwsFrame.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EwsFrame.ServiceBus.Relay
{
    internal class LogSolver : ILogSolver
    {
        public string GetBackupLatestProgress(string organization)
        {
            return OrganizationProgressManager.Instance.GetLatestProgressInfo(organization);
        }

        public string GetLatestProgress(string organization, string jobId)
        {
            throw new NotImplementedException();
        }
    }
}
