using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EwsFrame.ServiceBus.Relay;
using Microsoft.AspNet.Identity;
using WebRoleUI.Models;
using Microsoft.AspNet.Identity.Owin;
using EwsFrame.Manager.Impl;
using EwsFrame.Manager.Data;
using EwsFrame.ServiceBus;
using Microsoft.Azure;
using System.Threading;
using Microsoft.ServiceBus.Messaging;
using EwsFrame.Manager.IF;

namespace WebRoleUI.Controllers
{
    /// <summary>
    /// Only listen the latest job progress.
    /// 
    /// Following is something about long pooling:
    /// 1. This can be replaced by client REST call of service bus topic/messageQueue. Must support CORS.
    /// 2. And, you can try to use SignalR Hub technology(http://www.asp.net/signalr/overview/getting-started/tutorial-server-broadcast-with-signalr)
    /// 3. Further more, we also need think about the cases like: using load balance or client switch network.  
    /// </summary>
    public class ProgressController : AsyncController
    {
        // GET: Progress
        public ActionResult Index()
        {
            var currentUserId = User.Identity.GetUserId();
            ApplicationUser user = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(currentUserId);
            var progress = ServiceClientHelper.Instance.GetBackupLatestProgress(user.Organization);
            var info = JobFactory.Convert<ProtectProgressInfo>(progress);
            switch (info.Job.JobType)
            {
                case EwsFrame.Manager.IF.ArcJobType.Backup:
                    var backupInfo = JobFactory.Convert<BackupProgressInfo>(progress); // todo not only for backup, but also for restore and other jobs.
                    this.ViewData["LatestProgress"] = backupInfo.ProgressInfo.GetUIString();
                    this.ViewData["JobId"] = backupInfo.Job.JobId;
                    break;
                case EwsFrame.Manager.IF.ArcJobType.Restore:
                    throw new NotImplementedException();
                default:
                    throw new NotSupportedException();
            }
            return View();
        }

        SubscriptionClient _subClient;
        int isComplete = 0;
        private object _syncObj = new object();

        public void UpdateProgressAsync(string jobId)
        {
            // 1. Create subscription
            string topicName = CloudConfigurationManager.GetSetting("ServiceBusTopicName");
            _subClient = AzureServiceBusHelper.GetSubscriptionClientForEachJob(topicName, jobId);

            OnMessageOptions options = new OnMessageOptions();
            options.AutoComplete = false;
            options.AutoRenewTimeout = TimeSpan.FromMinutes(1);

            AsyncManager.OutstandingOperations.Increment();

            _subClient.OnMessage((message) =>
            {
                try
                {
                    lock (_syncObj)     // only deal 1 progress info, if receive a message in a short period of time, ignore.
                    {
                        if (isComplete == 0)
                        {

                            TopicHelper helper = new TopicHelper(message);
                            IProgressInfo progress = helper.ProgressInfo;
                            
                            AsyncManager.Parameters["progressInfo"] = progress;
                            AsyncManager.OutstandingOperations.Decrement();
                            isComplete = 1;
                        }
                        message.Complete();
                    }
                }
                catch (Exception e)
                {
                    message.Abandon();
                }
            });
        }

        public JsonResult UpdateProgressCompleted(IProgressInfo progressInfo)
        {
            lock (_syncObj)
            {
                return Json(progressInfo);
            }
        }
    }
}