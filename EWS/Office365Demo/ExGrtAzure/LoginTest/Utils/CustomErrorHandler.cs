using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LoginTest.Utils
{
    public class CustomErrorHandler : HandleErrorAttribute
    {
        private bool IsAjax(ExceptionContext filterContext)
        {
            return filterContext.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }

        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled || !filterContext.HttpContext.IsCustomErrorEnabled)
            {
                return;
            }

            // if the request is AJAX return JSON else view.
            if (IsAjax(filterContext))
            {
                filterContext.ExceptionHandled = true;
                filterContext.HttpContext.Response.Clear();
                filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
                filterContext.HttpContext.Response.StatusCode = 500;

                //Because its a exception raised after ajax invocation
                //Lets return Json
                filterContext.Result = new JsonResult()
                {
                    Data = new { Exception = filterContext.Exception.Message, StackTrace = filterContext.Exception.StackTrace },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };

               
            }
            else
            {
                //Normal Exception
                //So let it handle by its default ways.
                base.OnException(filterContext);
            }

            // Write error logging code here if you wish.

            //if want to get different of the request
            //var currentController = (string)filterContext.RouteData.Values["controller"];
            //var currentActionName = (string)filterContext.RouteData.Values["action"];
        }
    }
}