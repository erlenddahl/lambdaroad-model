using System;
using LambdaModel.General;
using LambdaModel.PathLoss;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using no.sintef.SpeedModule.Geometry.SimpleStructures;

namespace LambdaModel.Tests.PathLoss
{
    [TestClass]
    public class GetParameterTests : MobileNetworkPathLossCalculator
    {

        [TestMethod]
        public void ClearLineOfSight()
        {
            var path = new[]
            {
                new Point4D<double>(0, 0, 0),
                new Point4D<double>(1, 0, 0),
                new Point4D<double>(2, 0, 0),
                new Point4D<double>(3, 0, 0),
                new Point4D<double>(4, 0, 0),
                new Point4D<double>(5, 0, 0),
                new Point4D<double>(6, 0, 0),
                new Point4D<double>(7, 0, 0),
                new Point4D<double>(8, 0, 0),
                new Point4D<double>(9, 0, 0),
                new Point4D<double>(10, 0, 0),
            };

            var p = GetParameters(path, 1, 1);

            Assert.AreEqual(10, p.horizontalDistance, 0.001);
            Assert.AreEqual(0, p.rxi, 0.001);
            Assert.AreEqual(0, p.txi, 0.001);
            Assert.AreEqual(0, p.nobs, 0.001);
        }

        [TestMethod]
        public void SingleObstruction()
        {
            var path = new[]
            {
                new Point4D<double>(0, 0, 0),
                new Point4D<double>(1, 0, 0),
                new Point4D<double>(2, 0, 0),
                new Point4D<double>(3, 0, 5),
                new Point4D<double>(4, 0, 0),
                new Point4D<double>(5, 0, 0),
                new Point4D<double>(6, 0, 0),
                new Point4D<double>(7, 0, 0),
                new Point4D<double>(8, 0, 0),
                new Point4D<double>(9, 0, 0),
                new Point4D<double>(10, 0, 0),
            };

            var p = GetParameters(path, 1, 1);

            Assert.AreEqual(10, p.horizontalDistance, 0.001);
            Assert.AreEqual(8.1, p.rxi, 0.1);
            Assert.AreEqual(5, p.txi, 0.1);
            Assert.AreEqual(1, p.nobs);
        }

        [TestMethod]
        public void TwoObstructions()
        {
            var path = new[]
            {
                new Point4D<double>(0, 0, 0),
                new Point4D<double>(1, 0, 0),
                new Point4D<double>(2, 0, 0),
                new Point4D<double>(3, 0, 5),
                new Point4D<double>(4, 0, 0),
                new Point4D<double>(5, 0, 0),
                new Point4D<double>(6, 0, 4),
                new Point4D<double>(7, 0, 0),
                new Point4D<double>(8, 0, 0),
                new Point4D<double>(9, 0, 0),
                new Point4D<double>(10, 0, 0),
            };

            var p = GetParameters(path, 1, 1);

            Assert.AreEqual(10, p.horizontalDistance, 0.001);
            Assert.AreEqual(5, p.rxi, 0.1);
            Assert.AreEqual(5, p.txi, 0.1);
            Assert.AreEqual(2, p.nobs);
        }

        [TestMethod]
        public void ThreeObstructions()
        {
            var path = new[]
            {
                new Point4D<double>(0, 0, 0),
                new Point4D<double>(1, 0, 0),
                new Point4D<double>(2, 0, 0),
                new Point4D<double>(3, 0, 5),
                new Point4D<double>(4, 0, 0),
                new Point4D<double>(5, 0, 6),
                new Point4D<double>(6, 0, 6),
                new Point4D<double>(7, 0, 0),
                new Point4D<double>(8, 0, 0),
                new Point4D<double>(9, 0, 0),
                new Point4D<double>(10, 0, 0),
            };

            var p = GetParameters(path, 1, 1);

            Assert.AreEqual(10, p.horizontalDistance, 0.001);
            Assert.AreEqual(6.4, p.rxi, 0.1);
            Assert.AreEqual(5, p.txi, 0.1);
            Assert.AreEqual(3, p.nobs);
        }
    }
}