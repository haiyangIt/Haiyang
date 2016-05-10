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
using LoginTest.Models.Setting;
using LoginTest.Utils;
using System.Threading;
using LogInterface;

namespace LoginTest.Controllers
{
    [CustomErrorHandler]
    public class BackupController : Controller
    {
        [Authorize]
        // GET: Backup
        public ActionResult Index()
        {
            var currentUserId = User.Identity.GetUserId();
            ApplicationUser user = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(currentUserId);

            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                SettingModel model = context.Settings.Where(s => s.UserMail == user.Email).FirstOrDefault();
                if (model == null)
                {
                    return View(new BackupModel());
                }
                else
                {
                    var backupModel = new BackupModel()
                    {
                        BackupUserMailAddress = model.AdminUserName,// todo  user.Email,
                        BackupUserOrganization = user.Organization,
                        BackupUserPassword = model.AdminPassword,
                        IsAdminUseExist = true,
                        Index = 0
                    };
                    return View(backupModel);
                }
            }

        }

        [Authorize]
        [HttpPost]
        public ActionResult Index(BackupModel model)
        {
            if (ModelState.IsValid)
            {

                model.Index++;
                if (model.Index == 3)
                    return Run(model);

                if (model.Index == 1)
                {
                    IMailbox mailboxOper = CatalogFactory.Instance.NewMailboxOperatorImpl();
                    EwsServiceArgument argument = new EwsServiceArgument();
                    var password = RSAUtil.AsymmetricDecrypt(model.EncryptPassword);
                    argument.ServiceCredential = new System.Net.NetworkCredential(model.BackupUserMailAddress, password);
                    argument.UseDefaultCredentials = false;
                    argument.SetConnectMailbox(model.BackupUserMailAddress);
                    mailboxOper.ConnectMailbox(argument, model.BackupUserMailAddress);
                    return Json(model);
                }
            }

            return Json(model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult GetAllMailbox(BackupModel model)
        {
            var mailbox = model.BackupUserMailAddress;
            var password = RSAUtil.AsymmetricDecrypt(model.EncryptPassword);
            var organization = model.BackupUserOrganization;
            using (ICatalogService service = CatalogFactory.Instance.NewCatalogService(mailbox, password, null, organization))
            {
                var allMailboxes = service.GetAllUserMailbox();

                List<Item> infos = new List<Item>(allMailboxes.Count);
                IMailboxData loginMailbox = null;
                IMailboxData adminMailbox = null;
                var loginUserName = User.Identity.GetUserName().ToLower();
                var adminMailAddress = mailbox.ToLower();
                foreach (var data in allMailboxes)
                {
                    var temp = data.MailAddress.ToLower();
                    if (temp == loginUserName)
                    {
                        loginMailbox = data;
                    }
                    else if (temp == adminMailAddress)
                    {
                        adminMailbox = data;
                    }
                    else
                    {
                        AddToResult(data, infos);
                    }
                }

                if (adminMailbox != null)
                {
                    AddToResult(adminMailbox, infos, 0);
                }

                if (loginMailbox != null)
                {
                    AddToResult(loginMailbox, infos, 0);
                }

                return Json(new { Details = infos });
            }
        }

        private void AddToResult(IMailboxData data, List<Item> infos, int position = -1)
        {
            if (position == -1)
                infos.Add(new Item()
                {
                    Id = data.MailAddress,
                    DisplayName = data.DisplayName,
                    ChildCount = int.MaxValue,
                    ItemType = ItemTypeStr.Mailbox,
                    CanSelect = 0,
                    OtherInformation = data
                });
            else
            {
                infos.Insert(position, new Item()
                {
                    Id = data.MailAddress,
                    DisplayName = data.DisplayName,
                    ChildCount = int.MaxValue,
                    ItemType = ItemTypeStr.Mailbox,
                    CanSelect = 1,
                    OtherInformation = data
                });
            }
        }

        public ActionResult GetFolderInMailbox(string adminMailbox, string password, string organization, string mailbox, string parentFolderId)
        {
            password = RSAUtil.AsymmetricDecrypt(password);
            using (ICatalogService service = CatalogFactory.Instance.NewCatalogService(adminMailbox, password, null, organization))
            {
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
        }

        [Authorize]
        public JsonResult Run(BackupModel model)
        {
            if (ModelState.IsValid)
            {
                if (!JobProgressManager.Instance.CanStartNewCatalog())
                    return Json(new { HasError = true, Msg = "Can't start Job." , Index = model.Index});

                JavaScriptSerializer js = new JavaScriptSerializer();
                LoadedTreeItem selectedItem = js.Deserialize<LoadedTreeItem>(model.BackupSelectItems);

                //return View(model);
                // todo need use job table to save job status.
                var password = RSAUtil.AsymmetricDecrypt(model.EncryptPassword);
                var service = CatalogFactory.Instance.NewCatalogService(model.BackupUserMailAddress, password, null, model.BackupUserOrganization);

                service.CatalogJobName = model.BackupJobName;
                IFilterItem filterObj = CatalogFactory.Instance.NewFilterItemBySelectTree(selectedItem);
                var serviceId = Guid.NewGuid();
                JobProgressManager.Instance[serviceId] = (IDataProtectProgress)service;
                ThreadPool.QueueUserWorkItem(ThreadBackup, new CatalogArgs() { Service = service, FilterItem = filterObj });
                return Json(new { HasError = false, ServiceId = serviceId.ToString(), Index = model.Index });
            }

            return Json(new {  HasError = true, Msg = "Can't start job"});
        }

        private static void ThreadBackup(object arg)
        {
            var argument = arg as CatalogArgs;
            try
            {
                using (argument.Service)
                    argument.Service.GenerateCatalog(argument.FilterItem);
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.TraceError(e.GetExceptionDetail());
                LogFactory.LogInstance.WriteException(LogInterface.LogLevel.ERR, "Backup job failed.", e, e.Message);
            }
        }

        class CatalogArgs
        {
            public ICatalogService Service;
            public IFilterItem FilterItem;
        }
    }
}