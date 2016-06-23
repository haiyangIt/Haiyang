using Arcserve.Office365.Exchange.Tool.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Arcserve.Office365.Exchange.DataProtect.Tool.Result;
using Arcserve.Office365.Exchange.DataProtect.Tool.Resource;
using System.Management.Automation.Remoting;
using Arcserve.Office365.Exchange.DataProtect.Tool.Data;
using Arcserve.Office365.Exchange.EwsApi.Interface;

namespace Arcserve.Office365.Exchange.Tool
{
    public class GetAllMailboxCommand : ArcServeCommand
    {
        public const string CommandName = "GetAllMailbox";
        [CommandArgument("AdminUserName", "Please specify the administartor. like: -AdminUserName:user.", true)]
        public ArgInfo AdminUserName { get; set; }
        [CommandArgument("AdminPassword", "Please specify the password. like: -AdminPassword:userpassword.", true)]
        public ArgInfo AdminPassword { get; set; }

        public GetAllMailboxCommand(CommandArgs args) : base(args)
        {

        }

        protected override ResultBase DoExcute()
        {
            var result = GetAllMailbox(AdminUserName.Value, AdminPassword.Value);

            return new GetAllMailboxResult(result);
        }

        private static MailboxList GetAllMailbox(string adminName, string adminPassword)
        {
            var result = new MailboxList();
            result.AddRange(EwsServiceExtension.GetAllMailbox(adminName, adminPassword, null));
            return result;
        }

        protected override ResultBase GetErrorResultBase(Exception e)
        {
            if (e is PSRemotingTransportException)
            {
                if (e.HResult == -2146233087)
                {
                    if (((PSRemotingTransportException)e).ErrorCode == -2144108103)
                    {
                        return new GetAllMailboxResult(ResMessage.NetworkInvalid);
                    }
                    else if (((PSRemotingTransportException)e).ErrorCode == -2144108102)
                    {
                        return new GetAllMailboxResult(ResMessage.NetworkUnStable);
                    }
                }
            }

            return new GetAllMailboxResult(e.Message);
        }

        protected override ResultBase GetInvalidUserPsw()
        {
            return new GetAllMailboxResult(ResMessage.UserNamePswInvalid);
        }
    }
}
