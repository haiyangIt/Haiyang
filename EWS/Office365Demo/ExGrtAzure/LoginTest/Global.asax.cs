using EwsFrame;
using EwsFrame.EF;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
            Trace.TraceInformation("Application Start.");
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

            Trace.TraceError("Application Error. message:{0}, stackTrace", exception.Message, exception.StackTrace);
            LogFactory.LogInstance.WriteException(LogInterface.LogLevel.ERR, "Application_Error", exception, exception.Message);
        }

        protected void Application_End()
        {
            HttpRuntime runtime = (HttpRuntime)typeof(System.Web.HttpRuntime).InvokeMember("_theRuntime",
                                                                                    BindingFlags.NonPublic
                                                                                    | BindingFlags.Static
                                                                                    | BindingFlags.GetField,
                                                                                    null,
                                                                                    null,
                                                                                    null);

            if (runtime != null)
            {

                string shutDownMessage = (string)runtime.GetType().InvokeMember("_shutDownMessage",
                                                                                 BindingFlags.NonPublic
                                                                                 | BindingFlags.Instance
                                                                                 | BindingFlags.GetField,
                                                                                 null,
                                                                                 runtime,
                                                                                 null);

                string shutDownStack = (string)runtime.GetType().InvokeMember("_shutDownStack",
                                                                               BindingFlags.NonPublic
                                                                               | BindingFlags.Instance
                                                                               | BindingFlags.GetField,
                                                                               null,
                                                                               runtime,
                                                                               null);
                
                Trace.TraceError("Application End. _shutDownMessage={0} _shutDownStack={1}", shutDownMessage, shutDownStack);
                LogFactory.LogInstance.WriteLog(LogInterface.LogLevel.ERR, "Application_End", "_shutDownMessage={0} _shutDownStack={1}", shutDownMessage, shutDownStack);
            }
            LogFactory.LogInstance.WriteLog(LogInterface.LogLevel.INFO, "Application_End");
            LogFactory.LogInstance.Dispose();
            LogFactory.EwsTraceLogInstance.Dispose();

        }
    }
}
