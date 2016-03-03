using Microsoft.Azure;
using Microsoft.WindowsAzure.Management.Scheduler;
using Microsoft.WindowsAzure.Scheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Configuration
using Microsoft.WindowsAzure.Management.Scheduler.Models;
using Microsoft.WindowsAzure.Scheduler.Models;
using WebRoleUI.Models;
using System.Xml.Linq;
using System.IO;

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
            var subscriptionName = ConfigurationManager.AppSettings["SubscriptionNameForScheduler"];
            subscriptionId = string.Empty;
            return CertificateCloudCredentialsFactory.FromPublishSettingsFile(subscriptionName, out subscriptionId);
        }

        public static void CreateOrUpdateScheduleJob(string planName, string organization, ScheduleInfo scheduleInfo)
        {
            var cloudServiceName = GetCloudServiceName(organization);
            string subscriptionId = string.Empty;
            var cert = GetCertificate(out subscriptionId);

            var schedulerMgmCli = new SchedulerManagementClient(cert);
            var cloudServiceMgmCli = new CloudServiceManagementClient(cert);

            var cloudServiceResponse = cloudServiceMgmCli.CloudServices.Get(cloudServiceName);
            var isCloudServiceExist = cloudServiceResponse.StatusCode == System.Net.HttpStatusCode.OK;
            if (isCloudServiceExist)
            {
                schedulerMgmCli.JobCollections.Get(cloudServiceName, )
            }
            else
            {

            }
        }

        public static SchedulerClient CreateJob()
        {


            // create management credencials and cloud service management client

            
            var credentials = new CertificateCloudCredentials(subscriptionId, certificate);
            var cloudServiceMgmCli = new CloudServiceManagementClient(credentials);

            // create cloud service
            var cloudServiceCreateParameters = new CloudServiceCreateParameters()
            {
                Description = "Office365 Protection Scheduler Service",
                GeoRegion = "South Central US",
                Label = "Office365ProtectionSchedulerServiceL"
            };

            var cloudServiceName = "Office365ProtectionSchedulerService";
            var cloudService = cloudServiceMgmCli.CloudServices.Create(cloudServiceName, cloudServiceCreateParameters);

            // create job collection
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
            var jobCollectionName = "Office365ProtectionJobCollection";
            var jobCollectionCreateParameters = new JobCollectionCreateParameters()
            {
                IntrinsicSettings = jobCollectionIntrinsicSettings,
                Label = jobCollectionName
            };
            var jobCollectionCreateResponse = schedulerMgmCli.JobCollections.Create(cloudServiceName, jobCollectionName, jobCollectionCreateParameters);

            // create job
            var schedulerClient = new SchedulerClient(cloudServiceName, jobCollectionName, credentials);
            var jobAction = new JobAction()
            {
                Type = JobActionType.Http,
                Request = new JobHttpRequest()
                {
                    Uri = new Uri("http://blog.shaunxu.me"),
                    Method = "GET"
                }
            };
            var jobRecurrence = new JobRecurrence()
            {
                Frequency = JobRecurrenceFrequency.Hour,
                Interval = 1
            };
            var jobCreateOrUpdateParameters = new JobCreateOrUpdateParameters()
            {
                Action = jobAction,
                Recurrence = jobRecurrence
            };
            var jobCreateResponse = schedulerClient.Jobs.CreateOrUpdate("poll_blog", jobCreateOrUpdateParameters);
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

            foreach(var p in paths)
            {
                var file = Path.Combine(path, "credentials.publishsettings");
                if (File.Exists(file))
                    return file;
            }
            
            throw new FileNotFoundException();
        }
    }
}