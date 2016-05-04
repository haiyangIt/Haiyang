using EwsFrame;
using EwsFrame.EF;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace LoginTest
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            LogFactory.LogInstance.WriteLog(LogInterface.LogLevel.INFO, "Application_Start");
            SqlServerTypes.Utilities.LoadNativeAssemblies(Server.MapPath("~/bin"));

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            DbConfiguration.SetConfiguration(new CustomApplicationDbConfiguration());
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception exception = Server.GetLastError();
            LogFactory.LogInstance.WriteException(LogInterface.LogLevel.ERR, "Application_Error", exception, exception.Message);
        }

        protected void Application_End()
        {
            LogFactory.LogInstance.WriteLog(LogInterface.LogLevel.INFO, "Application_End");
            LogFactory.LogInstance.Dispose();
            LogFactory.EwsTraceLogInstance.Dispose();
            
        }
    }
}
