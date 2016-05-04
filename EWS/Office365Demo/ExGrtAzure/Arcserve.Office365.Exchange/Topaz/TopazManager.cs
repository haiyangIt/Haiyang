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
        private TopazManager() { }

        private static object _lockObj;
        private static TopazManager _instance;

        public static TopazManager Instance
        {
            get
            {
                if(_instance == null)
                {
                    _lockObj.LockWhile(() =>
                    {
                        if (_instance == null)
                        {
                            _instance = new TopazManager();
                        }
                    });
                }
                return _instance;
            }
        }

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
