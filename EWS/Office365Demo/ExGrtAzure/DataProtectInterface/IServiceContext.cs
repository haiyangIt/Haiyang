using EwsServiceInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataProtectInterface
{
    public interface IServiceContext : IDisposable
    {
        OrganizationAdminInfo AdminInfo { get; }
        string CurrentMailbox { get; set; }
        
        EwsServiceArgument Argument { get; }
        

        TaskType TaskType { get; }

        Exception LastException { get; set; }
        
    }
}
