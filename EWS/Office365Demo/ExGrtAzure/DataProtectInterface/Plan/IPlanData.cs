using Microsoft.WindowsAzure.Scheduler.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProtectInterface.Plan
{
    public interface IPlanData
    {
        string Name { get; }
        string Organization { get; }
        string CredentialInfo { get; }
        string PlanMailInfos { get; }
        DateTime FirstStartTime { get; }
        DateTime NextFullBackupTime { get; }
        string LastSyncStaus { get; }
    }

    public interface IPlanMailInfo
    {

    }

    public interface IPlanAzureInfo
    {
        Job Job { get; }
    }
}
