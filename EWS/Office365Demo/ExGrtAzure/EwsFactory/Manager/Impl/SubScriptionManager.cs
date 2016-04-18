using EwsFrame.Manager.IF;
using EwsFrame.ServiceBus;
using Microsoft.Azure;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EwsFrame.Manager.Impl
{
    public class SubScriptionManager : ManagerBase, ISubScriptionManager
    {
        public SubScriptionManager() : base(EventNumber)
        {

        }

        public override string ManagerName
        {
            get
            {
                return "TopicSubscriptionManager";
            }
        }

        private object _sync = new object();
        private Queue<SubScriptionCallback> _addingQueue = new Queue<SubScriptionCallback>();
        private Queue<SubScriptionCallback> _removeQueue = new Queue<SubScriptionCallback>();
        private Queue<string> _disposeQueue = new Queue<string>();

        private const int AddCallbackEventIndex = 0;
        private const int RemoveCallbackEventIndex = 1;
        private const int DisposeSubscriptEventIndex = 2;
        private const int EventNumber = 3;

        private Dictionary<string, SubScriptionClientManager> _subClients = new Dictionary<string, SubScriptionClientManager>();

        protected override void MethodWhenTimeOutTriggerd()
        {
            AddCallbackInInternalThread();
            RemoveCallbackInInternalThread();
        }
        protected override void MethodWhenOtherEventTriggered(int eventIndex)
        {
            if (eventIndex == AddCallbackEventIndex)
                AddCallbackInInternalThread();
            else if (eventIndex == RemoveCallbackEventIndex)
                RemoveCallbackInInternalThread();
            else if (eventIndex == DisposeSubscriptEventIndex)
                DisposeSubscriptInInternalThread();
        }

        private void DisposeSubscriptInInternalThread()
        {
            lock (_sync)
            {
                while (_disposeQueue.Count > 0)
                {
                    var item = _disposeQueue.Dequeue();

                    SubScriptionClientManager val;
                    if (_subClients.TryGetValue(item, out val))
                    {
                        val.ReleaseSubscription();
                    }
                }
            }
        }

        private void AddCallbackInInternalThread()
        {
            lock (_sync)
            {
                while (_addingQueue.Count > 0)
                {
                    var item = _addingQueue.Dequeue();

                    SubScriptionClientManager val;
                    if (!_subClients.TryGetValue(item.SubscriptFilterInfo, out val))
                    {
                        val = new SubScriptionClientManager(item.SubscriptFilterInfo);
                        _subClients.Add(item.SubscriptFilterInfo, val);
                    }
                    val.AddCallback(item);
                }
            }
        }

        private void RemoveCallbackInInternalThread()
        {
            lock (_sync)
            {
                while(_removeQueue.Count > 0)
                {
                    var item = _removeQueue.Dequeue();
                    SubScriptionClientManager val;
                    if (_subClients.TryGetValue(item.SubscriptFilterInfo, out val))
                    {
                        val.RemoveCallback(item.ThreadId);
                    }
                }
            }
        }

        public void AddListener(string subscriptFilterInfo, Action<IProgressInfo> callback)
        {
            CheckOtherEventCanExecute();

            lock (_sync)
            {
                SubScriptionCallback temp = new SubScriptionCallback(subscriptFilterInfo, callback);
                _addingQueue.Enqueue(temp);
            }

            TriggerOtherEvent(AddCallbackEventIndex);
        }

        public void RemoveListener(string subscriptionFilter)
        {
            CheckOtherEventCanExecute();
            lock (_sync)
            {
                _removeQueue.Enqueue(SubScriptionCallback.GetSubScriptionCallbackForRemove(subscriptionFilter));
            }

            TriggerOtherEvent(RemoveCallbackEventIndex);
        }

        public void DisposeSubScript(string subscriptionFilter)
        {
            CheckOtherEventCanExecute();
            lock (_sync)
            {
                _disposeQueue.Enqueue(subscriptionFilter);
            }
            TriggerOtherEvent(DisposeSubscriptEventIndex);
        }

        class SubScriptionCallback
        {
            public SubScriptionCallback(string subscriptFilterInfo, Action<IProgressInfo> callback)
            {
                ThreadId = Thread.CurrentThread.ManagedThreadId;
                SubscriptFilterInfo = subscriptFilterInfo;
                Callback = callback;
            }

            private SubScriptionCallback(string subscriptFilterInfo)
            {
                ThreadId = Thread.CurrentThread.ManagedThreadId;
                SubscriptFilterInfo = subscriptFilterInfo;
            }

            public static SubScriptionCallback GetSubScriptionCallbackForRemove(string subscriptFilterInfo)
            {
                return new SubScriptionCallback(subscriptFilterInfo);
            }

            public readonly int ThreadId;
            public readonly string SubscriptFilterInfo;
            public readonly Action<IProgressInfo> Callback;
        }

        class SubScriptionClientManager
        {
            private Dictionary<int, SubScriptionCallback> CallbackList = new Dictionary<int, SubScriptionCallback>();

            public string SubscriptFilterInfo;
            public SubscriptionClient SubClient;
            private TopicHelper _latestTopicHelper;

            public SubScriptionClientManager(string subscriptFilterInfo)
            {
                SubscriptFilterInfo = subscriptFilterInfo;
                CreateClient();
            }

            public void AddCallback(SubScriptionCallback callback)
            {
                lock (CallbackList)
                {
                    if(!CallbackList.ContainsKey(callback.ThreadId))
                    {
                        CallbackList.Add(callback.ThreadId, callback);
                        if(_latestTopicHelper != null)
                        {
                            callback.Callback.Invoke(_latestTopicHelper.ProgressInfo);
                        }
                    }
                }
            }

            public void RemoveCallback(int threadId)
            {
                lock (CallbackList)
                {
                    if (CallbackList.ContainsKey(threadId))
                    {
                        CallbackList.Remove(threadId);
                    }
                }
            }

            private void CreateClient()
            {
                string topicName = CloudConfigurationManager.GetSetting("ServiceBusTopicName");
                SubClient = AzureServiceBusHelper.GetSubscriptionClientWithFilter(topicName, SubscriptFilterInfo);

                OnMessageOptions options = new OnMessageOptions();
                options.AutoComplete = false;
                options.AutoRenewTimeout = TimeSpan.FromMinutes(1);

                SubClient.OnMessage(DealTopicMessage);
            }

            private void DealTopicMessage(BrokeredMessage message)
            {
                try
                {
                    var topicHelper = new TopicHelper(message);
                    // todo if job is finished, please release subscription. and in manager, must remove this object some times later.
                    foreach (var callback in CallbackList.Values)
                    {
                        try
                        {
                            callback.Callback.Invoke(topicHelper.ProgressInfo);
                        }
                        catch (Exception e)
                        {
                            // todo log;
                        }
                    }
                    Interlocked.Exchange<TopicHelper>(ref _latestTopicHelper, topicHelper);
                    message.Complete();
                    
                }
                catch (Exception e)
                {
                    message.Abandon();
                }
            }

            internal void ReleaseSubscription()
            {
                string topicName = CloudConfigurationManager.GetSetting("ServiceBusTopicName");
                AzureServiceBusHelper.DeleteSubscriptionClientWithFilter(topicName, SubscriptFilterInfo);
            }
        }
    }
}
