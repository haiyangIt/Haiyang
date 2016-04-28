using Arcserve.Office365.Exchange.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Cache
{
    public class OrganizationCacheManager : ICacheManager, IDisposable
    {
        public static OrganizationCacheManager CacheManager = new OrganizationCacheManager();

        private static string GetName(string organization, string cacheName)
        {
            return string.Format("{0}_{1}", organization, cacheName);
        }

        public ICache NewCache(string organization, string cacheName, Type cacheType)
        {
            using (_CacheDicLock.Write())
            {
                ICache outObj;
                string name = GetName(organization, cacheName);
                if (!CacheDic.TryGetValue(name, out outObj))
                {
                    outObj = cacheType.GetConstructor(new Type[0]).Invoke(new object[0]) as ICache;
                    if (outObj == null)
                        throw new ArgumentException(string.Format("type {0} is not implement class of ICache", cacheType.FullName), "cacheType");
                    CacheDic.Add(name, outObj);
                    outObj.DeSerialize();
                }
                return outObj;
            }
        }

        public ICache GetCache(string organization, string cacheName)
        {
            using (_CacheDicLock.Read())
            {
                string name = GetName(organization, cacheName);


                ICache outObj;
                if (!CacheDic.TryGetValue(name, out outObj))
                {
                    return null;
                }
                return outObj;
            }
        }

        public void ReleaseCache(bool isSerialize = false)
        {
            using (_CacheDicLock.Read())
            {
                foreach (var keyValue in CacheDic)
                {
                    keyValue.Value.Serialize();
                }
            }
            using (_CacheDicLock.Write())
            {
                CacheDic.Clear();
            }
        }

        public void Dispose()
        {
            _CacheDicLock.Dispose();
        }

        private Dictionary<string, ICache> CacheDic = new Dictionary<string, ICache>();
        private ReaderWriterLockSlim _CacheDicLock = new ReaderWriterLockSlim();
    }
}
