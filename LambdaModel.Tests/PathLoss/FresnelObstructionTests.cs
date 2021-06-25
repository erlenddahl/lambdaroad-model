using System;
using LambdaModel.General;
using LambdaModel.PathLoss;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using no.sintef.SpeedModule.Geometry.SimpleStructures;

namespace LambdaModel.Tests.PathLoss
{
    [TestClass]
    public class FresnelObstructionTests : PathLossCalculator
    {
        [TestMethod]
        public void FindFresnelObstructionNearest()
        {
            var path = new[]
            {
                new Point3D(0, 0, 1),
                new Point3D(1, 0, 0),
                new Point3D(2, 0, 0),
                new Point3D(3, 0, 5),
                new Point3D(4, 0, 0),
                new Point3D(5, 0, 0),
                new Point3D(6, 0, 3),
                new Point3D(7, 0, 0),
                new Point3D(8, 0, 0),
                new Point3D(9, 0, 0),
                new Point3D(10, 0, 1),
            };

            var r = FindFresnelObstruction(path, true);
            Assert.AreEqual(3, r.index);

            r = FindFresnelObstruction(path, false);
            Assert.AreEqual(3, r.index);
        }

        [TestMethod]
        public void FindFresnelObstructionNearest2()
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

            var r = FindFresnelObstruction(path, true);
            Assert.AreEqual(3, r.index);

            r = FindFresnelObstruction(path, false);
            Assert.AreEqual(6, r.index);
        }

        [TestMethod]
        public void FindFresnelObstructionTxRx()
        {
            var path = new[]
            {
                new Point3D(0, 0, 10),
                new Point3D(1, 0, 6),
                new Point3D(2, 0, 8),
                new Point3D(3, 0, 12),
                new Point3D(4, 0, 2),
                new Point3D(5, 0, 1),
                new Point3D(6, 0, 1),
                new Point3D(7, 0, 3),
                new Point3D(8, 0, 6),
                new Point3D(9, 0, 2),
                new Point3D(10, 0, 2),
            };

            var r = FindFresnelObstruction(path, true);
            Assert.AreEqual(3, r.index);
        }

        [TestMethod]
        public void FindFresnelObstructionRxTx()
        {
            var path = new[]
            {
                new Point3D(0, 0, 10),
                new Point3D(1, 0, 6),
                new Point3D(2, 0, 8),
                new Point3D(3, 0, 12),
                new Point3D(4, 0, 2),
                new Point3D(5, 0, 1),
                new Point3D(6, 0, 1),
                new Point3D(7, 0, 3),
                new Point3D(8, 0, 6),
                new Point3D(9, 0, 2),
                new Point3D(10, 0, 2),
            };

            var r = FindFresnelObstruction(path, false);
            Assert.AreEqual(8, r.index);
        }
    }
}
