using Arcserve.Office365.Exchange.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.EwsApi.Impl.Impl
{
    public class AutodiscoveryUrlCache : ICache
    {
        public static string CacheName = "AutodiscoveryUrlCache";

        public void AddKeyValue(ICacheKey cacheKey, object cacheValue)
        {
            _dic.Add(cacheKey, cacheValue);
        }

        public void SetKeyValue(ICacheKey cacheKey, object cacheValue)
        {
            _dic[cacheKey] = cacheValue;
        }

        public bool TryGetValue(ICacheKey cacheKey, out object value)
        {
            return _dic.TryGetValue(cacheKey, out value);
        }

        public void DeSerialize()
        {
        }

        public void Serialize()
        {
        }

        public static string GetDomainName(string mailAddress)
        {
            var index = mailAddress.IndexOf("@");
            return mailAddress.Substring(index + 1, mailAddress.Length - index - 1);
        }

        private Dictionary<ICacheKey, object> _dic = new Dictionary<ICacheKey, object>();
    }
}
