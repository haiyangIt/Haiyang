using EwsServiceInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataProtectInterface
{
    public interface IServiceContext
    {
        OrganizationAdminInfo AdminInfo { get; }
        string CurrentMailbox { get; set; }
        EwsServiceArgument Argument { get; }
        IDataAccess DataAccessObj { get; }
        TaskType TaskType { get; }
        Exception LastException { get; set; }
        string GetOrganizationPrefix();
    }
}
