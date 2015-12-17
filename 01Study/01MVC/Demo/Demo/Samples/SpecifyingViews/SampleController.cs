using Demo.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Demo.Controllers
{
    public class SampleController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Welcome to ASP.NET MVC!";
            return View();
        }

        public ActionResult Index2()
        {
            ViewBag.Message = "Welcome to ASP.NET MVC!";
            return View("NotIndex");
        }

        public ActionResult Index3()
        {
            ViewBag.Message = "Welcome to ASP.NET MVC!";
            return View("~/Views/Example/Index.cshtml");
        }

        public ActionResult Message()
        {
            ViewBag.Message = "This is a partial view.";
            return PartialView();
        }

        public ActionResult PartialViewDemo()
        {
            return View();
        }

        private static int id = 0;
        public ActionResult LoadMore()
        {
            var result = new List<AddMoreModel>();
            
            for (int i = 0; i < 5; i++)
            {
                id++;
                result.Add(new AddMoreModel() { Name = string.Format("Name-{0}", id), Id = (id * 2).ToString(), Type = string.Format("type{0}",i.ToString()) });
            }
            return PartialView("LoadMore", result);
        }

        [HttpPost]
        public JsonResult UpdateMailboxes(DateTime catalogDateTime)
        {
            UpdateMailboxesModel result = new UpdateMailboxesModel();
            result.CatalogTime = catalogDateTime;
            List<MailboxInfo> infos = new List<MailboxInfo>(24);
            for(int i = 0; i < 24; i++)
            {
                infos.Add(new MailboxInfo() { DisplayName = string.Format("DisplayName-{0}", i), MailAddress = string.Format("Address{0}@arcserve.com", i), RootFolderId = i.ToString() });
            }
            return Json(result);
        }
    }

    
}
