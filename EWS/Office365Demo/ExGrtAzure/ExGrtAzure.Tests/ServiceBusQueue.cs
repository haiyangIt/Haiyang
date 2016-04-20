using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.ServiceBus.Messaging;
using EwsFrame.Util.Setting;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Diagnostics;

namespace ExGrtAzure.Tests
{
    [TestClass]
    public class ServiceBusQueue
    {
        class Data
        {
            public string name;
            public string firstName;
        }

        QueueClient sendClient;
        QueueClient receiveClient;
        AutoResetEvent[] sendEvents;
        AutoResetEvent[] receiveEvents;
        Data[] data = new Data[]
            {
                new Data(){name = "Einstein", firstName = "Albert"},
                new Data() {name = "Heisenberg", firstName = "Werner"},
                new Data() {name = "Curie", firstName = "Marie"},
                new Data() {name = "Hawking", firstName = "Steven"},
                new Data() {name = "Newton", firstName = "Isaac"},
                new Data() {name = "Bohr", firstName = "Niels"},
                new Data() {name = "Faraday", firstName = "Michael"},
                new Data() {name = "Galilei", firstName = "Galileo"},
                new Data() {name = "Kepler", firstName = "Johannes"},
                new Data() {name = "Kopernikus", firstName = "Nikolaus"}
            };

        [TestMethod]
        public void MessageQueueTest()
        {
            Thread t = new Thread(TestThread);
            t.Start();

            Thread.Sleep(60 * 1000 * 1000);
        }

        [TestMethod]
        public void MessageQueueReceived()
        {
            Thread t = new Thread(ReceiveThread);
            t.Start();

            while (true)
            {
                Thread.Sleep(10 * 1000);
                break;
            }
            receiveClient.Close();
        }

        private void ReceiveThread()
        {
            var connectionString = CloudConfig.Instance.ServiceBusConnectionString;
            var queueName = CloudConfig.Instance.ServiceBusQueueName;

            receiveClient = QueueClient.CreateFromConnectionString(connectionString, queueName, ReceiveMode.PeekLock);
            this.receiveClient.OnMessage(
               message =>
               {
                   lock (data)
                   {
                       try
                       {
                           Debug.WriteLine(message.ContentType);

                           message.Complete();
                       }
                       catch (Exception e)
                       {

                       }
                   }

               },
               new OnMessageOptions { AutoComplete = false, MaxConcurrentCalls = 1 });
        }

        void TestThread()
        {
            var connectionString = CloudConfig.Instance.ServiceBusConnectionString;
            var queueName = CloudConfig.Instance.ServiceBusQueueName;

            receiveClient = QueueClient.CreateFromConnectionString(connectionString, queueName, ReceiveMode.PeekLock);
            InitializeReceiver();

            sendClient = QueueClient.CreateFromConnectionString(connectionString, queueName);
            this.SendMessagesAsync();

            WaitHandle.WaitAll(sendEvents);
            WaitHandle.WaitAll(receiveEvents);

            // shut down the receiver, which will stop the OnMessageAsync loop
            receiveClient.Close();

            sendClient.Close();
        }

        void SendMessagesAsync()
        {
            int length = ((Array)data).Length;
            sendEvents = new AutoResetEvent[length];
            for (int i = 0; i < length; i++)
            {
                sendEvents[i] = new AutoResetEvent(false);
            }


            for (int i = 0; i < data.Length; i++)
            {
                var message = new BrokeredMessage(new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data[i]))))
                {
                    ContentType = "application/json",
                    Label = "Scientist",
                    MessageId = i.ToString(),
                    TimeToLive = TimeSpan.FromMinutes(2)
                };

                this.sendClient.Send(message);
                sendEvents[i].Set();
                Debug.WriteLine(string.Format("Message sent: Id = {0}", message.MessageId));
                Debug.WriteLine("");
            }
        }

        void InitializeReceiver()
        {
            int length = ((Array)data).Length;
            receiveEvents = new AutoResetEvent[length];
            for (int i = 0; i < length; i++)
            {
                receiveEvents[i] = new AutoResetEvent(false);
            }

            int receiveIndex = 0;
            // register the OnMessageAsync callback
            this.receiveClient.OnMessage(
                message =>
                {
                    lock (receiveEvents)
                    {
                        try {
                            if (message.Label != null &&
                            message.ContentType != null &&
                            message.Label.Equals("Scientist", StringComparison.InvariantCultureIgnoreCase) &&
                            message.ContentType.Equals("application/json", StringComparison.InvariantCultureIgnoreCase))
                            {
                                var body = message.GetBody<Stream>();

                                Data scientist = JsonConvert.DeserializeObject<Data>(new StreamReader(body, true).ReadToEnd());
                                Debug.WriteLine(string.Format(
                                    "Message received: MessageId = {0}, \\n\\t\\t\\t\\t\\t\tSequenceNumber = {1}, \\n\\t\\t\\t\\t\\t\tEnqueuedTimeUtc = {2}," +
                                    "\\n\\t\\t\\t\\t\\t\tExpiresAtUtc = {5}, \\n\\t\\t\\t\\t\\t\tContentType = \"{3}\", \\n\\t\\t\\t\\t\\t\tSize = {4},  \\n\\t\\t\\t\\t\\t\tContent: [ firstName = {6}, name = {7} ]",
                                    message.MessageId,
                                    message.SequenceNumber,
                                    message.EnqueuedTimeUtc,
                                    message.ContentType,
                                    message.Size,
                                    message.ExpiresAtUtc,
                                    scientist.firstName,
                                    scientist.name));
                                Debug.WriteLine("");
                            }

                            message.Complete();
                        }
                        catch(Exception e)
                        {

                        }

                        receiveEvents[receiveIndex].Set();
                        Interlocked.Increment(ref receiveIndex);
                    }

                },
                new OnMessageOptions { AutoComplete = false, MaxConcurrentCalls = 1 });
        }
    }
}
