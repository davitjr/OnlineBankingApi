using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text;
using XBS;

namespace OnlineBankingLibrary.Utilities
{
    public class CacheManager
    {
        private readonly IMemoryCache _cache;


        public CacheManager(IMemoryCache cache)
        {
            _cache = cache;
        }

        public void Set<K, V>(K key, V value, int expiration = 2)
        {
            _cache.Set(key, value, new MemoryCacheEntryOptions() { AbsoluteExpiration = DateTime.Now.AddMinutes(expiration) });
        }

        public void Remove<K>(K key)
        {
            _cache.Remove(key);
        }

        public V Get<K, V>(K key)
        {
            V value;
            if(!_cache.TryGetValue(key, out value))
            {
                value = default(V);
            }
            return value;
        }

        public V GetOrAdd<K, V>(K key, V value)
        {
            V ResultValue;
            if (!_cache.TryGetValue(key, out ResultValue))
            {
                Set(key, value);
                ResultValue = value;
            }

            return ResultValue;
        }


        public void UpdateExpiration<K>(K key, int expiration = 1)
        {
            object value;
            if(_cache.TryGetValue(key, out value))
            {
                _cache.Set(key, _cache.Get(key), new MemoryCacheEntryOptions() { AbsoluteExpiration = DateTime.Now.AddMinutes(expiration) });
            }
        }
    }
}
