using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using Arcserve.Office365.Exchange.DataProtect.Interface;

namespace EwsFrame.Context
{
    public class ServiceContextManager : Dictionary<Guid, IServiceContext>
    {
    }
}
