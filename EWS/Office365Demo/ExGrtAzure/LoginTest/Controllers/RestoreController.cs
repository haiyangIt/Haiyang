using DataProtectImpl;
using DataProtectInterface;
using Demo.Models.Restore;
using EwsDataInterface;
using EwsFrame;
using EwsFrame.Util;
using LoginTest.Models;
using LoginTest.Models.Restore;
using LoginTest.Models.Setting;
using LoginTest.Utils;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LoginTest.Controllers
{
    [CustomErrorHandler]
    public class RestoreController : Controller
    {
        private static IRestoreService GetRestoreService(RestoreUserInfo restoreUser)
        {
            var service = RestoreFactory.Instance.NewRestoreService(restoreUser.Name, restoreUser.Organization);
            return service;
        }

        [Authorize]
        // GET: Restore
        public ActionResult Index()
        {
            var currentUserId = User.Identity.GetUserId();
            ApplicationUser user = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(currentUserId);
            RestoreUserInfo restoreUser = null;
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                SettingModel model = context.Settings.Where(s => s.UserMail == user.Email).FirstOrDefault();
                if(model == null)
                {
                    restoreUser = new RestoreUserInfo() { Name = user.UserName, Organization = user.Organization, Id = user.Id, IsExistSetting = false };
                }
                else
                {
                    restoreUser = new RestoreUserInfo() { Name = user.UserName, Organization = user.Organization, Id = user.Id ,IsExistSetting = true, AdminPassword = model.AdminPassword, AdminUserName = model.AdminUserName};
                }
            }

            return View(restoreUser);
        }

        [Authorize]
        public JsonResult GetLatestMonthCatalogs(string organization)
        {
            var dataAccess = RestoreFactory.Instance.NewDataAccessForQuery();
            dataAccess.Organization = organization;
            ICatalogJob job = dataAccess.GetLatestCatalogJob();
            if (job != null)
            {
                List<DateTime> daysHasCatalogs = dataAccess.GetCatalogDaysInMonth(job.StartTime);
                List<ICatalogJob> catalogJobs = dataAccess.GetCatalogsInOneDay(job.StartTime);
                return Json(new { LatestCatalogJob = job, CatalogJobsInLatestDay = catalogJobs, Days = daysHasCatalogs });
            }
            else
            {
                return Json(new { Days = new List<DateTime>(0) });
            }
        }

        [Authorize]
        public JsonResult GetCatalogDaysInMonth(DateTime date, string organization)
        {
            var dataAccess = RestoreFactory.Instance.NewDataAccessForQuery();
            dataAccess.Organization = organization;
            List<DateTime> daysHasCatalogs = dataAccess.GetCatalogDaysInMonth(date);
            return Json(new { Days = daysHasCatalogs });
        }

        [Authorize]
        [HttpPost]
        public JsonResult UpdateMailboxes(CatalogJobParameter catalogJob)
        {
            var dataAccess = RestoreFactory.Instance.NewDataAccessForQuery();
            dataAccess.CatalogJob = catalogJob;
            var allMailboxes = dataAccess.GetAllMailbox();

            List<Item> infos = new List<Item>(allMailboxes.Count);
            foreach (var mailbox in allMailboxes)
            {
                infos.Add(new Item()
                {
                    Id = mailbox.RootFolderId,
                    DisplayName = mailbox.DisplayName,
                    ChildCount = mailbox.ChildFolderCount,
                    ItemType = ItemTypeStr.Mailbox,
                    OtherInformation = mailbox
                });
            }

            return Json(new { Details = infos, CatalogTime = catalogJob.StartTime });
        }

        [Authorize]
        public JsonResult UpdateFolders(CatalogJobParameter catalogJob, string mailAddress)
        {
            var dataAccess = RestoreFactory.Instance.NewDataAccessForQuery();
            dataAccess.CatalogJob = catalogJob;
            var allFolder = dataAccess.GetAllFoldersInMailboxes(mailAddress);

            var tree = TreeNode.CreateTree(allFolder);

            List<Item> result = new List<Item>();
            Stack<Item> stacks = new Stack<Item>();
            int oldDepth = -1;
            TreeNode.DepthFirstTraverseTree(tree, (currentNode, depth, inChildrenIndex) =>
            {
                Item item = null;
                if (currentNode.Folder == null)
                {
                    item = new Item();
                }
                else
                {
                    item = new Item()
                    {
                        Id = currentNode.Folder.FolderId,
                        DisplayName = ((IItemBase)currentNode.Folder).DisplayName,
                        ChildCount = currentNode.Folder.ChildFolderCount + currentNode.Folder.ChildItemCount,
                        ItemType = ItemTypeStr.Folder,
                        OtherInformation = currentNode.Folder
                    };
                }

                if (oldDepth <= depth)
                {
                    stacks.Push(item);
                }
                else
                {
                    LinkedList<Item> childItems = new LinkedList<Item>();
                    var stackCount = stacks.Count;
                    for (int i = 0; i < inChildrenIndex; i++)
                    {
                        childItems.AddFirst(stacks.Pop());
                    }
                    item.Container = childItems.ToList();
                    stacks.Push(item);
                }

                oldDepth = depth;
            });


            var rootItem = stacks.Pop();

            return Json(new { CatalogTime = catalogJob.StartTime, Details = rootItem.Container });
        }

        [Authorize]
        public JsonResult UpdateMails(CatalogJobParameter catalogJob, string folderId, int pageIndex, int pageCount)
        {
            var dataAccess = RestoreFactory.Instance.NewDataAccessForQuery();
            dataAccess.CatalogJob = catalogJob;

            List<IItemData> items = dataAccess.GetChildItems(folderId, pageIndex, pageCount);


            int startIndex = pageIndex * pageCount;
            int endIndex = startIndex + pageCount;
            List<Item> result = new List<Item>(pageCount);
            int index = 0;
            while (startIndex < endIndex && index < items.Count)
            {
                var mailItem = items[index++];
                var item = new Item() { Id = mailItem.ItemId, ChildCount = 0, ItemType = ItemTypeStr.Item, DisplayName = mailItem.DisplayName, OtherInformation = mailItem };

                result.Add(item);

                startIndex++;
            }

            int totalCount = -1;
            if (pageIndex == 0)
            {
                totalCount = dataAccess.GetChildItemsCount(folderId);
            }

            return Json(new { Mails = result, TotalCount = totalCount });
        }

        [Authorize]
        public JsonResult GetCatalogs(DateTime day, string organization)
        {
            var dataAccess = RestoreFactory.Instance.NewDataAccessForQuery();
            dataAccess.Organization = organization;
            List<ICatalogJob> catalogJobs = dataAccess.GetCatalogsInOneDay(day);
            return Json(new { CatalogInfos = catalogJobs });
        }

        [Authorize]
        public JsonResult RestoreItemsToAlter(CatalogJobParameter catalog,
            RestoreAdminUserInfo restoreAdminUserInfo,
            RestoreDestination destination,
            LoadedTreeItem selectedItem)
        {
            var password = RSAUtil.AsymmetricDecrypt(restoreAdminUserInfo.Password);
            IRestoreServiceEx restore = RestoreFactory.Instance.NewRestoreServiceEx(restoreAdminUserInfo.UserAddress, password, string.Empty, restoreAdminUserInfo.Organization);
            restore.CurrentRestoreCatalogJob = catalog;
            var context = ((RestoreServiceEx)restore).ServiceContext;
            var dataAccess = CatalogFactory.Instance.NewCatalogDataAccessInternal(context.Argument, context.AdminInfo.OrganizationName);
            restore.Destination = RestoreFactory.Instance.NewRestoreDestinationEx(restore.ServiceContext.Argument, dataAccess);
            restore.Destination.SetOtherInformation(destination.MailboxAddress, destination.FolderPath);
            restore.Restore(selectedItem);
            return Json(new { IsSuccess = true });
        }

        [Authorize]
        public JsonResult RestoreItemsToOrg(CatalogJobParameter catalog,
            RestoreAdminUserInfo restoreAdminUserInfo,
            RestoreDestination destination,
            LoadedTreeItem selectedItem)
        {
            var password = RSAUtil.AsymmetricDecrypt(restoreAdminUserInfo.Password);
            IRestoreServiceEx restore = RestoreFactory.Instance.NewRestoreServiceEx(restoreAdminUserInfo.UserAddress, password, string.Empty, restoreAdminUserInfo.Organization);
            restore.CurrentRestoreCatalogJob = catalog;
            var context = ((RestoreServiceEx)restore).ServiceContext;
            var dataAccess = RestoreFactory.Instance.NewCatalogDataAccessInternal();
            restore.Destination = RestoreFactory.Instance.NewRestoreDestinationOrgEx(restore.ServiceContext.Argument, dataAccess);
            restore.Destination.SetOtherInformation(destination.FolderPath);
            restore.Restore(selectedItem);
            return Json(new { IsSuccess = true });
        }

        [Authorize]
        public JsonResult DumpItems(CatalogJobParameter catalog,
            RestoreAdminUserInfo restoreAdminUserInfo,
            LoadedTreeItem selectedItem,
            string notificationAddress,
            ExportType exportType)
        {
            var password = RSAUtil.AsymmetricDecrypt(restoreAdminUserInfo.Password);
            IRestoreServiceEx restore = RestoreFactory.Instance.NewRestoreServiceEx(restoreAdminUserInfo.UserAddress, password, string.Empty, restoreAdminUserInfo.Organization);
            
            restore.CurrentRestoreCatalogJob = catalog;
            restore.Destination = RestoreFactory.Instance.NewDumpDestination();
            restore.Destination.ExportType = exportType;
            restore.Destination.SetOtherInformation(notificationAddress);
            restore.Restore(selectedItem);
            return Json(new { IsSuccess = true });
        }
    }
}