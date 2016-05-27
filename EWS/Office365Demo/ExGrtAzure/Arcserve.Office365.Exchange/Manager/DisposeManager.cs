using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Arcserve.Office365.Exchange.Manager
{
    public static class DisposeManager
    {
        private static readonly ConcurrentQueue<IDisposable> _Instance;
        static DisposeManager()
        {
            _Instance = new ConcurrentQueue<IDisposable>();
        }

        public static void RegisterInstance(IDisposable obj)
        {
            _Instance.Enqueue(obj);
        }

        public static void DisposeInstance()
        {
            IDisposable obj = null;
            while (_Instance.TryDequeue(out obj))
            {
                if (obj != null)
                {
                    obj.Dispose();
                    obj = null;
                }
            }
        }
    }
}
