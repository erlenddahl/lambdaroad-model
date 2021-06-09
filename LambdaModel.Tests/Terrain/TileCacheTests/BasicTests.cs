﻿using LambdaModel.Terrain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LambdaModel.Tests.Terrain.TileCacheTests
{
    [TestClass]
    public class BasicTests
    {
        private TileCache _tiles;

        [TestInitialize]
        public void Init()
        {
            _tiles = new TileCache(@"..\..\..\..\Data\Testing\CacheTest");
        }

        [TestMethod]
        public void VariousCoordinateTests()
        {
            Assert.AreEqual(462.72, _tiles.GetAltitude(290426, 7100996), 0.01);
            Assert.AreEqual(465.52, _tiles.GetAltitude(290446, 7101028), 0.01);
            Assert.AreEqual(471.51, _tiles.GetAltitude(290486, 7101011), 0.01);
        }

        [TestMethod]
        public void BottomLeftCorner()
        {
            Assert.AreEqual(462.56, _tiles.GetAltitude(290425, 7100995), 0.01);
        }

        [TestMethod]
        public void BottomRightCorner()
        {
            Assert.AreEqual(473.10, _tiles.GetAltitude(290425 + 99, 7100995), 0.01);
        }

        [TestMethod]
        public void TopLeftCorner()
        {
            Assert.AreEqual(453.67, _tiles.GetAltitude(290425, 7100995 + 99), 0.01);
        }

        [TestMethod]
        public void TopRightCorner()
        {
            Assert.AreEqual(453.08, _tiles.GetAltitude(290425 + 99, 7100995 + 99), 0.01);
        }
    }
}
