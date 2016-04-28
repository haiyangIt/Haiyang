using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arcserve.Office365.Exchange.Data.Plan;

namespace Arcserve.Office365.Exchange.StorageAccess.Interface
{
    public interface IPlanDataAccess
    {
        IPlanData GetPlan(string name);
        bool IsCloudServiceExist(string cloudService);
        bool IsJobCollectionExist(string cloudService, string jobCollectionName);
        int GetJobCountInCollection(string cloudService, string jobCollectionName);
        void InsertPlanModel(IPlanData planModel);
        void InsertPlanAzureInfo(IPlanAzureInfo planAzureInfo);
        List<IPlanData> GetAllPlans();
    }
}
