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
            SetEnds();
            HeightMap = new float[,]
            {
                // 500, 1000                509, 1000
                {1, 1, 2, 1, 1, 1, 1, 1, 1, 2},
                {1, 1, 2, 1, 1, 1, 1, 1, 1, 1},
                {1, 1, 2, 1, 1, 1, 1, 1, 1, 1},
                {1, 1, 3, 1, 1, 1, 1, 1, 1, 1},
                {1, 1, 3, 1, 1, 1, 1, 1, 1, 1},
                {1, 1, 3, 1, 1, 9, 1, 1, 1, 1},
                {1, 1, 4, 1, 1, 1, 7, 1, 1, 1},
                {1, 1, 4, 1, 1, 1, 1, 1, 1, 1},
                {1, 1, 5, 1, 1, 1, 1, 1, 1, 1},
                {3, 1, 6, 1, 1, 1, 1, 1, 1, 4}
                // 500, 991                 509, 991
            };
        }

        [TestMethod]
        public void Subset_EntireMatrix()
        {
            var tiff = GetSubset(StartX, StartY, 10);

            Assert.AreEqual(StartX, tiff.StartX);
            Assert.AreEqual(StartY, tiff.StartY);
            Assert.AreEqual(EndX, tiff.EndX);
            Assert.AreEqual(EndY, tiff.EndY);
            Assert.AreEqual(Width, tiff.Width);
            Assert.AreEqual(Height, tiff.Height);

            for (var x = StartX; x <= EndX; x++)
            for (var y = StartY; y <= EndY; y++)
                Assert.AreEqual(GetAltitude(x, y), tiff.GetAltitude(x, y));
        }

        [TestMethod]
        public void Subset_TopLeft_3()
        {
            var tiff = GetSubset(500, 998, 3);

            Assert.AreEqual(500, tiff.StartX);
            Assert.AreEqual(998, tiff.StartY);
            Assert.AreEqual(503, tiff.EndX);
            Assert.AreEqual(995, tiff.EndY);
            Assert.AreEqual(3, tiff.Width);
            Assert.AreEqual(3, tiff.Height);

            for (var x = tiff.StartX; x <= tiff.EndX; x++)
            for (var y = tiff.StartY; y <= tiff.EndY; y++)
                Assert.AreEqual(GetAltitude(x, y), tiff.GetAltitude(x, y));
        }

        [TestMethod]
        public void Subset_TopRight_2()
        {
            var tiff = GetSubset(508, 999, 2);

            Assert.AreEqual(508, tiff.StartX);
            Assert.AreEqual(999, tiff.StartY);
            Assert.AreEqual(510, tiff.EndX);
            Assert.AreEqual(997, tiff.EndY);
            Assert.AreEqual(2, tiff.Width);
            Assert.AreEqual(2, tiff.Height);

            for (var x = tiff.StartX; x <= tiff.EndX; x++)
            for (var y = tiff.StartY; y <= tiff.EndY; y++)
                Assert.AreEqual(GetAltitude(x, y), tiff.GetAltitude(x, y));
        }

        [TestMethod]
        public void Subset_BottomLeft_4()
        {
            var tiff = GetSubset(500, 995, 4);

            Assert.AreEqual(500, tiff.StartX);
            Assert.AreEqual(995, tiff.StartY);
            Assert.AreEqual(504, tiff.EndX);
            Assert.AreEqual(991, tiff.EndY);
            Assert.AreEqual(4, tiff.Width);
            Assert.AreEqual(4, tiff.Height);

            for (var x = tiff.StartX; x <= tiff.EndX; x++)
            for (var y = tiff.StartY; y <= tiff.EndY; y++)
                Assert.AreEqual(GetAltitude(x, y), tiff.GetAltitude(x, y));
        }

        [TestMethod]
        public void Subset_BottomRight_1()
        {
            var tiff = GetSubset(509, 991, 1);

            Assert.AreEqual(509, tiff.StartX);
            Assert.AreEqual(991, tiff.StartY);
            Assert.AreEqual(510, tiff.EndX);
            Assert.AreEqual(990, tiff.EndY);
            Assert.AreEqual(1, tiff.Width);
            Assert.AreEqual(1, tiff.Height);

            for (var x = tiff.StartX; x <= tiff.EndX; x++)
            for (var y = tiff.StartY; y <= tiff.EndY; y++)
                Assert.AreEqual(GetAltitude(x, y), tiff.GetAltitude(x, y));
        }

        [TestMethod]
        public void Subset_SomewhereInside_5()
        {
            var tiff = GetSubset(502, 998, 5);

            Assert.AreEqual(502, tiff.StartX);
            Assert.AreEqual(998, tiff.StartY);
            Assert.AreEqual(507, tiff.EndX);
            Assert.AreEqual(993, tiff.EndY);
            Assert.AreEqual(5, tiff.Width);
            Assert.AreEqual(5, tiff.Height);

            for (var x = tiff.StartX; x <= tiff.EndX; x++)
            for (var y = tiff.StartY; y <= tiff.EndY; y++)
                Assert.AreEqual(GetAltitude(x, y), tiff.GetAltitude(x, y));
        }

        [TestMethod]
        public void GetAltitude_Corners()
        {
            Assert.AreEqual(1, GetAltitude(500, 1000));
            Assert.AreEqual(2, GetAltitude(509, 1000));
            Assert.AreEqual(3, GetAltitude(500, 991));
            Assert.AreEqual(4, GetAltitude(509, 991));
        }

        [TestMethod]
        public void GetAltitude_Center()
        {
            Assert.AreEqual(9, GetAltitude(505, 995));
        }


        [TestMethod]
        public void GetAltitudeVector_TopRow()
        {
            var correct = new[] { 1, 1, 2, 1, 1, 1, 1, 1, 1, 2 };
            var actual = GetAltitudeVector(500, 1000, 509, 1000);

            Assert.AreEqual(correct.Length, actual.Length);

            for (var i = 0; i < actual.Length; i++)
            {
                Assert.AreEqual(correct[i], actual[i].Z, 0.0001);
            }
        }


        [TestMethod]
        public void GetAltitudeVector_TopRowRightLeft()
        {
            var correct = new[] { 2, 1, 1, 1, 1, 1, 1, 2, 1, 1 };
            var actual = GetAltitudeVector(509, 1000, 500, 1000);

            Assert.AreEqual(correct.Length, actual.Length);

            for (var i = 0; i < actual.Length; i++)
            {
                Assert.AreEqual(correct[i], actual[i].Z, 0.0001);
            }
        }


        [TestMethod]
        public void GetAltitudeVector_BottomRow()
        {
            var correct = new[] { 3, 1, 6, 1, 1, 1, 1, 1, 1, 4 };
            var actual = GetAltitudeVector(500, 991, 509, 991);

            Assert.AreEqual(correct.Length, actual.Length);

            for (var i = 0; i < actual.Length; i++)
            {
                Assert.AreEqual(correct[i], actual[i].Z, 0.0001);
            }
        }


        [TestMethod]
        public void GetAltitudeVector_BottomRowRightLeft()
        {
            var correct = new[] {4, 1, 1, 1, 1, 1, 1, 6, 1, 3};
            var actual = GetAltitudeVector(509, 991, 500, 991);

            Assert.AreEqual(correct.Length, actual.Length);

            for (var i = 0; i < actual.Length; i++)
            {
                Assert.AreEqual(correct[i], actual[i].Z, 0.0001);
            }
        }


        [TestMethod]
        public void GetAltitudeVector_LeftCol()
        {
            var correct = new[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 3 };
            var actual = GetAltitudeVector(500, 1000, 500, 991);

            Assert.AreEqual(correct.Length, actual.Length);

            for (var i = 0; i < actual.Length; i++)
            {
                Assert.AreEqual(correct[i], actual[i].Z, 0.0001);
            }
        }


        [TestMethod]
        public void GetAltitudeVector_LeftColBottomUp()
        {
            var correct = new[] {3, 1, 1, 1, 1, 1, 1, 1, 1, 1};
            var actual = GetAltitudeVector(500, 991, 500, 1000);

            Assert.AreEqual(correct.Length, actual.Length);

            for (var i = 0; i < actual.Length; i++)
            {
                Assert.AreEqual(correct[i], actual[i].Z, 0.0001);
            }
        }


        [TestMethod]
        public void GetAltitudeVector_RightCol()
        {
            var correct = new[] { 2, 1, 1, 1, 1, 1, 1, 1, 1, 4 };
            var actual = GetAltitudeVector(509, 1000, 509, 991);

            Assert.AreEqual(correct.Length, actual.Length);

            for (var i = 0; i < actual.Length; i++)
            {
                Assert.AreEqual(correct[i], actual[i].Z, 0.0001);
            }
        }


        [TestMethod]
        public void GetAltitudeVector_RightColBottomUp()
        {
            var correct = new[] { 4, 1, 1, 1, 1, 1, 1, 1, 1, 2 };
            var actual = GetAltitudeVector(509, 991, 509, 1000);

            Assert.AreEqual(correct.Length, actual.Length);

            for (var i = 0; i < actual.Length; i++)
            {
                Assert.AreEqual(correct[i], actual[i].Z, 0.0001);
            }
        }


        [TestMethod]
        public void DiagonalTopLeftBottomRight()
        {
            var correct = new[] {1, 1, 1, 2, 1, 1, 1, 9, 7, 7, 1, 1, 1};
            var actual = GetAltitudeVector(500, 1000, 509, 991);

            Assert.AreEqual(correct.Length, actual.Length);

            for (var i = 0; i < actual.Length; i++)
            {
                Assert.AreEqual(correct[i], actual[i].Z, 0.0001);
            }
        }


        [TestMethod]
        public void DiagonalBottomRightTopLeft()
        {
            var correct = new[] { 4, 1, 1, 1, 7, 9, 9, 1, 1, 1, 2, 1, 1 };
            var actual = GetAltitudeVector(509, 991, 500, 1000);

            Assert.AreEqual(correct.Length, actual.Length);

            for (var i = 0; i < actual.Length; i++)
            {
                Assert.AreEqual(correct[i], actual[i].Z, 0.0001);
            }
        }
    }
}
