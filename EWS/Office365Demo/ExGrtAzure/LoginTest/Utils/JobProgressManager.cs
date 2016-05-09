using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections.Concurrent;
using DataProtectInterface;

namespace LoginTest.Utils
{
    public class JobProgressManager : ConcurrentDictionary<Guid, IDataProtectProgress>
    {
        private static JobProgressManager _Instance = null;
        private static object _lock = new object();
        public static JobProgressManager Instance
        {
            get
            {
                if(_Instance == null)
                {
                    lock (_lock)
                    {
                        if(_Instance == null)
                        {
                            _Instance = new JobProgressManager();
                        }
                    }
                }
                return _Instance;
            }
        }
    }
}