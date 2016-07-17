using Arcserve.Office365.Exchange.DataProtect.Tool.Result;
using Arcserve.Office365.Exchange.Log;
using Arcserve.Office365.Exchange.Manager;
using Arcserve.Office365.Exchange.Tool.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.DataProtect.Tool
{
    class Program
    {
        static int Main(string[] args)
        {
            SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);
            //System.Threading.Thread.Sleep(15000);

            if (args.Length == 1 && args[0] == "Help")
            {
                Console.Out.WriteLine(Resource.ResMessage.Help);
                return 0;
            }

            int exitCode = 0;

            bool isRunAsAdministrator = IsAdministrator();
            LogFactory.LogInstance.WriteLog(LogLevel.DEBUG, isRunAsAdministrator ? "Is Administrator" : "Is Not Administrator");
            //Console.Out.WriteLine(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
            LogFactory.LogInstance.WriteLog(LogLevel.DEBUG, "config file", "file path {0}", AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
            foreach (var arg in args)
            {
                if (arg.ToLower().IndexOf("password") < 0)
                    LogFactory.LogInstance.WriteLog(LogLevel.DEBUG, "args", "arg {0}", arg);
            }

            //return;
            ResultBase result = null;
            try
            {
                var argDic = ArgParser.Parser(args);
                ArcserveCommandFactory factory = new ArcserveCommandFactory(argDic);
                int errorCode = 0;
                result = factory.GetArcserveCommand().Execute(out errorCode);

                exitCode = errorCode;
                Console.Out.WriteLine(ResultBase.Serialize(result));
            }
            catch (Exception e)
            {
                LogFactory.LogInstance.WriteException(LogLevel.ERR, "serialize to xml error", e, e.Message);
                exitCode = -1;
            }

            DisposeManager.DisposeInstance();
            return exitCode;
        }

        public static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
