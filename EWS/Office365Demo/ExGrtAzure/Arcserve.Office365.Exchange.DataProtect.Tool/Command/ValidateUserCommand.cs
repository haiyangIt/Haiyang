using Arcserve.Office365.Exchange.Data.Account;
using Arcserve.Office365.Exchange.DataProtect.Impl;
using Arcserve.Office365.Exchange.DataProtect.Tool.Resource;
using Arcserve.Office365.Exchange.DataProtect.Tool.Result;
using Arcserve.Office365.Exchange.EwsApi.Interface;
using Arcserve.Office365.Exchange.Tool.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.DataProtect.Tool.Command
{
    [ArcserveCommand("ValidateUser")]
    public class ValidateUserCommand : ArcServeCommand
    {
        public ValidateUserCommand(CommandArgs args) : base(args)
        {

        }

        [CommandArgument("AdminUserName", "Please specify the job type. like: -AdminUserName:user.", true)]
        public ArgInfo AdminUserName { get; set; }

        [CommandArgument("AdminPassword", "Please specify the job type. like: -AdminPassword:user.", true)]
        public ArgInfo AdminPassword { get; set; }

        [CommandArgument("ConnectMailbox", "Please specify the job type. like: -AdminPassword:user.", false)]
        public ArgInfo ConnectMailbox { get; set; }

        public static string CommandName
        {
            get
            {
                return "ValidateUser";
            }
        }

        protected override ResultBase DoExcute()
        {
            var taskSyncContextBase = DataProtectFactory.Instance.NewDefaultTaskSyncContext();
            var ewsServiceAdapter = EwsFactory.Instance.NewEwsAdapter();
            ewsServiceAdapter.InitTaskSyncContext(taskSyncContextBase);
            var adminInfo = new Arcserve.Office365.Exchange.Data.Account.OrganizationAdminInfo()
            {
                OrganizationName = AdminUserName.Value.GetOrganization(),
                UserName = AdminUserName.Value,
                UserPassword = AdminPassword.Value
            };
            var con = AdminUserName.Value;
            if(ConnectMailbox != null && !string.IsNullOrEmpty(ConnectMailbox.Value))
            {
                con = ConnectMailbox.Value;
            }
            ewsServiceAdapter.GetExchangeService(con, adminInfo);
            return new ExchangeBackupResult();
        }

        protected override ResultBase GetErrorResultBase(Exception e)
        {
            return new ExchangeBackupResult(e.Message);
        }

        protected override ResultBase GetInvalidUserPsw()
        {
            return new ExchangeBackupResult(ResMessage.UserNamePswInvalid);
        }
    }
}
