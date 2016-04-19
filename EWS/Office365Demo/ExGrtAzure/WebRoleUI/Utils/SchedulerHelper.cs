using Microsoft.Azure;
using Microsoft.WindowsAzure.Management.Scheduler;
using Microsoft.WindowsAzure.Scheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Configuration;
using Microsoft.WindowsAzure.Management.Scheduler.Models;
using Microsoft.WindowsAzure.Scheduler.Models;
using System.Xml.Linq;
using System.IO;
using SqlDbImpl.Model;
using EwsFrame;
using DataProtectInterface.Plan;
using Microsoft.ServiceBus;
using EwsFrame.Util;
using EwsFrame.ServiceBus;
using EwsFrame.Util.Setting;

namespace WebRoleUI.Utils
{
    public class SchedulerHelper
    {
        private static string GetCloudServiceName(string organization)
        {
            return string.Format("{0}_CloudService", organization);
        }

        private static CertificateCloudCredentials GetCertificate(out string subscriptionId)
        {
            var subscriptionName = CloudConfig.Instance.SubscriptionNameForAzure;
            subscriptionId = string.Empty;
            return CertificateCloudCredentialsFactory.FromPublishSettingsFile(subscriptionName, out subscriptionId);
        }

        private static readonly int JobMaxCountInStandardCollection = 50;
        public static void CreateSchdule(PlanModel planModel, PlanAzureInfo planAzureInfo)
        {
            planAzureInfo.Job.Action = CreateBackupJobAction(planModel, planAzureInfo);
            planAzureInfo.Job.Id = planModel.Name;

            var planDataAccess = PlanFactory.Instance.NewPlanDataAccess(planModel.Organization);
            // 1. Check the plan name is exist.
            IPlanData planData = planDataAccess.GetPlan(planModel.Name);
            if (planData != null)
                throw new ArgumentException(string.Format("Plan {0} exists, cannot create.", planModel.Name));

            string subscriptionId = string.Empty;
            CertificateCloudCredentials cert = GetCertificate(out subscriptionId);

            // 2. Check the cloud service name is exist.
            bool isCloudServiceExist = planDataAccess.IsCloudServiceExist(planAzureInfo.CloudService);
            bool isJobCollectionExist = false;
            int jobCountInCollection = 0;
            if (!isCloudServiceExist)
            {
                CreateCloudService(planAzureInfo.CloudService, cert);
            }
            else
            {
                // 3. Check the collection is exist.
                isJobCollectionExist = planDataAccess.IsJobCollectionExist(planAzureInfo.CloudService, planAzureInfo.JobCollectionName);
                if (!isJobCollectionExist)
                {
                    CreateJobCollection(planAzureInfo.CloudService, planAzureInfo.JobCollectionName, cert);
                }
            }
            if (isJobCollectionExist)
            {
                jobCountInCollection = planDataAccess.GetJobCountInCollection(planAzureInfo.CloudService, planAzureInfo.JobCollectionName);
            }

            // 4. Check the collection count is valid. 
            // The default max jobs quota is 5 jobs in a free job collection and 50 jobs in a standard job collection.
            if (jobCountInCollection >= JobMaxCountInStandardCollection)
            {
                planAzureInfo.JobCollectionName = string.Format("{0}{1}", planModel.Organization, DateTime.Now.ToString("yyyyMMddHHmmss"));
                CreateJobCollection(planAzureInfo.CloudService, planAzureInfo.JobCollectionName, cert);
            }

            // 5. Create Job
            CreateJob(planAzureInfo.CloudService, planAzureInfo.JobCollectionName, cert, planAzureInfo.Job);
            // 6. Insert data to Db.
            planDataAccess.InsertPlanModel(planModel);
            planDataAccess.InsertPlanAzureInfo(planAzureInfo);
        }

        private static void CreateJob(string cloudServiceName, string jobCollectionName, CertificateCloudCredentials credentials, Job job)
        {
            var schedulerClient = new SchedulerClient(cloudServiceName, jobCollectionName, credentials);
            var jobAction = job.Action;
            var jobRecurrence = job.Recurrence; 
            var jobCreateOrUpdateParameters = new JobCreateOrUpdateParameters()
            {
                Action = jobAction,
                Recurrence = jobRecurrence,
                StartTime = job.StartTime
            };
            var jobCreateResponse = schedulerClient.Jobs.CreateOrUpdate(job.Id, jobCreateOrUpdateParameters);
            if (jobCreateResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new HttpException(string.Format("Create job [{0}] failed, status code: [{1}]."
                    , job.Id, jobCreateResponse.StatusCode));
            }
            

        }

