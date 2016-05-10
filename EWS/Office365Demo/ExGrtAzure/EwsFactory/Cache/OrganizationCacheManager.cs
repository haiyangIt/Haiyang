using EwsFrame.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EwsFrame.Cache
{
    public class OrganizationCacheManager : ICacheManager
    {
        public static OrganizationCacheManager CacheManager = new OrganizationCacheManager();

        private static string GetName(string organization, string cacheName)
        {
            return string.Format("{0}_{1}", organization, cacheName);
        }

        public ICache NewCache(string organization, string cacheName, Type cacheType)
        {
            ICache result = null;
            using (CacheDic.LockWhile(() =>
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
                 result = outObj;
             }))
            { }
            return result;
        }

        public ICache GetCache(string organization, string cacheName)
        {
            ICache result = null;
            using (CacheDic.LockWhile(() =>
            {
                string name = GetName(organization, cacheName);


                ICache outObj;
                if (!CacheDic.TryGetValue(name, out outObj))
                {
                    result = null;
                    return;
                }
                result = outObj;
            }))
            { }
            return result;
        }

        public void ReleaseCache(bool isSerialize = false)
        {
            using (CacheDic.LockWhile(() =>
            {
                foreach (var keyValue in CacheDic)
                {
                    keyValue.Value.Serialize();
                }
                CacheDic.Clear();
            }))
            { }
        }


        private Dictionary<string, ICache> CacheDic = new Dictionary<string, ICache>();
    }
}
