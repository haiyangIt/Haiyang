using Arcserve.Office365.Exchange.Log;
using Arcserve.Office365.Exchange.Manager;
using Arcserve.Office365.Exchange.Tool.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.DataProtect.Tool
{
    class Program
    {
        static void Main(string[] args)
        {
            SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);

            if (args.Length == 1 && args[0] == "Help")
            {
                Console.Out.WriteLine(Resource1.Help);
                return;
            }

            //Console.Out.WriteLine(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

            LogFactory.LogInstance.WriteLog(LogLevel.DEBUG, "config file", "file path {0}", AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
            foreach(var arg in args)
            {
                if (arg.ToLower().IndexOf("password") < 0)
                    LogFactory.LogInstance.WriteLog(LogLevel.DEBUG, "args", "arg {0}", arg);
            }

            //System.Threading.Thread.Sleep(20000);
            //return;
            try
            {
                var argDic = ArgParser.Parser(args);
                ArcserveCommandFactory factory = new ArcserveCommandFactory(argDic);
                factory.GetArcserveCommand().Execute(Console.Out);
            }
            catch (ArgumentInToolException e)
            {
                Console.Out.WriteLine("Error");
                Console.Out.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                Console.Out.WriteLine("Error");
                Console.Out.WriteLine(e.Message);
                Console.Out.WriteLine(e.StackTrace);
            }
            finally
            {
                DisposeManager.DisposeInstance();
            }
        }
    }
}
