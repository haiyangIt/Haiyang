using Arcserve.Office365.Exchange.ArcJob;
using Arcserve.Office365.Exchange.Data.Account;
using Arcserve.Office365.Exchange.EwsApi;
using Arcserve.Office365.Exchange.StorageAccess.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.DataProtect.Interface
{
    public interface IServiceContext : IDisposable
    {
        OrganizationAdminInfo AdminInfo { get; }
        string CurrentMailbox { get; set; }
        EwsServiceArgument Argument { get; }
        IDataAccess DataAccessObj { get; }
        TaskType TaskType { get; }
        Exception LastException { get; set; }
        
    }
}
