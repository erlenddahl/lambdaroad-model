using System;
using LambdaModel.General;
using LambdaModel.PathLoss;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using no.sintef.SpeedModule.Geometry.SimpleStructures;

namespace LambdaModel.Tests.PathLoss
{
    [TestClass]
    public class GetParameterTests : PathLossCalculator
    {

        [TestMethod]
        public void ClearLineOfSight()
        {
            var path = new[]
            {
                new Point3D(0, 0, 1),
                new Point3D(1, 0, 0),
                new Point3D(2, 0, 0),
                new Point3D(3, 0, 0),
                new Point3D(4, 0, 0),
                new Point3D(5, 0, 0),
                new Point3D(6, 0, 0),
                new Point3D(7, 0, 0),
                new Point3D(8, 0, 0),
                new Point3D(9, 0, 0),
                new Point3D(10, 0, 1),
            };

            var p = GetParameters(path);

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
                new Point3D(0, 0, 1),
                new Point3D(1, 0, 0),
                new Point3D(2, 0, 0),
                new Point3D(3, 0, 5),
                new Point3D(4, 0, 0),
                new Point3D(5, 0, 0),
                new Point3D(6, 0, 0),
                new Point3D(7, 0, 0),
                new Point3D(8, 0, 0),
                new Point3D(9, 0, 0),
                new Point3D(10, 0, 1),
            };

            var p = GetParameters(path);

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
                new Point3D(0, 0, 1),
                new Point3D(1, 0, 0),
                new Point3D(2, 0, 0),
                new Point3D(3, 0, 5),
                new Point3D(4, 0, 0),
                new Point3D(5, 0, 0),
                new Point3D(6, 0, 4),
                new Point3D(7, 0, 0),
                new Point3D(8, 0, 0),
                new Point3D(9, 0, 0),
                new Point3D(10, 0, 1),
            };

            var p = GetParameters(path);

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
                new Point3D(0, 0, 1),
                new Point3D(1, 0, 0),
                new Point3D(2, 0, 0),
                new Point3D(3, 0, 5),
                new Point3D(4, 0, 0),
                new Point3D(5, 0, 6),
                new Point3D(6, 0, 6),
                new Point3D(7, 0, 0),
                new Point3D(8, 0, 0),
                new Point3D(9, 0, 0),
                new Point3D(10, 0, 1),
            };

            var p = GetParameters(path);

            Assert.AreEqual(10, p.horizontalDistance, 0.001);
            Assert.AreEqual(6.4, p.rxi, 0.1);
            Assert.AreEqual(5, p.txi, 0.1);
            Assert.AreEqual(3, p.nobs);
        }
    }
}