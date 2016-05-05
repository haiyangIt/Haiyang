using EwsFrame;
using EwsFrame.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDbImpl.Cache
{
    public class ExistItemCache : ICache
    {


        public static string CacheName = "ExistItemCache";

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

        private Dictionary<ICacheKey, object> _dic = new Dictionary<ICacheKey, object>();
    }
}
