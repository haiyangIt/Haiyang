using DataProtectInterface;
using EwsFrame;
using LoginTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

namespace LoginTest.Controllers
{
    public class BackupController : Controller
    {
        [Authorize]
        // GET: Backup
        public ActionResult Index()
        {
            var currentUserId = User.Identity.GetUserId();
            ApplicationUser user = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(currentUserId);
            var backupModel = new BackupModel()
            {
                BackupUserMailAddress = user.Email,
                BackupUserOrganization = user.Organization,
                BackupUserPassword = "abc",
                Index = 0
            };
            return View(backupModel);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(BackupModel model)
        {
            if (ModelState.IsValid)
            {
                model.Index++;
                if (model.Index == 2)
                    Run(model);
            }

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Run(BackupModel model)
        {
            if(ModelState.IsValid)
            {
                // todo need use job table to save job status.
                ICatalogService service = CatalogFactory.Instance.NewCatalogService(model.BackupUserMailAddress, model.BackupUserPassword, null, model.BackupUserOrganization);

                service.GenerateCatalog();
            }

            return View(model);
        }
    }
}