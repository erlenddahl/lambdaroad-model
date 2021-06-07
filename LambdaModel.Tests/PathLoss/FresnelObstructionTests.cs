using System;
using LambdaModel.General;
using LambdaModel.PathLoss;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
                new PointUtm(0, 0, 1),
                new PointUtm(1, 0, 0),
                new PointUtm(2, 0, 0),
                new PointUtm(3, 0, 5),
                new PointUtm(4, 0, 0),
                new PointUtm(5, 0, 0),
                new PointUtm(6, 0, 3),
                new PointUtm(7, 0, 0),
                new PointUtm(8, 0, 0),
                new PointUtm(9, 0, 0),
                new PointUtm(10, 0, 1),
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
                new PointUtm(0, 0, 1),
                new PointUtm(1, 0, 0),
                new PointUtm(2, 0, 0),
                new PointUtm(3, 0, 5),
                new PointUtm(4, 0, 0),
                new PointUtm(5, 0, 0),
                new PointUtm(6, 0, 4),
                new PointUtm(7, 0, 0),
                new PointUtm(8, 0, 0),
                new PointUtm(9, 0, 0),
                new PointUtm(10, 0, 1),
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
                new PointUtm(0, 0, 10),
                new PointUtm(1, 0, 6),
                new PointUtm(2, 0, 8),
                new PointUtm(3, 0, 12),
                new PointUtm(4, 0, 2),
                new PointUtm(5, 0, 1),
                new PointUtm(6, 0, 1),
                new PointUtm(7, 0, 3),
                new PointUtm(8, 0, 6),
                new PointUtm(9, 0, 2),
                new PointUtm(10, 0, 2),
            };

            var r = FindFresnelObstruction(path, true);
            Assert.AreEqual(3, r.index);
        }

        [TestMethod]
        public void FindFresnelObstructionRxTx()
        {
            var path = new[]
            {
                new PointUtm(0, 0, 10),
                new PointUtm(1, 0, 6),
                new PointUtm(2, 0, 8),
                new PointUtm(3, 0, 12),
                new PointUtm(4, 0, 2),
                new PointUtm(5, 0, 1),
                new PointUtm(6, 0, 1),
                new PointUtm(7, 0, 3),
                new PointUtm(8, 0, 6),
                new PointUtm(9, 0, 2),
                new PointUtm(10, 0, 2),
            };

            var r = FindFresnelObstruction(path, false);
            Assert.AreEqual(8, r.index);
        }
    }
}
