using Arcserve.Office365.Exchange.Tool.Framework;
using System;
using System.Threading;

namespace Arcserve.Office365.Exchange.Tool
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 1 && args[0] == "Help")
            {
                Console.Out.WriteLine(Resource1.Help);
                return;
            }

            System.Threading.Thread.Sleep(10000);
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
        }
    }

    public class Mailbox
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string MailAddress { get; set; }
        public string Id { get; set; }

        public Mailbox() { }

        public Mailbox(string displayName, string mailAddress)
        {
            DisplayName = displayName;
            MailAddress = mailAddress;
        }
    }
}
