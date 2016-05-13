using DataProtectInterface;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EwsFrame.Util
{
    public class JobProgressManager : ConcurrentDictionary<Guid, IDataProtectProgress>
    {
        private static JobProgressManager _Instance = null;
        private static object _lock = new object();
        public static JobProgressManager Instance
        {
            get
            {
                if (_Instance == null)
                {
                    using (_lock.LockWhile(() =>
                    {
                        if (_Instance == null)
                        {
                            _Instance = new JobProgressManager();
                        }
                    }))
                    { }
                }
                return _Instance;
            }
        }

        public bool CanStartNewCatalog()
        {
            foreach (var value in this.Values)
            {
                if (value.EndTime != DateTime.MinValue)
                    return false;
            }
            return true;
        }
    }
}
