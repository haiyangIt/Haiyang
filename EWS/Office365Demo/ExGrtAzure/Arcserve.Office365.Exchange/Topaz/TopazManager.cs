using Arcserve.Office365.Exchange.Util;
using Microsoft.Practices.TransientFaultHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Topaz
{
    public class TopazManager
    {
        static TopazManager()
        {
            Instance = new TopazManager();
        }
        private TopazManager() { }


        public static readonly TopazManager Instance;

        public ITransientErrorDetectionStrategy GetDetectionStrategy(RetryContext context)
        {
            throw new NotImplementedException();
        }

        public RetryStrategy GetRetryStrategy(RetryContext context)
        {
            throw new NotImplementedException();
        }
    }
}
