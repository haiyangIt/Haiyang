using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace EwsFrame.ServiceBus.Relay
{
    [ServiceContract]
    public interface ILogSolver
    {
        [OperationContract]
        string GetBackupLatestProgress(string organization);
    }

    public interface ILogSolverChannel : ILogSolver, IClientChannel { }
}
