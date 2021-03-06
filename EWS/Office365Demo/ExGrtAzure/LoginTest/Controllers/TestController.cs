﻿using EwsFrame;
using LoginTest.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LoginTest.Controllers
{
    [CustomErrorHandler]
    public class TestController : Controller
    {
        // GET: Test
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult TestIsInAzure()
        {
            if (FactoryBase.IsRunningOnAzureOrStorageInAzure())
                return Json("");
            else
                throw new InvalidOperationException("the method FactoryBase.IsRunningOnAzure may be wrong.");
        }

        private static bool IsDllExist(string directory)
        {
            var result = Directory.EnumerateFiles(directory, "*EwsDataInterface.dll*");
            return result.Count() > 0;
        }

        public JsonResult TestLibPath()
        {
            var directory = AppDomain.CurrentDomain.BaseDirectory;
            var path = "";
            var temp = Path.Combine(directory, "bin");
            if (IsDllExist(temp))
            {
                path = temp;
            }
            else
            {
                temp = Path.Combine(directory, "..\\lib");

                if (IsDllExist(temp))
                {
                    path = directory;
                }
                else
                {
                    throw new NotSupportedException();
                }
            }

            return Json(new { Result = path });
        }

        public JsonResult TestLoadAssembly()
        {
            var log = LogFactory.LogInstance;
            var dataConvert = CatalogFactory.Instance.NewDataConvert();
            var dataAccess = RestoreFactory.Instance.NewDataAccessForQuery();
            var restoreDataConvert = RestoreFactory.Instance.NewDataConvert();
            return Json("");
        }

        public JsonResult TestAppendLog()
        {
            var log = LogFactory.LogInstance;
            log.WriteLog(LogInterface.LogLevel.INFO, "Test");
            log.WriteLog(LogInterface.LogLevel.ERR, "Test Error");
            return Json("");
        }

        public JsonResult TestDownloadLog()
        {
            var log = LogFactory.LogInstance;
            var result = log.GetTotalLog(DateTime.Now);
            return Json(new { Log = result });
        }
    }
}