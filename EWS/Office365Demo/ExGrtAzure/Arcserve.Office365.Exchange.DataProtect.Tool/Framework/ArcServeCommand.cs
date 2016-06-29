using Arcserve.Office365.Exchange.DataProtect.Tool.Command;
using Arcserve.Office365.Exchange.DataProtect.Tool.Resource;
using Arcserve.Office365.Exchange.DataProtect.Tool.Result;
using Arcserve.Office365.Exchange.Log;
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

        protected abstract ResultBase DoExcute();
        protected abstract ResultBase GetErrorResultBase(Exception e);
        protected abstract ResultBase GetInvalidUserPsw();
        public ResultBase Execute()
        {
            try
            {
                CheckArgument();
                return DoExcute();
            }
            catch (ArgumentInToolException e)
            {
                LogFactory.LogInstance.WriteException(LogLevel.ERR, string.Format("execute command {0} error", JobType), e, e.Message);
                return GetErrorResultBase(e);
            }
            catch (PSRemotingTransportException e)
            {
                if (e.HResult == -2146233087 && e.ErrorCode == -2144108477)
                {
                    LogFactory.LogInstance.WriteException(LogLevel.ERR, string.Format("execute command {0} error", JobType), e, e.Message);
                    return GetInvalidUserPsw();
                }
                else
                {
                    LogFactory.LogInstance.WriteException(LogLevel.ERR, string.Format("execute command {0} error", JobType), e, e.Message);
                    return GetErrorResultBase(e);
                }
            }
            catch (Exception e)
            {
                LogFactory.LogInstance.WriteException(LogLevel.ERR, string.Format("execute command {0} error", JobType), e, e.Message);
                return GetErrorResultBase(e);
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
            {ExchangeBackupCommand.CommandName, (args) => {return new ExchangeBackupCommand(args); } },
             {GetAllMailboxCommand.CommandName, (args) => {return new GetAllMailboxCommand(args); } },
             {TestCommand.CommandName, (args) => {return new TestCommand(args); } }
        };

        public ArcServeCommand GetArcserveCommand()
        {
            CheckArgument();
            var type = JobType.Value;

            var result = AllCommand[type].Invoke(_Args);
            return result;
        }

        protected override ResultBase DoExcute()
        {
            throw new NotImplementedException();
        }

        protected override ResultBase GetErrorResultBase(Exception e)
        {
            throw new NotImplementedException();
        }

        protected override ResultBase GetInvalidUserPsw()
        {
            throw new NotImplementedException();
        }
    }
}
