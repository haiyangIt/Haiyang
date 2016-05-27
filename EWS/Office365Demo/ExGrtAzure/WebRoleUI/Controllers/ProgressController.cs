using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using WebRoleUI.Models;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Azure;
using System.Threading;
using Microsoft.ServiceBus.Messaging;
using Arcserve.Office365.Exchange.ServiceBus.Relay;
using Arcserve.Office365.Exchange.Manager.Data;
using Arcserve.Office365.Exchange.Manager.IF;
using Arcserve.Office365.Exchange.Manager.Impl;
using Arcserve.Office365.Exchange.Data.Event;
using Arcserve.Office365.Exchange.ServiceBus;

namespace WebRoleUI.Controllers
{
    /// <summary>
    /// Only listen the latest job progress.
    /// 
    /// Following is something about long pooling: todo
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
            var info = JobFactoryServer.Convert<ProtectProgressInfo>(progress);
            switch (info.Job.JobType)
            {
                case ArcJobType.Backup:
                    var backupInfo = JobFactoryServer.Convert<BackupProgressInfo>(progress); // todo not only for backup, but also for restore and other jobs.
                    this.ViewData["LatestProgress"] = backupInfo.ProgressInfo.GetUIString();
                    this.ViewData["JobId"] = backupInfo.Job.JobId;
                    break;
                case ArcJobType.Restore:
                    throw new NotImplementedException();
                default:
                    throw new NotSupportedException();
            }
            return View();
        }


        public void UpdateProgressAsync(string jobId)
        {
            var filter = TopicHelper.GetFilterByJobId(jobId);
            JobFactoryClient.SubscriptManager.AddListener(filter, TopicMessageReceivedCallback);
        }

        private void TopicMessageReceivedCallback(IProgressInfo message)
        {
            AsyncManager.Parameters["progressInfo"] = message;
            AsyncManager.OutstandingOperations.Increment();
        }

        public JsonResult UpdateProgressCompleted(IProgressInfo progressInfo)
        {
            return Json(progressInfo);
        }
    }
}