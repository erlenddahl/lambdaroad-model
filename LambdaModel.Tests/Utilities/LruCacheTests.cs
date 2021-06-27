using System;
using System.Collections.Generic;
using System.Text;
using LambdaModel.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LambdaModel.Tests.Utilities
{
    [TestClass]
    public class LruCacheTests
    {
        [TestMethod]
        public void GetMissingItemFromEmptyCache()
        {
            var cache = new LruCache<int, int>(10, 2);
            Assert.AreEqual(false, cache.TryGetValue(1, out var v));
        }

        [TestMethod]
        public void GetMissingItemFromNonEmptyCache()
        {
            var cache = new LruCache<int, int>(10, 2);
            cache.Add(5, 5);
            Assert.AreEqual(false, cache.TryGetValue(1, out var v));
        }

        [TestMethod]
        public void GetExistingItem()
        {
            var cache = new LruCache<int, int>(10, 2);
            cache.Add(5, 51);
            Assert.AreEqual(true, cache.TryGetValue(5, out var v));
            Assert.AreEqual(51, v);
        }

        [TestMethod]
        public void AddCountIsCorrect()
        {
            var cache = new LruCache<int, int>(5, 1);
            Assert.AreEqual(0, cache.AddedToCache);
            cache.Add(5, 51);
            Assert.AreEqual(1, cache.AddedToCache);
            cache.TryGetValue(5, out var v);
            Assert.AreEqual(1, cache.AddedToCache);
            cache.Add(6, 52);
            Assert.AreEqual(2, cache.AddedToCache);
            cache.TryGetValue(5, out v);
            Assert.AreEqual(2, cache.AddedToCache);
            cache.TryGetValue(3, out v);
            Assert.AreEqual(2, cache.AddedToCache);
            cache.Add(3, 1);
            Assert.AreEqual(3, cache.AddedToCache);
            cache.TryGetValue(3, out v);
            Assert.AreEqual(3, cache.AddedToCache);

            cache.Add(10, 0);
            Assert.AreEqual(4, cache.AddedToCache);
            cache.Add(11, 0);
            Assert.AreEqual(5, cache.AddedToCache);
            cache.Add(12, 0);
            Assert.AreEqual(6, cache.AddedToCache);
            cache.Add(13, 0);
            Assert.AreEqual(7, cache.AddedToCache);
            cache.Add(14, 0);
            Assert.AreEqual(8, cache.AddedToCache);
            cache.Add(15, 0);
            Assert.AreEqual(9, cache.AddedToCache);
            cache.Add(16, 0);
            Assert.AreEqual(10, cache.AddedToCache);
            cache.TryGetValue(16, out v);
            Assert.AreEqual(10, cache.AddedToCache);
        }

        [TestMethod]
        public void RetrievedCountIsCorrect()
        {
            var cache = new LruCache<int, int>(5, 1);
            Assert.AreEqual(0, cache.RetrievedFromCache);
            cache.Add(5, 51);
            Assert.AreEqual(0, cache.RetrievedFromCache);
            cache.TryGetValue(5, out var v);
            Assert.AreEqual(1, cache.RetrievedFromCache);
            cache.Add(6, 52);
            Assert.AreEqual(1, cache.RetrievedFromCache);
            cache.TryGetValue(5, out v);
            Assert.AreEqual(2, cache.RetrievedFromCache);
            cache.TryGetValue(3, out v);
            Assert.AreEqual(2, cache.RetrievedFromCache);
            cache.Add(3, 1);
            Assert.AreEqual(2, cache.RetrievedFromCache);
            cache.TryGetValue(3, out v);
            Assert.AreEqual(3, cache.RetrievedFromCache);

            cache.Add(10, 0);
            Assert.AreEqual(3, cache.RetrievedFromCache);
            cache.Add(11, 0);
            Assert.AreEqual(3, cache.RetrievedFromCache);
            cache.Add(12, 0);
            Assert.AreEqual(3, cache.RetrievedFromCache);
            cache.Add(13, 0);
            Assert.AreEqual(3, cache.RetrievedFromCache);
            cache.Add(14, 0);
            Assert.AreEqual(3, cache.RetrievedFromCache);
            cache.Add(15, 0);
            Assert.AreEqual(3, cache.RetrievedFromCache);
            cache.Add(16, 0);
            Assert.AreEqual(3, cache.RetrievedFromCache);
            cache.TryGetValue(16, out v);
            Assert.AreEqual(4, cache.RetrievedFromCache);
        }

        [TestMethod]
        public void InCacheCountIsCorrect()
        {
            var cache = new LruCache<int, int>(5, 1);
            Assert.AreEqual(0, cache.CurrentlyInCache);
            cache.Add(5, 51);
            Assert.AreEqual(1, cache.CurrentlyInCache);
            cache.TryGetValue(5, out var v);
            Assert.AreEqual(1, cache.CurrentlyInCache);
            cache.Add(6, 52);
            Assert.AreEqual(2, cache.CurrentlyInCache);
            cache.TryGetValue(5, out v);
            Assert.AreEqual(2, cache.CurrentlyInCache);
            cache.TryGetValue(3, out v);
            Assert.AreEqual(2, cache.CurrentlyInCache);
            cache.Add(3, 1);
            Assert.AreEqual(3, cache.CurrentlyInCache);
            cache.TryGetValue(3, out v);
            Assert.AreEqual(3, cache.CurrentlyInCache);

            cache.Add(10, 0);
            Assert.AreEqual(4, cache.CurrentlyInCache);
            cache.Add(11, 0);
            Assert.AreEqual(5, cache.CurrentlyInCache);
            cache.Add(12, 0);
            Assert.AreEqual(5, cache.CurrentlyInCache);
            cache.Add(13, 0);
            Assert.AreEqual(5, cache.CurrentlyInCache);
            cache.Add(14, 0);
            Assert.AreEqual(5, cache.CurrentlyInCache);
            cache.Add(15, 0);
            Assert.AreEqual(5, cache.CurrentlyInCache);
            cache.Add(16, 0);
            Assert.AreEqual(5, cache.CurrentlyInCache);
            cache.TryGetValue(16, out v);
            Assert.AreEqual(5, cache.CurrentlyInCache);
        }

        [TestMethod]
        public void RemovedCountIsCorrect()
        {
            var cache = new LruCache<int, int>(5, 1);
            Assert.AreEqual(0, cache.RemovedFromCache);
            cache.Add(5, 51);
            Assert.AreEqual(0, cache.RemovedFromCache);
            cache.TryGetValue(5, out var v);
            Assert.AreEqual(0, cache.RemovedFromCache);
            cache.Add(6, 52);
            Assert.AreEqual(0, cache.RemovedFromCache);
            cache.TryGetValue(5, out v);
            Assert.AreEqual(0, cache.RemovedFromCache);
            cache.TryGetValue(3, out v);
            Assert.AreEqual(0, cache.RemovedFromCache);
            cache.Add(3, 1);
            Assert.AreEqual(0, cache.RemovedFromCache);
            cache.TryGetValue(3, out v);
            Assert.AreEqual(0, cache.RemovedFromCache);

            cache.Add(10, 0);
            Assert.AreEqual(0, cache.RemovedFromCache);
            cache.Add(11, 0);
            Assert.AreEqual(0, cache.RemovedFromCache);
            cache.Add(12, 0);
            Assert.AreEqual(1, cache.RemovedFromCache);
            cache.Add(13, 0);
            Assert.AreEqual(2, cache.RemovedFromCache);
            cache.Add(14, 0);
            Assert.AreEqual(3, cache.RemovedFromCache);
            cache.Add(15, 0);
            Assert.AreEqual(4, cache.RemovedFromCache);
            cache.Add(16, 0);
            Assert.AreEqual(5, cache.RemovedFromCache);
            cache.TryGetValue(16, out v);
            Assert.AreEqual(5, cache.RemovedFromCache);
        }

        
        [TestMethod]
        public void CorrectItemIsRemoved()
        {
            var cache = new LruCache<int, int>(5, 1);
            cache.Add(5, 51);
            cache.Add(6, 52);
            cache.Add(13, 1);
            cache.Add(3, 1);
            cache.Add(12, 1);

            cache.TryGetValue(5, out var v);
            cache.TryGetValue(5, out v);
            cache.TryGetValue(5, out v);
            cache.TryGetValue(5, out v);
            cache.TryGetValue(5, out v);
            cache.TryGetValue(6, out v);
            cache.TryGetValue(6, out v);
            cache.TryGetValue(13, out v);
            cache.TryGetValue(6, out v);
            cache.TryGetValue(6, out v);
            cache.TryGetValue(3, out v);
            cache.TryGetValue(3, out v);
            cache.TryGetValue(3, out v);
            cache.TryGetValue(12, out v);
            cache.TryGetValue(12, out v);

            cache.Add(10, 0);

            Assert.AreEqual(false, cache.TryGetValue(13, out v));
        }

        [TestMethod]
        public void CorrectItemsAreRemoved()
        {
            var cache = new LruCache<int, int>(5, 2);
            cache.Add(5, 51);
            cache.Add(6, 52);
            cache.Add(13, 1);
            cache.Add(3, 1);
            cache.Add(12, 1);

            cache.TryGetValue(5, out var v);
            cache.TryGetValue(5, out v);
            cache.TryGetValue(5, out v);
            cache.TryGetValue(5, out v);
            cache.TryGetValue(5, out v);
            cache.TryGetValue(6, out v);
            cache.TryGetValue(6, out v);
            cache.TryGetValue(13, out v);
            cache.TryGetValue(6, out v);
            cache.TryGetValue(6, out v);
            cache.TryGetValue(3, out v);
            cache.TryGetValue(3, out v);
            cache.TryGetValue(3, out v);
            cache.TryGetValue(12, out v);
            cache.TryGetValue(12, out v);

            cache.Add(10, 0);

            Assert.AreEqual(false, cache.TryGetValue(13, out v));
            Assert.AreEqual(false, cache.TryGetValue(12, out v));
        }
    }
}
