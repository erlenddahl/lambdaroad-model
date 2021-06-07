using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LambdaModel.Terrain.Tiff;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LambdaModel.Tests.Terrain.Tiff
{
    [TestClass]
    public class GeoTiffTests : GeoTiff
    {
        [TestInitialize]
        public void Initialize()
        {
            StartX = 500;
            StartY = 1000;
            Width = 10;
            Height = 10;
            HeightMap = new float[,]
            {
                {1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 2},
                {1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 1},
                {1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 1},
                {1, 1, 3, 1, 1, 1, 1, 1, 1, 1, 1},
                {1, 1, 3, 1, 1, 1, 1, 1, 1, 1, 1},
                {1, 1, 3, 1, 1, 9, 1, 1, 1, 1, 1},
                {1, 1, 4, 1, 1, 1, 7, 1, 1, 1, 1},
                {1, 1, 4, 1, 1, 1, 1, 1, 1, 1, 1},
                {1, 1, 5, 1, 1, 1, 1, 1, 1, 1, 1},
                {1, 1, 5, 1, 1, 1, 1, 1, 1, 1, 1},
                {3, 1, 6, 1, 1, 1, 1, 1, 1, 1, 4}
            };
        }

        [TestMethod]
        public void GetAltitude_Corners()
        {
            Assert.AreEqual(1, GetAltitude(500, 1000));
            Assert.AreEqual(2, GetAltitude(510, 1000));
            Assert.AreEqual(3, GetAltitude(500, 990));
            Assert.AreEqual(4, GetAltitude(510, 990));
        }

        [TestMethod]
        public void GetAltitude_Center()
        {
            Assert.AreEqual(9, GetAltitude(505, 995));
        }


        [TestMethod]
        public void GetAltitudeVector_TopRow()
        {
            var correct = new[] { 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 2 };
            var actual = GetAltitudeVector(500, 1000, 510, 1000);

            Assert.AreEqual(correct.Length, actual.Count);

            for (var i = 0; i < actual.Count; i++)
            {
                Assert.AreEqual(correct[i], actual[i].Z, 0.0001);
            }
        }


        [TestMethod]
        public void GetAltitudeVector_TopRowRightLeft()
        {
            var correct = new[] { 2, 1, 1, 1, 1, 1, 1, 1, 2, 1, 1 };
            var actual = GetAltitudeVector(510, 1000, 500, 1000);

            Assert.AreEqual(correct.Length, actual.Count);

            for (var i = 0; i < actual.Count; i++)
            {
                Assert.AreEqual(correct[i], actual[i].Z, 0.0001);
            }
        }


        [TestMethod]
        public void GetAltitudeVector_BottomRow()
        {
            var correct = new[] { 3, 1, 6, 1, 1, 1, 1, 1, 1, 1, 4 };
            var actual = GetAltitudeVector(500, 990, 510, 990);

            Assert.AreEqual(correct.Length, actual.Count);

            for (var i = 0; i < actual.Count; i++)
            {
                Assert.AreEqual(correct[i], actual[i].Z, 0.0001);
            }
        }


        [TestMethod]
        public void GetAltitudeVector_BottomRowRightLeft()
        {
            var correct = new[] {4, 1, 1, 1, 1, 1, 1, 1, 6, 1, 3};
            var actual = GetAltitudeVector(510, 990, 500, 990);

            Assert.AreEqual(correct.Length, actual.Count);

            for (var i = 0; i < actual.Count; i++)
            {
                Assert.AreEqual(correct[i], actual[i].Z, 0.0001);
            }
        }


        [TestMethod]
        public void GetAltitudeVector_LeftCol()
        {
            var correct = new[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 3 };
            var actual = GetAltitudeVector(500, 1000, 500, 990);

            Assert.AreEqual(correct.Length, actual.Count);

            for (var i = 0; i < actual.Count; i++)
            {
                Assert.AreEqual(correct[i], actual[i].Z, 0.0001);
            }
        }


        [TestMethod]
        public void GetAltitudeVector_LeftColBottomUp()
        {
            var correct = new[] {3, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1};
            var actual = GetAltitudeVector(500, 990, 500, 1000);

            Assert.AreEqual(correct.Length, actual.Count);

            for (var i = 0; i < actual.Count; i++)
            {
                Assert.AreEqual(correct[i], actual[i].Z, 0.0001);
            }
        }


        [TestMethod]
        public void GetAltitudeVector_RightCol()
        {
            var correct = new[] { 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 4 };
            var actual = GetAltitudeVector(510, 1000, 510, 990);

            Assert.AreEqual(correct.Length, actual.Count);

            for (var i = 0; i < actual.Count; i++)
            {
                Assert.AreEqual(correct[i], actual[i].Z, 0.0001);
            }
        }


        [TestMethod]
        public void GetAltitudeVector_RightColBottomUp()
        {
            var correct = new[] { 4, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2 };
            var actual = GetAltitudeVector(510, 990, 510, 1000);

            Assert.AreEqual(correct.Length, actual.Count);

            for (var i = 0; i < actual.Count; i++)
            {
                Assert.AreEqual(correct[i], actual[i].Z, 0.0001);
            }
        }


        [TestMethod]
        public void DiagonalTopLeftBottomRight()
        {
            var correct = new[] { 1, 1, 1, 2, 1, 1, 1, 9, 7, 7, 1, 1, 1, 1, 4 };
            var actual = GetAltitudeVector(500, 1000, 510, 990);

            Assert.AreEqual(correct.Length, actual.Count);

            for (var i = 0; i < actual.Count; i++)
            {
                Assert.AreEqual(correct[i], actual[i].Z, 0.0001);
            }
        }


        [TestMethod]
        public void DiagonalBottomRightTopLeft()
        {
            var correct = new[] { 4, 1, 1, 1, 1, 7, 7, 9, 1, 1, 1, 2, 2, 1, 1 };
            var actual = GetAltitudeVector(510, 990, 500, 1000);

            Assert.AreEqual(correct.Length, actual.Count);

            for (var i = 0; i < actual.Count; i++)
            {
                Assert.AreEqual(correct[i], actual[i].Z, 0.0001);
            }
        }
    }
}
