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
                new Point4D(0, 0, 1),
                new Point4D(1, 0, 0),
                new Point4D(2, 0, 0),
                new Point4D(3, 0, 5),
                new Point4D(4, 0, 0),
                new Point4D(5, 0, 0),
                new Point4D(6, 0, 3),
                new Point4D(7, 0, 0),
                new Point4D(8, 0, 0),
                new Point4D(9, 0, 0),
                new Point4D(10, 0, 1),
            };

            var (tx, rx) = FindFresnelObstruction(path);
            Assert.AreEqual(3, tx.Index);

            Assert.AreEqual(3, rx.Index);
        }

        [TestMethod]
        public void FindFresnelObstructionNearest2()
        {
            var path = new[]
            {
                new Point4D(0, 0, 1),
                new Point4D(1, 0, 0),
                new Point4D(2, 0, 0),
                new Point4D(3, 0, 5),
                new Point4D(4, 0, 0),
                new Point4D(5, 0, 0),
                new Point4D(6, 0, 4),
                new Point4D(7, 0, 0),
                new Point4D(8, 0, 0),
                new Point4D(9, 0, 0),
                new Point4D(10, 0, 1),
            };

            var (tx, rx) = FindFresnelObstruction(path);
            Assert.AreEqual(3, tx.Index);
            
            Assert.AreEqual(6, rx.Index);
        }

        [TestMethod]
        public void FindFresnelObstructionTxRx()
        {
            var path = new[]
            {
                new Point4D(0, 0, 10),
                new Point4D(1, 0, 6),
                new Point4D(2, 0, 8),
                new Point4D(3, 0, 12),
                new Point4D(4, 0, 2),
                new Point4D(5, 0, 1),
                new Point4D(6, 0, 1),
                new Point4D(7, 0, 3),
                new Point4D(8, 0, 6),
                new Point4D(9, 0, 2),
                new Point4D(10, 0, 2),
            };

            var (tx, _) = FindFresnelObstruction(path);
            Assert.AreEqual(3, tx.Index);
        }

        [TestMethod]
        public void FindFresnelObstructionRxTx()
        {
            var path = new[]
            {
                new Point4D(0, 0, 10),
                new Point4D(1, 0, 6),
                new Point4D(2, 0, 8),
                new Point4D(3, 0, 12),
                new Point4D(4, 0, 2),
                new Point4D(5, 0, 1),
                new Point4D(6, 0, 1),
                new Point4D(7, 0, 3),
                new Point4D(8, 0, 6),
                new Point4D(9, 0, 2),
                new Point4D(10, 0, 2),
            };

            var (_, rx) = FindFresnelObstruction(path);
            Assert.AreEqual(8, rx.Index);
        }
    }
}
