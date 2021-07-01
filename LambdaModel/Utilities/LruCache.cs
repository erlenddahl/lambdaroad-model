using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LambdaModel.Utilities
{
    public class LruCache<K,T>
    {
        protected readonly Dictionary<K, CacheItem<T>> _cache = new Dictionary<K, CacheItem<T>>();

        public int RetrievedFromCache { get; private set; }
        public int RemovedFromCache { get; private set; }
        public int CacheRemovals { get; private set; }
        public int AddedToCache { get; private set; }
        public int CurrentlyInCache => _cache.Count;
        public int MaxItems { get; }
        public int RemoveItemsWhenFull { get; set; }

        public Action<T> OnRemoved = null;

        private int _ageCounter = 0;

        public LruCache(int maxItems, int removeItemsWhenFull)
        {
            MaxItems = maxItems;
            RemoveItemsWhenFull = removeItemsWhenFull;
        }

        public virtual bool TryGetValue(K key, out T value)
        {
            var result = _cache.TryGetValue(key, out var cacheItem);
            if (result)
            {
                RetrievedFromCache++;
                cacheItem.AddedAt = _ageCounter++;
                value = cacheItem.Item;
            }
            else
            {
                value = default;
            }

            if (_ageCounter == int.MaxValue) ResetAge();

            return result;
        }

        private void ResetAge()
        {
            _ageCounter = 0;
            foreach (var item in _cache.Values.OrderBy(p => p.AddedAt))
                item.AddedAt = _ageCounter++;
        }

        public int GetCreationAge(K key)
        {
            return _cache[key].AddedAt;
        }

        public void Add(K key, T value)
        {
            if (_cache.Count == MaxItems)
                RemoveLeastRecentlyUsed(RemoveItemsWhenFull);

            _cache.Add(key, new CacheItem<T>(value)
            {
                AddedAt = _ageCounter++
            });
            AddedToCache++;

            if (_ageCounter == int.MaxValue)
                ResetAge();
        }

        private void RemoveLeastRecentlyUsed(int remove)
        {
            var lastRecentlyUsed = _cache.OrderBy(p => p.Value.AddedAt).Take(remove).ToArray();
            foreach (var item in lastRecentlyUsed)
            {
                OnRemoved?.Invoke(item.Value.Item);
                _cache.Remove(item.Key);
                RemovedFromCache++;
            }

            CacheRemovals++;
        }

        public void Clear()
        {
            _cache.Clear();
            AddedToCache = 0;
            RemovedFromCache = 0;
            RetrievedFromCache = 0;
            CacheRemovals = 0;
        }
    }

    public class CacheItem<T>
    {
        public T Item;
        public int AddedAt;

        public CacheItem(T value)
        {
            Item = value;
        }
    }
}
