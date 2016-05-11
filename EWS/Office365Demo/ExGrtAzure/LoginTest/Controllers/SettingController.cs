using EwsFrame;
using EwsServiceInterface;
using LogInterface;
using LoginTest.Models;
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
    public class SettingController : Controller
    {
        [Authorize]
        // GET: Setting
        public ActionResult Index()
        {
            var currentUserId = User.Identity.GetUserId();
            ApplicationUser user = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(currentUserId);

            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                SettingModel model = context.Settings.Where(s => s.UserMail == user.Email).FirstOrDefault();
                SettingViewModel viewModel;
                if(model == default(SettingModel))
                {
                    viewModel = new SettingViewModel() { UserMail = user.Email, IsExist = false, EwsConnectUrl = "https://outlook.office365.com/EWS/Exchange.asmx" };
                }
                else
                {
                    viewModel = new SettingViewModel()
                    {
                        AdminUserName = model.AdminUserName,
                        AdminPassword = model.AdminPassword,
                        //ConfirmPassword = model.AdminPassword,
                        EwsConnectUrl = model.EwsConnectUrl,
                        UserMail = user.Email,
                        IsExist = true
                    };
                }
                return View(viewModel);
            }
        }

        [Authorize]
        public JsonResult SaveSetting(SettingViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (ApplicationDbContext context = new ApplicationDbContext())
                {
                    EwsServiceArgument argument = new EwsServiceArgument();
                    if (model.IsExist)
                    {
                        SettingModel saveModel = context.Settings.Where(s => s.UserMail == model.UserMail).FirstOrDefault();
                        saveModel.UserMail = model.UserMail;
                        saveModel.AdminPassword = model.EncryptPassword;
                        saveModel.AdminUserName = model.AdminUserName;
                        saveModel.EwsConnectUrl = model.EwsConnectUrl;
                    }
                    else
                    {
                        SettingModel saveModel = new SettingModel()
                        {
                            UserMail = model.UserMail,
                            AdminPassword = model.EncryptPassword,
                            AdminUserName = model.AdminUserName,
                            EwsConnectUrl = model.EwsConnectUrl
                        };
                        context.Settings.Add(saveModel);
                    }
                    
                    context.SaveChanges();
                }
            }
            return Json(model);
        }

        [Authorize]
        public JsonResult TestConnect(SettingViewModel model)
        {
            if (ModelState.IsValid)
            {
                IEwsAdapter mailboxOper = CatalogFactory.Instance.NewEwsAdapter();
                EwsServiceArgument argument = new EwsServiceArgument();
                var password = RSAUtil.AsymmetricDecrypt(model.EncryptPassword);
                argument.ServiceCredential = new System.Net.NetworkCredential(model.AdminUserName, password);
                argument.UseDefaultCredentials = false;
                argument.SetConnectMailbox(model.AdminUserName);
                try {
                    var url = mailboxOper.ConnectMailbox(argument, model.AdminUserName);

                    if (url == model.EwsConnectUrl)
                        return Json(new { Success = true, IsChangeUrl = false });
                    else
                        return Json(new { Success = true, IsChangeUrl = true, Url = url });
                }
                catch(Exception e)
                {
                    System.Diagnostics.Trace.TraceError(e.GetExceptionDetail());
                    return Json(new { Success = false});
                }
            }
            return Json(model);
        }
    }
}