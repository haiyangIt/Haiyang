using DataProtectInterface;
using EwsFrame;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.WindowsAzure.Management.Scheduler.Models;
using Microsoft.WindowsAzure.Scheduler.Models;
using SqlDbImpl.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebRoleUI.Models;
using WebRoleUI.Models.Setting;
using WebRoleUI.Utils;

namespace WebRoleUI.Controllers
{
    [CustomErrorHandler]
    public class PlanController : Controller
    {
        [Authorize]
        // GET: Plan
        public ActionResult Index()
        {
            var currentUserId = User.Identity.GetUserId();
            ApplicationUser user = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(currentUserId);

            var dataAccess = PlanFactory.Instance.NewPlanDataAccess(user.Organization);
            var plans = dataAccess.GetAllPlans();

            return View(plans);
        }

        [Authorize]
        public ActionResult CreatePlan()
        {
            var currentUserId = User.Identity.GetUserId();
            ApplicationUser user = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(currentUserId);
            SettingModel settingModel = null;
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                settingModel = context.Settings.Where(s => s.UserMail == user.Email).FirstOrDefault();
            }
            OrganizationAdminInfo adminInfo = null;
            if (settingModel != null)
            {
                adminInfo.UserName = settingModel.AdminUserName;
                adminInfo.UserPassword = settingModel.AdminPassword;
            }

            var planModel = new PlanViewModel()
            {
                AdminInfo = adminInfo,
                SyncDatas = null,
                Organization = user.Organization
            };

            return View(planModel);
        }

        public JsonResult CreatePlan(PlanModel planModel,PlanAzureInfo planAzureInfo)
        {
            planModel = new PlanModel()
            {
                Name = DateTime.Now.ToString("MMddHHmmss"),
                FirstStartTime = DateTime.Now,
                Organization = "Arcserve",
                PlanMailInfos = string.Empty

            };

            planAzureInfo = new PlanAzureInfo()
            {
                CloudService = "CloudServiceForJobCollection",
                Name = "BackupTest",
                JobCollectionName = "BackupJobCollection"
            };
            var testJob = new Job();
            testJob.StartTime = DateTime.Now.AddMinutes(3);

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