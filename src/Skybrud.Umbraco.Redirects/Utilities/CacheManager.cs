using System;
using System.Runtime.Caching;

namespace Skybrud.Umbraco.Redirects.Utilities
{
    public static class CacheManager
    {
        public static void Cache<T>(string key, T value)
        {
            ObjectCache cache = MemoryCache.Default;

            cache.Set(key, value, DateTime.Now.AddHours(1));
        }
        
        /// <summary>
         /// Returns item from cache by the specified key
         /// </summary>
         /// <returns></returns>
        public static T Get<T>(string key)
        {
            ObjectCache cache = MemoryCache.Default;

            return (T)cache[key];
        }

        /// <summary>
        /// Returns item from cache by the specified key. Adds the item to the cache if it doesn't exist
        /// </summary>
        /// <returns></returns>
        public static T Get<T>(string key, T value)
        {
            ObjectCache cache = MemoryCache.Default;

            if (cache[key] == null)
            {
                cache.Set(key, value, DateTime.Now.AddHours(1));
            }

            return (T)cache[key];
        }

        /// <summary>
        /// Returns item from cache by the specified key. Adds the item to the cache if it doesn't exist
        /// </summary>
        /// <returns></returns>
        public static T Get<T>(string key, Func<T> getValue)
        {
            ObjectCache cache = MemoryCache.Default;

            if (cache[key] == null)
            {
                var val = getValue();

                cache.Set(key, val, DateTime.Now.AddHours(1));
            }

            return (T)cache[key];
        }

        public static void Delete(string key)
        {
            ObjectCache cache = MemoryCache.Default;

            cache.Remove(key);
        }
    }
}
