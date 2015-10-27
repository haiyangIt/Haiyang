using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EwsFrame.Cache
{
    public class MailboxCacheManager
    {
        [ThreadStatic]
        public static MailboxCacheManager CacheManager = new MailboxCacheManager();

        public ICache NewCache(string mailboxAddress, string cacheName, Type cacheType)
        {
            lock(CacheDic)
            {
                if (string.IsNullOrEmpty(_mailboxAddress))
                    _mailboxAddress = mailboxAddress;

                if(!mailboxAddress.Equals(_mailboxAddress))
                {
                    CacheDic.Clear();
                }

                ICache outObj;
                if(!CacheDic.TryGetValue(cacheName,out outObj))
                {
                    outObj = cacheType.GetConstructor(new Type[0]).Invoke(new object[0]) as ICache;
                    if (outObj == null)
                        throw new ArgumentException(string.Format("type {0} is not implement class of ICache", cacheType.FullName), "cacheType");
                    CacheDic.Add(cacheName, outObj);
                    outObj.DeSerialize();
                }
                return outObj;
            }
        }
        
        public ICache GetCache(string mailboxAddress, string cacheName)
        {
            lock (CacheDic)
            {
                if (!string.IsNullOrEmpty(_mailboxAddress) && !mailboxAddress.Equals(_mailboxAddress))
                {
                    return null;
                }

                ICache outObj;
                if (!CacheDic.TryGetValue(cacheName, out outObj))
                {
                    return null;
                }
                return outObj;
            }
        }

        public void ReleaseCache(bool isSerialize = false)
        {
            lock (CacheDic)
            {
                foreach(var keyValue in CacheDic)
                {
                    keyValue.Value.Serialize();
                }
                CacheDic.Clear();
            }
        }
        

        private string _mailboxAddress;
        private Dictionary<string, ICache> CacheDic = new Dictionary<string, ICache>();
    }
}