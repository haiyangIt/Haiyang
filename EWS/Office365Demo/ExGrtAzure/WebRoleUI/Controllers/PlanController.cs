using Microsoft.WindowsAzure.Management.Scheduler.Models;
using Microsoft.WindowsAzure.Scheduler.Models;
using SqlDbImpl.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebRoleUI.Utils;

namespace WebRoleUI.Controllers
{
    public class PlanController : Controller
    {
        // GET: Plan
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult CreatePlan(PlanModel planModel,PlanAzureInfo planAzureInfo)
        {
            var testJob = new Job();
            testJob.StartTime = DateTime.Now;

            // if has schedule time, uncomment the following code.
            //testJob.Recurrence = new JobRecurrence(); 
            //testJob.Recurrence.Frequency = JobRecurrenceFrequency.Day;
            //testJob.Recurrence.Schedule = new JobRecurrenceSchedule();
            //testJob.Recurrence.Schedule.Hours = new List<int> { 8 };// at 8:00 oclock run.
            planAzureInfo.Job = testJob;
            SchedulerHelper.CreateSchdule(planModel, planAzureInfo);
            return Json(new { });
        }

        public JsonResult UpdatePlan(PlanModel planModel, string planMailInfo, PlanAzureInfo planAzureInfo)
        {
            return Json(new { });
        }

        public JsonResult DeletePlan(string planName)
        {
            return Json(new { });
        }

        public JsonResult TestPlan()
        {
            return Json(new { });
        }
    }
}