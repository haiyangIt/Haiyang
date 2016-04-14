using DataProtectInterface.Event;
using EwsFrame.Manager.IF;
using EwsFrame.Manager.Impl;
using Microsoft.Azure;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EwsFrame.ServiceBus
{
    public class AzureServiceBusHelper
    {
        //public static void CreateBackupStartMessageQueue(string planName)
        //{
        //    // 1. Message Queue contains following information:
        //    //     Administrator credential.
        //    //     Backup Mail Info
        //    //     Plan Name
        //    //     Organization
        //    // 2. Scheduler send message and work role receive message.

        //}

        public static void SendBackupProgressTopic(IProgressInfo progress)
        {
            // 1. Topic has following information:
            //     CurrentRunningJob Unique Id( for this id can cancel the job)
            //     Progress Information
            //     Plan Name
            string topicName = CloudConfigurationManager.GetSetting("ServiceBusTopicName");
            var topicClient = GetTopicClient(topicName);
            TopicHelper helper = new TopicHelper(progress);
            BrokeredMessage message = helper.Message;
            message.ContentType = GetContentTypeByMessageType(MessageType.BackupProgress);
            topicClient.Send(message);
        }

        public static void CreateBackupHistoryLogRelay()
        {

        }

        public static void CreateCancelJobMessageQueue()
        {
            // 1. Message Queue structure:
            //     RunningJob Unique Id.

        }


        private static void SendMessageToQueue(string queueName, BrokeredMessage message)
        {
            string connectionString =
    CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");

            CreateQueue(queueName);

            QueueClient Client =
                QueueClient.CreateFromConnectionString(connectionString, queueName);

            Client.Send(message);
        }


        public static void CreateQueue(string queueName)
        {
            // Create a new queue with custom settings.
            string connectionString =
                CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");

            var namespaceManager =
                NamespaceManager.CreateFromConnectionString(connectionString);

            if (!namespaceManager.QueueExists(queueName))
            {
                // Configure queue settings.
                QueueDescription qd = new QueueDescription(queueName);
                qd.MaxSizeInMegabytes = Convert.ToInt32(CloudConfigurationManager.GetSetting("ServiceBusQueueMaxSize"));
                var minute = Convert.ToInt32(CloudConfigurationManager.GetSetting("ServiceBusQueueTTL"));
                qd.DefaultMessageTimeToLive = new TimeSpan(0, minute, 0);
                namespaceManager.CreateQueue(qd);
            }
        }
        public static QueueClient GetQueueClient(string queueName)
        {
            string connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);

            CreateQueue(queueName);

            // Initialize the connection to Service Bus Queue
            return QueueClient.CreateFromConnectionString(connectionString, queueName);
        }

        public static void CreateTopic(string topicName)
        {
            // Create a new queue with custom settings.
            string connectionString =
                CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");

            var namespaceManager =
                NamespaceManager.CreateFromConnectionString(connectionString);

            if (!namespaceManager.TopicExists(topicName))
            {
                // Configure queue settings.
                TopicDescription qd = new TopicDescription(topicName);
                qd.MaxSizeInMegabytes = Convert.ToInt32(CloudConfigurationManager.GetSetting("ServiceBusTopicMaxSize"));
                var minute = Convert.ToInt32(CloudConfigurationManager.GetSetting("ServiceBusTopicTTL"));
                qd.DefaultMessageTimeToLive = new TimeSpan(0, minute, 0);
                namespaceManager.CreateTopic(qd);
            }
        }

        public static TopicClient GetTopicClient(string topicName)
        {
            string connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);

            CreateTopic(topicName);

            // Initialize the connection to Service Bus Queue
            return TopicClient.CreateFromConnectionString(connectionString, topicName);
        }

        public static SubscriptionClient GetSubscriptionClientForEachJob(string topicName, string jobId)
        {
            string connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
            CreateSubscriptionWithJobId(topicName, jobId);
            return SubscriptionClient.CreateFromConnectionString(connectionString, topicName, jobId);
        }

        public static SubscriptionClient GetSubscriptionClientWithFilter(string topicName, string filter)
        {
            string connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
            CreateSubscriptionWithFilter(topicName, filter);
            return SubscriptionClient.CreateFromConnectionString(connectionString, topicName, filter);
        }

        public static void CreateSubscriptionWithFilter(string topicName, string filter)
        {
            string connectionString =
    CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");

            var namespaceManager =
                NamespaceManager.CreateFromConnectionString(connectionString);

            if (namespaceManager.SubscriptionExists(topicName, filter))
            {
                return;
            }

            SqlFilter sqlFilter = new SqlFilter(filter);

            namespaceManager.CreateSubscription(topicName, filter,
               sqlFilter);
            return;
        }

        public static void CreateSubscriptionWithJobId(string topicName, string jobId)
        {
            string connectionString =
    CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");

            var namespaceManager =
                NamespaceManager.CreateFromConnectionString(connectionString);

            if (namespaceManager.SubscriptionExists(topicName, jobId))
            {
                return;
            }

            SqlFilter filter =
   new SqlFilter(string.Format("JobId = {0}", jobId));

            namespaceManager.CreateSubscription(topicName, jobId,
               filter);
            return;
        }

        public static MessageType GetMessageType(string contentType)
        {
            return _typeStrKey[contentType];
        }

        private static Dictionary<MessageType, string> _typeKeyStr = new Dictionary<MessageType, string>()
        {
            {MessageType.Backup, "Backup" },
            {MessageType.Cancel, "Cancel" },
            {MessageType.GetLogHistory,"GetLogHistory" },
            {MessageType.BackupProgress,"BackupProgress" }
        };

        private static Dictionary<string, MessageType> _typeStrKey = new Dictionary<string, MessageType>()
        {
            { "Backup" ,MessageType.Backup },
            { "Cancel" ,MessageType.Cancel  },
            { "GetLogHistory",MessageType.GetLogHistory },
             {"BackupProgress",MessageType.BackupProgress }
        };

        public static string GetContentTypeByMessageType(MessageType messageType)
        {
            return _typeKeyStr[messageType];
        }
    }

    public enum MessageType
    {
        Backup,
        Cancel,
        BackupProgress,
        RestoreProgress,
        GetLogHistory
    };
}
