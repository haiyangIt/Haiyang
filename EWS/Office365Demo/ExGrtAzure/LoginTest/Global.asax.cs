using EwsFrame;
using EwsFrame.EF;
using EwsFrame.Util;
using FTStreamUtil;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
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
            if (!FactoryBase.IsRunningOnAzure())
            {
                string logFolder = ConfigurationManager.AppSettings["LogPath"];
                var logPath = Path.Combine(logFolder, string.Format("{0}Trace.txt", DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss")));
                Trace.Listeners.Add(new TextWriterTraceListener(logPath));
            }
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
            try {
                Exception exception = Server.GetLastError();

                Trace.Fail("Application Error", exception.GetExceptionDetail());
                LogFactory.LogInstance.WriteException(LogInterface.LogLevel.ERR, "Application_Error", exception, exception.Message);
            }
            catch(Exception ex)
            {
                try
                {
                    Trace.Fail("Application Error Error", ex.GetExceptionDetail());
                }
                catch(Exception ex1)
                {

                }
            }
        }

        protected void Application_End()
        {
            try {
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
                EwsRequestGate.Instance.Dispose();
            }
            catch(Exception e)
            {
                try
                {
                    Trace.TraceError(e.GetExceptionDetail());
                }
                catch
                {

                }
            }

        }
    }
}
