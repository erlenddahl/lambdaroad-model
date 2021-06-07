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
    public class ActualTiffGeoTiffTests
    {
        private GeoTiff _geotiff;

        [TestInitialize]
        public void Init()
        {
            _geotiff = new GeoTiff(@"C:\Users\Erlend\Desktop\Søppel\2021-06-01 - Lambda-test\DOM\12-14\33-126-145.tif");
        }

        [TestMethod]
        public void VariousCoordinateTests()
        {
            // Random, around center
            Assert.AreEqual(333.48, _geotiff.GetAltitude(299381, 7109380), 0.01);
            Assert.AreEqual(468.00, _geotiff.GetAltitude(295219, 7111848), 0.01);
            Assert.AreEqual(352.1, _geotiff.GetAltitude(302201, 7110536), 0.01);

            // Upper left corner (LaserInnsyn says .48 for some reason)
            Assert.AreEqual(396.44, _geotiff.GetAltitude(290963, 7115490), 0.01);

            // Upper right corner (.38)
            Assert.AreEqual(389.32, _geotiff.GetAltitude(305115, 7115561), 0.01);

            // Lower left corner (.22)
            Assert.AreEqual(409.13, _geotiff.GetAltitude(290702, 7101318), 0.01);

            // Lower right corner
            Assert.AreEqual(496.59, _geotiff.GetAltitude(305178, 7101338), 0.01);

            // Some other random points
            Assert.AreEqual(395.65, _geotiff.GetAltitude(291571, 7115923), 0.01); // .61
            Assert.AreEqual(597.93, _geotiff.GetAltitude(299494, 7112667), 0.01); // .89
        }
    }
}
