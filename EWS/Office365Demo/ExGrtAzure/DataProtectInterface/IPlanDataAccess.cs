using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataProtectInterface.Plan;

namespace DataProtectInterface
{
    public interface IPlanDataAccess
    {
        IPlanData GetPlan(string name);
        bool IsCloudServiceExist(string cloudService);
        bool IsJobCollectionExist(string cloudService, string jobCollectionName);
        int GetJobCountInCollection(string cloudService, string jobCollectionName);
        void InsertPlanModel(IPlanData planModel);
        void InsertPlanAzureInfo(IPlanAzureInfo planAzureInfo);
    }
}
