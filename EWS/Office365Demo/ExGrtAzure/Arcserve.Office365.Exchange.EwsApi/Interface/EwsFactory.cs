using Arcserve.Office365.Exchange.EwsApi.Impl.Increment;
using Arcserve.Office365.Exchange.Thread;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.EwsApi.Interface
{
    public class EwsFactory
    {
        static EwsFactory()
        {
            Instance = new EwsFactory();
        }
        public static EwsFactory Instance;

        public IEwsServiceAdapter<IJobProgress> NewEwsAdapter()
        {
            return new EwsServiceAdapter();
        }
    }
}
