using DataProtectInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace EwsFrame.Context
{
    public class ServiceContextManager : Dictionary<Guid, IServiceContext>
    {
    }
}
