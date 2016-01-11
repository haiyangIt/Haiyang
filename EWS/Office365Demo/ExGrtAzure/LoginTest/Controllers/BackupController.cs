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
using Demo.Models.Restore;
using EwsDataInterface;
using EwsServiceInterface;
using System.Web.Script.Serialization;

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
                BackupUserMailAddress = "devO365admin@arcservemail.onmicrosoft.com",// todo  user.Email,
                BackupUserOrganization = user.Organization,
                BackupUserPassword = "Loro0237",
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
                if (model.Index == 3)
                    Run(model);
            }

            return Json(model);
        }

        public ActionResult GetAllMailbox(string mailbox, string password, string organization)
        {
            ICatalogService service = CatalogFactory.Instance.NewCatalogService(mailbox, password, null, organization);
            var allMailboxes = service.GetAllUserMailbox();

            List<Item> infos = new List<Item>(allMailboxes.Count);
            foreach (var data in allMailboxes)
            {
                if (data.MailAddress == "haiyang.ling@arcserve.com")
                {
                    infos.Add(new Item()
                    {
                        Id = data.MailAddress,
                        DisplayName = data.DisplayName,
                        ChildCount = int.MaxValue,
                        ItemType = ItemTypeStr.Mailbox,
                        OtherInformation = mailbox
                    });
                    break;
                }
            }

            return Json(new { Details = infos });
        }

        public ActionResult GetFolderInMailbox(string adminMailbox, string password, string organization, string mailbox, string parentFolderId)
        {
            ICatalogService service = CatalogFactory.Instance.NewCatalogService(adminMailbox, password, null, organization);
            
            var allFolder = service.GetFolder(mailbox, parentFolderId, false);
            var result = new List<Item>();
            foreach (var folder in allFolder)
            {
                var item = new Item()
                {
                    Id = folder.FolderId,
                    DisplayName = ((IItemBase)folder).DisplayName,
                    ChildCount = int.MaxValue,
                    ItemType = ItemTypeStr.Folder
                };
                result.Add(item);
            }

            return Json(new { Details = result });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Run(BackupModel model)
        {
            if(ModelState.IsValid)
            {
                JavaScriptSerializer js = new JavaScriptSerializer();
                LoadedTreeItem selectedItem = js.Deserialize<LoadedTreeItem>(model.BackupSelectItems);

                //return View(model);
                // todo need use job table to save job status.
                var service = CatalogFactory.Instance.NewCatalogService(model.BackupUserMailAddress, model.BackupUserPassword, null, model.BackupUserOrganization);
                IFilterItem filterObj = CatalogFactory.Instance.NewFilterItemBySelectTree(selectedItem);
                service.GenerateCatalog(filterObj);
            }

            return View(model);
        }
    }
}