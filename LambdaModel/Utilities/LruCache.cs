using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LambdaModel.Utilities
{
    public class LruCache<K,T>
    {
        private readonly Dictionary<K, T> _cache = new Dictionary<K, T>();
        private readonly K[] _retrievedKeys;
        private int _retrievedKeyIndex = 0;

        public int RetrievedFromCache { get; private set; }
        public int RemovedFromCache { get; private set; }
        public int AddedToCache { get; private set; }
        public int CurrentlyInCache => _cache.Count;
        public int MaxItems { get; }
        public int RemoveItemsWhenFull { get; set; }

        public Action<T> OnRemoved = null;

        public LruCache(int maxItems, int removeItemsWhenFull)
        {
            MaxItems = maxItems;
            RemoveItemsWhenFull = removeItemsWhenFull;

            _retrievedKeys = new K[maxItems * 5];
        }

        public virtual bool TryGetValue(K key, out T value)
        {
            var result = _cache.TryGetValue(key, out value);
            if (result)
            {
                RetrievedFromCache++;
                MarkRetrieved(key);
            }

            return result;
        }

        private void MarkRetrieved(K key)
        {
            _retrievedKeys[_retrievedKeyIndex++] = key;
            if (_retrievedKeyIndex >= _retrievedKeys.Length)
                _retrievedKeyIndex = 0;
        }

        public void Add(K key, T value)
        {
            if (_cache.Count == MaxItems)
                RemoveLeastRecentlyUsed(RemoveItemsWhenFull);
            
            _cache.Add(key, value);
            MarkRetrieved(key);
            AddedToCache++;
        }

        private void RemoveLeastRecentlyUsed(int remove)
        {
            var lastRecentlyUsed = _retrievedKeys.GroupBy(p => p).OrderBy(p => p.Count()).Select(p => p.Key);
            var removeIds = new HashSet<K>();
            foreach (var key in lastRecentlyUsed)
            {
                if (key == null) continue;
                if (!_cache.TryGetValue(key, out var v)) continue;

                removeIds.Add(key);
                OnRemoved?.Invoke(v);
                _cache.Remove(key);
                RemovedFromCache++;

                if (removeIds.Count >= remove) break;
            }

            for (var i = 0; i < _retrievedKeys.Length; i++)
            {
                if (removeIds.Contains(_retrievedKeys[i]))
                    _retrievedKeys[i] = default;
            }
        }

        public void Clear()
        {
            _cache.Clear();
            for (var i = 0; i < _retrievedKeys.Length; i++)
                _retrievedKeys[i] = default;
            AddedToCache = 0;
            RemovedFromCache = 0;
            RetrievedFromCache = 0;
        }
    }
}
