using EwsFrame.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EwsFrame.Cache
{
    public class MailboxCacheManager : ICacheManager
    {
        private static MailboxCacheManager _CacheManager;

        public static MailboxCacheManager CacheManager
        {
            get
            {
                if (_CacheManager == null)
                    _CacheManager = new MailboxCacheManager();
                return _CacheManager;
            }
        }

        public ICache NewCache(string mailboxAddress, string cacheName, Type cacheType)
        {
            ICache result = null;
            using (CacheDic.LockWhile(() =>
            {
                if (string.IsNullOrEmpty(_mailboxAddress))
                    _mailboxAddress = mailboxAddress;

                if (!mailboxAddress.Equals(_mailboxAddress))
                {
                    CacheDic.Clear();
                }

                ICache outObj;
                if (!CacheDic.TryGetValue(cacheName, out outObj))
                {
                    outObj = cacheType.GetConstructor(new Type[0]).Invoke(new object[0]) as ICache;
                    if (outObj == null)
                        throw new ArgumentException(string.Format("type {0} is not implement class of ICache", cacheType.FullName), "cacheType");
                    CacheDic.Add(cacheName, outObj);
                    outObj.DeSerialize();
                }
                result = outObj;
            }))
            { }
            return result;
        }

        public ICache GetCache(string mailboxAddress, string cacheName)
        {
            ICache result = null;
            using (CacheDic.LockWhile(() =>
            {
                if (!string.IsNullOrEmpty(_mailboxAddress) && !mailboxAddress.Equals(_mailboxAddress))
                {
                    result = null;
                    return;
                }

                ICache outObj;
                if (!CacheDic.TryGetValue(cacheName, out outObj))
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


        private string _mailboxAddress;
        private Dictionary<string, ICache> CacheDic = new Dictionary<string, ICache>();
    }
}