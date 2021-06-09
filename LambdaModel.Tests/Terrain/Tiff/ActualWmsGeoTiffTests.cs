using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using LambdaModel.General;
using LambdaModel.Terrain.Tiff;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LambdaModel.Tests.Terrain.Tiff
{
    [TestClass]
    public class ActualWmsGeoTiffTests
    {
        private GeoTiff _geotiff;

        [TestInitialize]
        public void Init()
        {
            _geotiff = new GeoTiff(@"..\..\..\..\Data\Testing\290425,7100995_100x100.tiff");
        }

        [TestMethod]
        public void VariousCoordinateTests()
        {
            Assert.AreEqual(462.72, _geotiff.GetAltitude(290426, 7100996), 0.01);
            Assert.AreEqual(465.52, _geotiff.GetAltitude(290446, 7101028), 0.01);
            Assert.AreEqual(471.51, _geotiff.GetAltitude(290486, 7101011), 0.01);
        }

        [TestMethod]
        public void BottomLeftCorner()
        {
            Assert.AreEqual(462.56, _geotiff.GetAltitude(290425, 7100995), 0.01);
        }

        [TestMethod]
        public void BottomRightCorner()
        {
            Assert.AreEqual(473.10, _geotiff.GetAltitude(290425 + 99, 7100995), 0.01);
        }

        [TestMethod]
        public void TopLeftCorner()
        {
            Assert.AreEqual(453.67, _geotiff.GetAltitude(290425, 7100995 + 99), 0.01);
        }

        [TestMethod]
        public void TopRightCorner()
        {
            Assert.AreEqual(453.08, _geotiff.GetAltitude(290425 + 99, 7100995 + 99), 0.01);
        }
    }
}
