using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Appology.Service
{
    public interface ICacheService
    {
        Task<T> GetAsync<T>(string key, Func<Task<T>> action, int? cacheTime = null);
        IList<string> GetKeys();
        void Remove(string key);
        void RemoveAll(string cachePrefix = null);
    }

    public class CacheService : ICacheService
    {
        private IMemoryCache _cache;
        protected static ConcurrentDictionary<string, bool> _allKeys;
        protected CancellationTokenSource _cancellationTokenSource;


        public CacheService()
        {
            _cache = new MemoryCache(new MemoryCacheOptions());
            _cancellationTokenSource = new CancellationTokenSource();

            _allKeys = new ConcurrentDictionary<string, bool>();
        }

        protected MemoryCacheEntryOptions GetMemoryCacheEntryOptions(TimeSpan cacheTime)
        {
            var options = new MemoryCacheEntryOptions()
                // add cancellation token for clear cache
                .AddExpirationToken(new CancellationChangeToken(_cancellationTokenSource.Token))
                //add post eviction callback
                .RegisterPostEvictionCallback(PostEviction);

            //set cache time
            options.AbsoluteExpirationRelativeToNow = cacheTime;

            return options;
        }

        protected string AddKey(string key)
        {
            _allKeys.TryAdd(key, true);
            return key;
        }

        protected string RemoveKey(string key)
        {
            TryRemoveKey(key);
            return key;
        }

        public IList<string> GetKeys() => _allKeys.Keys.ToList();

        protected void TryRemoveKey(string key)
        {
            //try to remove key from dictionary
            if (!_allKeys.TryRemove(key, out _))
                //if not possible to remove key from dictionary, then try to mark key as not existing in cache
                _allKeys.TryUpdate(key, false, true);
        }

        public void RemoveAll(string cachePrefix = null)
        {
            if (_allKeys.Any())
            {
                foreach (var key in _allKeys.Where(x => cachePrefix == null || x.Key.StartsWith(cachePrefix)))
                {
                    _cache.Remove(RemoveKey(key.Key));
                }
            }
        }

        private void ClearKeys()
        {
            foreach (var key in _allKeys.Where(p => !p.Value).Select(p => p.Key).ToList())
            {
                RemoveKey(key);
            }
        }

        private void PostEviction(object key, object value, EvictionReason reason, object state)
        {
            //if cached item just change, then nothing doing
            if (reason == EvictionReason.Replaced)
                return;

            //try to remove all keys marked as not existing
            ClearKeys();

            //try to remove this key from dictionary
            TryRemoveKey(key.ToString());
        }

        public async Task<T> GetAsync<T>(string key, Func<Task<T>> action, int? cacheTime = null)
        {
            if (!_cache.TryGetValue(key, out T cacheEntry))// Look for cache key.
            {
                // Key not in cache, so get data.
                cacheEntry = await action();

                // Save data in cache.
                _cache.Set(key, cacheEntry);

                //and set in cache (if cache time is defined)
                if (cacheTime.HasValue)
                    Set(key, cacheEntry, cacheTime.Value);

            }

            if (!_allKeys.ContainsKey(key))
            {
                AddKey(key);
            }

            return cacheEntry;
        }

        public void Set(string key, object data, int cacheTime)
        {
            if (data != null)
            {
                _cache.Set(AddKey(key), data, GetMemoryCacheEntryOptions(TimeSpan.FromMinutes(cacheTime)));
            }
        }

        public  bool IsSet(string key)
        {
            return _cache.TryGetValue(key, out object _);
        }

        public bool PerformActionWithLock(string key, TimeSpan expirationTime, Action action)
        {
            //ensure that lock is acquired
            if (!_allKeys.TryAdd(key, true))
                return false;

            try
            {
                _cache.Set(key, key, GetMemoryCacheEntryOptions(expirationTime));

                //perform action
                action();

                return true;
            }
            finally
            {
                //release lock even if action fails
                Remove(key);
            }
        }

        public void Remove(string key)
        {
            _cache.Remove(RemoveKey(key));
        }


        public void Clear()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
        }
    }
}
