using Arcserve.Office365.Exchange.Manager.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.ServiceBus.Relay
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
