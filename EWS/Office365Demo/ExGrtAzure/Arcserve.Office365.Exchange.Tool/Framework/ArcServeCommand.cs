using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation.Remoting;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Tool.Framework
{
    public abstract class ArcServeCommand
    {
        protected CommandArgs _Args;
        protected ArcServeCommand(CommandArgs args)
        {
            _Args = args;
        }

        [CommandArgument("JobType", "Please specify the job type. like: -JobType:GetAllMailbox.", true)]
        public ArgInfo JobType { get; set; }

        public const string CommandType = "JobType";



        //public static ArcServeCommand CheckArgument(string[] args)
        //{
        //    var argDic = ArgParser.Parser(args);
        //    return CheckArgument(argDic);
        //}

        //public static ArcServeCommand CheckArgument(CommandArgs args)
        //{
        //    ArgInfo outV;
        //    if (!args.TryGetValue(CommandType, out outV))
        //        throw new ArgumentInToolException("Please specify the job type. like: -JobType:GetAllMailbox.");
        //    var type = outV.Value;
        //    return AllCommand[type].Invoke(args);
        //}

        protected virtual void CheckArgument()
        {
            var properties = this.GetType().GetProperties();
            foreach (var property in properties)
            {
                var attribute = property.GetCustomAttribute(typeof(CommandArgumentAttribute), true) as CommandArgumentAttribute;
                if (attribute != null)
                {
                    var commandType = attribute.ArgumentName;
                    ArgInfo value;
                    if (!_Args.TryGetValue(commandType, out value))
                    {
                        if (attribute.Required)
                            throw new ArgumentInToolException(attribute.ErrorMsg);
                    }
                    else
                    {
                        property.SetValue(this, value);
                    }
                }
            }

        }

        protected abstract string DoExcute();
        public void Execute(TextWriter output)
        {
            try
            {
                CheckArgument();
                var result = DoExcute();
                output.WriteLine("Success");
                output.WriteLine(result);
            }
            catch (ArgumentInToolException e)
            {
                output.WriteLine("Error");
                output.WriteLine(e.Message);
            }
            catch (PSRemotingTransportException e)
            {
                if (e.HResult == -2146233087 && e.ErrorCode == -2144108477)
                {
                    output.WriteLine("Error");
                    output.WriteLine("The admin user name or password is not valid.");
                    output.WriteLine(e.Message);
                    output.WriteLine(e.GetType().FullName);
                    output.WriteLine(e.StackTrace);
                }
                else
                {
                    output.WriteLine("Error");
                    output.WriteLine(e.Message);
                    output.WriteLine(e.GetType().FullName);
                    output.WriteLine(e.StackTrace);
                }
            }
            catch (Exception e)
            {
                output.WriteLine("Error");
                output.WriteLine(e.Message);
                output.WriteLine(e.StackTrace);
            }
        }
    }

    public class CommandArgs : Dictionary<string, ArgInfo>
    {
        public CommandArgs(int capacity) : base(capacity) { }
    }

    public class ArcserveCommandFactory : ArcServeCommand
    {
        public ArcserveCommandFactory(CommandArgs args) : base(args) { }

        private static Dictionary<string, Func<CommandArgs, ArcServeCommand>> AllCommand = new Dictionary<string, Func<CommandArgs, ArcServeCommand>>
        {
            {GetAllMailboxCommand.CommandName, (args)=> {return new GetAllMailboxCommand(args); } }
        };

        public ArcServeCommand GetArcserveCommand()
        {
            CheckArgument();
            var type = JobType.Value;
            return AllCommand[type].Invoke(_Args);
        }


        protected override string DoExcute()
        {
            throw new NotImplementedException();
        }
    }
}
