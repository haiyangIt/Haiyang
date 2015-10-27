using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EwsFrame.Cache
{
    public interface ICache
    {
        bool TryGetValue(ICacheKey cacheKey, out object cacheValue);
        void AddKeyValue(ICacheKey cacheKey, object cacheValue);
        void SetKeyValue(ICacheKey cacheKey, object cacheValue);
        void DeSerialize();
        void Serialize();
    }

    public interface ICacheKey
    {
        int GetHashCode();
        bool Equals(object obj);
    }

    public class StringCacheKey :  ICacheKey
    {
        private readonly string _key;

        public StringCacheKey(string key)
        {
            _key = key;
        }

        public override int GetHashCode()
        {
            return _key.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            StringCacheKey strObj = obj as StringCacheKey;
            if (strObj == null)
                return false;
            return _key.Equals(strObj._key);
        }
    }
}
