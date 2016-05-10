using DataProtectInterface;
using LogInterface;
using LoginTest.Models;
using LoginTest.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace LoginTest.Controllers
{
    [CustomErrorHandler]
    public class ProgressController : AsyncController
    {
        // GET: Progress
        public ActionResult Index(string serviceId)
        {
            if (string.IsNullOrEmpty(serviceId))
                return View(new ProgressModel());

            Guid id = new Guid(serviceId);
            return View(GetProgressModel(id));
        }

        private ProgressModel GetProgressModel(Guid serviceId)
        {
            IDataProtectProgress progress = null;
            if (JobProgressManager.Instance.TryGetValue(serviceId, out progress))
            {
                return new ProgressModel(progress, serviceId);
            }
            return new ProgressModel(serviceId);
        }

        //public JsonResult Refresh(ProgressModel model)
        //{
        //    return Json(GetProgressModel(model.ServiceId));
        //}

        public void RefreshAsync(ProgressModel model)
        {
            AsyncManager.OutstandingOperations.Increment();
            ThreadPool.QueueUserWorkItem((progressModel) =>
            {
                ProgressModel temp = progressModel as ProgressModel;
                try
                {
                    AsyncManager.Parameters["ProgressModel"] = GetProgressModel(temp.ServiceId);
                }
                catch(Exception e)
                {
                    System.Diagnostics.Trace.TraceError(e.GetExceptionDetail());
                    AsyncManager.Parameters["ProgressModel"] = new ProgressModel(temp.ServiceId);
                }
                AsyncManager.OutstandingOperations.Decrement();
            }, model);
        }

        public JsonResult RefreshCompleted(ProgressModel progressModel)
        {
            return Json(progressModel);
        }
    }
}