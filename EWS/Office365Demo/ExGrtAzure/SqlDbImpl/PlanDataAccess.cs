using DataProtectInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataProtectInterface.Plan;
using SqlDbImpl.Model;

namespace SqlDbImpl
{
    public class PlanDataAccess : CatalogDataAccessBase, IPlanDataAccess
    {
        public PlanDataAccess(string organization) : base(organization)
        {
        }

        public int GetJobCountInCollection(string cloudService, string jobCollectionName)
        {
            return QueryData<IPlanData>((context) =>
            {
                return (from plan in context.PlanAzureInfos
                        where plan.JobCollectionName == jobCollectionName && plan.CloudService == cloudService
                        select plan).Count();
            });
        }

        public IPlanData GetPlan(string name)
        {
            return QueryData<IPlanData>((context) =>
            {
                return from plan in context.PlanModels
                where plan.Name == name
                select plan;
            });
        }

        public void InsertPlanAzureInfo(IPlanAzureInfo planAzureInfo)
        {
            SaveModel<IPlanAzureInfo, PlanAzureInfo>(planAzureInfo,
                (context, list) => { context.PlanAzureInfos.AddRange(list); });
        }

        public void InsertPlanModel(IPlanData planModel)
        {
            SaveModel<IPlanData, PlanModel>(planModel,
                 (context, list) => { context.PlanModels.AddRange(list); });
        }

        public bool IsCloudServiceExist(string cloudService)
        {
            var planAzureInfo = QueryData<IPlanAzureInfo>((context) =>
            {
                return from plan in context.PlanAzureInfos where plan.CloudService == cloudService select plan;
            });
            return planAzureInfo != null;
        }

        public bool IsJobCollectionExist(string cloudService, string jobCollectionName)
        {
            var planAzureInfo = QueryData<IPlanAzureInfo>((context) =>
            {
                return from plan in context.PlanAzureInfos where plan.CloudService == cloudService && plan.JobCollectionName == jobCollectionName select plan;
            });
            return planAzureInfo != null;
        }
    }
}
