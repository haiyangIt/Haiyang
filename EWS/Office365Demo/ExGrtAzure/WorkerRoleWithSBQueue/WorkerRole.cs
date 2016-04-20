using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using EwsFrame.ServiceBus;
using EwsFrame.Manager.Impl;
using EwsFrame.Util.Setting;

namespace WorkerRoleWithSBQueue
{
    public class WorkerRole : RoleEntryPoint
    {
        // The name of your queue
        const string QueueName = "ProcessingQueue";

        // QueueClient is thread-safe. Recommended that you cache 
        // rather than recreating it on every request
        QueueClient Client;
        ManualResetEvent CompletedEvent = new ManualResetEvent(false);

        public override void Run()
        {
            Trace.WriteLine("Starting processing of messages");
            OnMessageOptions options = new OnMessageOptions();
            options.AutoComplete = false;
            options.AutoRenewTimeout = TimeSpan.FromMinutes(1);

            // Initiates the message pump and callback is invoked for each message that is received, calling close on the client will stop the pump.
            Client.OnMessage((receivedMessage) =>
                {
                    try
                    {
                        // Process the message
                        Trace.WriteLine("Processing Service Bus message: " + receivedMessage.SequenceNumber.ToString());
                        var messageType = AzureServiceBusHelper.GetMessageType(receivedMessage.ContentType);
                        var body = receivedMessage.GetBody<string>();

                        switch (messageType)
                        {
                            case MessageType.Backup:
                                JobFactoryServer.Instance.JobManager.AddJob(new BackupJob(receivedMessage.Properties)); // todo if job manager end.
                                break;
                            case MessageType.Cancel:
                                break;
                            default:
                                throw new NotImplementedException();
                        }

                        receivedMessage.Complete();
                    }
                    catch
                    {
                        try {
                            // todo log.
                            receivedMessage.Abandon();
                            // Handle any message processing specific exceptions here
                        }
                        catch
                        {
                            // todo log
                        }
                    }
                }, options);

            CompletedEvent.WaitOne();
        }

        public override bool OnStart()
        {
            // todo log start time.
            // Set the maximum number of concurrent connections 
            //ServicePointManager.DefaultConnectionLimit = 12;

            Client = AzureServiceBusHelper.GetQueueClient(CloudConfig.Instance.ServiceBusQueueName);

            JobFactoryServer.OnStart();

            return base.OnStart();
        }

        public override void OnStop()
        {
            //todo log stop time. and if a message dealing, must wait. so need lock.
            // Close the connection to Service Bus Queue
            Client.Close();

            JobFactoryServer.OnStop();

            CompletedEvent.Set();
            
            base.OnStop();
        }
    }
}