        private static JobAction CreateBackupJobAction(PlanModel planModel, PlanAzureInfo planAzureInfo)
        {
            string connectionString = CloudConfig.Instance.ServiceBusConnectionString;
            ServiceBusConnectionStringBuilder parser = new ServiceBusConnectionStringBuilder(connectionString);

            var jobAction = new JobAction(JobActionType.ServiceBusQueue);
            var serviceBusQueueMessage = new JobServiceBusQueueMessage();
            serviceBusQueueMessage.Authentication = new JobServiceBusAuthentication();
            serviceBusQueueMessage.Authentication.SasKeyName = parser.SharedAccessKeyName;
            serviceBusQueueMessage.Authentication.SasKey = parser.SharedAccessKey;
            serviceBusQueueMessage.TransportType = JobServiceBusTransportType.AMQP;
            serviceBusQueueMessage.Namespace = CloudConfig.Instance.SubscriptionNameForAzure;
            serviceBusQueueMessage.QueueName = CloudConfig.Instance.ServiceBusQueueName;
            serviceBusQueueMessage.Message = planModel.Name;
            var keyValue = new Dictionary<string, string>();
            keyValue["planBaseInfo"] = planModel.ToString();
            //keyValue["planMailInfo"] = PlanUtil.GetMailString(planMailInfo); //todo can delete
            keyValue["planJobStartTime"] = DateTime.UtcNow.Ticks.ToString();
            serviceBusQueueMessage.CustomMessageProperties = keyValue;

            var brokeredMessageProp = new JobServiceBusBrokeredMessageProperties();
            brokeredMessageProp.ContentType = AzureServiceBusHelper.GetContentTypeByMessageType(MessageType.Backup);
            serviceBusQueueMessage.BrokeredMessageProperties = brokeredMessageProp;
            
            jobAction.ServiceBusQueueMessage = serviceBusQueueMessage;
            return jobAction;
        }

        private static void CreateCloudService(string cloudServiceName, CertificateCloudCredentials credentials)
        {
            var cloudServiceMgmCli = new CloudServiceManagementClient(credentials);

            // create cloud service
            var cloudServiceCreateParameters = new CloudServiceCreateParameters()
            {
                Description = "Office365 Protection Scheduler Service",
                GeoRegion = "East Asia",
                Label = cloudServiceName + "Label"
            };

            var cloudServiceResponse = cloudServiceMgmCli.CloudServices.Create(cloudServiceName, cloudServiceCreateParameters);
            if (cloudServiceResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new HttpException(string.Format("Create cloud service [{0}] failed, error code: [{1}], error message:[{2}], status code: [{2}]."
                    , cloudServiceName, cloudServiceResponse.Error.Code, cloudServiceResponse.Error.Message, cloudServiceResponse.HttpStatusCode));
            }
        }

        private static void CreateJobCollection(string cloudServiceName, string jobCollectionName, CertificateCloudCredentials credentials)
        {
            var schedulerMgmCli = new SchedulerManagementClient(credentials);
            var jobCollectionIntrinsicSettings = new JobCollectionIntrinsicSettings()
            {
                Plan = JobCollectionPlan.Standard
                //,Quota = new JobCollectionQuota()
                //{
                //    MaxJobCount = 5,
                //    MaxJobOccurrence = 1,
                //    MaxRecurrence = new JobCollectionMaxRecurrence()
                //    {
                //        Frequency = JobCollectionRecurrenceFrequency.Hour,
                //        Interval = 1
                //    }
                //}
            };
            var jobCollectionCreateParameters = new JobCollectionCreateParameters()
            {
                IntrinsicSettings = jobCollectionIntrinsicSettings,
                Label = jobCollectionName
            };
            var jobCollectionCreateResponse = schedulerMgmCli.JobCollections.Create(cloudServiceName, jobCollectionName, jobCollectionCreateParameters);
            if (jobCollectionCreateResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new HttpException(string.Format("Create job collection [{0}] failed, error code: [{1}], error message:[{2}], status code: [{2}]."
                    , jobCollectionName, jobCollectionCreateResponse.Error.Code, jobCollectionCreateResponse.Error.Message, jobCollectionCreateResponse.HttpStatusCode));
            }
        }
    }

    public static class CertificateCloudCredentialsFactory
    {
        public static CertificateCloudCredentials FromPublishSettingsFile(string subscriptionName, out string subscriptionId)
        {
            var path = GetPath();
            var profile = XDocument.Load(path);
            subscriptionId = profile.Descendants("Subscription")
                .First(element => element.Attribute("Name").Value == subscriptionName)
                .Attribute("Id").Value;
            var certificate = new X509Certificate2(
                Convert.FromBase64String(profile.Descendants("PublishProfile").Descendants("Subscription").Single().Attribute("ManagementCertificate").Value));
            return new CertificateCloudCredentials(subscriptionId, certificate);
        }

        private static string GetPath()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            List<string> paths = new List<string>();
            paths.Add(path);
            paths.Add(Path.Combine(path, "bin"));
            paths.Add(Path.Combine("..", path));

            foreach (var p in paths)
            {
                var file = Path.Combine(path, "credentials.publishsettings");
                if (File.Exists(file))
                    return file;
            }

            throw new FileNotFoundException();
        }
    }
}