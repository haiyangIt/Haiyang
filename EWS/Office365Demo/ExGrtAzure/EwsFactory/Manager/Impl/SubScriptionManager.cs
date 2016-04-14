using EwsFrame.Manager.IF;
using EwsFrame.ServiceBus;
using Microsoft.Azure;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EwsFrame.Manager.Impl
{
    public class SubScriptionManager : ManagerBase, ISubScriptionManager
    {
        public SubScriptionManager() : base(2)
        {

        }

        public override string ManagerName
        {
            get
            {
                return "TopicSubscriptionManager";
            }
        }

        private Dictionary<string, SubScriptionCallback> _callbacks = new Dictionary<string, SubScriptionCallback>();
        private Queue<SubScriptionCallback> _addingQueue = new Queue<SubScriptionCallback>();
        private Queue<string> _removeQueue = new Queue<string>();

        private Dictionary<string, SubscriptionClient> _subClients = new Dictionary<string, SubscriptionClient>();

        protected override void MethodWhenTimeOutTriggerd()
        {
            AddCallback();
            RemoveCallback();
        }
        protected override void MethodWhenOtherEventTriggered(int eventIndex)
        {
            if (eventIndex == 0)
                AddCallback();
            else if (eventIndex == 1)
                RemoveCallback();
        }

        private void AddCallback()
        {
            lock (_callbacks)
            {
                while(_addingQueue.Count > 0)
                {
                    var temp = _addingQueue.Dequeue();
                    SubScriptionCallback val;
                    if (!_callbacks.TryGetValue(temp.SubscriptFilterInfo, out val))
                    {
                        _callbacks.Add(temp.SubscriptFilterInfo, temp);

                        temp.CreateClient();

                        string topicName = CloudConfigurationManager.GetSetting("ServiceBusTopicName");
                        var client = AzureServiceBusHelper.GetSubscriptionClientWithFilter(topicName, temp.SubscriptFilterInfo);

                        OnMessageOptions options = new OnMessageOptions();
                        options.AutoComplete = false;
                        options.AutoRenewTimeout = TimeSpan.FromMinutes(1);

                        client.OnMessage(DealTopicMessage);
                    }
                }
            }
        }

        private void DealTopicMessage(BrokeredMessage message)
        {

        }

        private void RemoveCallback()
        {

        }

        public void AddCallback(string subscriptFilterInfo, Action<IProgressInfo> callback)
        {
            CheckOtherEventCanExecute();

            lock (_callbacks)
            {
                SubScriptionCallback temp = new SubScriptionCallback(subscriptFilterInfo);
                temp.SetCallback(callback);
                _addingQueue.Enqueue(temp);
            }

            TriggerOtherEvent(0);
        }

        public void RemoveCallback(string subscriptionFilter)
        {
            CheckOtherEventCanExecute();
            lock (_callbacks)
            {
                _removeQueue.Enqueue(subscriptionFilter);
            }

            TriggerOtherEvent(1);
        }


        class SubScriptionCallback
        {
            public SubScriptionCallback(string subscriptFilterInfo)
            {
                SubscriptFilterInfo = subscriptFilterInfo;
            }

            public Action<IProgressInfo> ProgressCallback;
            public bool HasNew;
            public string SubscriptFilterInfo;

            public void SetCallback(Action<IProgressInfo> callback)
            {

            }

            public void RemoveCallback()
            {

            }

            internal void CreateClient()
            {
                throw new NotImplementedException();
            }
        }
    }
}
