using System;
using System.Linq;
using LambdaModel.General;
using LambdaModel.PathLoss;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LambdaModel.Tests.PathLoss
{
    [TestClass]
    public class LosObstructionTests : PathLossCalculator
    {

        [TestMethod]
        public void FindLosObstructionTxRx()
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

            var tx = path[0];
            var rx = path.Last();

            var sightLineHeightChangePerMeter = (rx.Z - tx.Z) / tx.Distance2d(rx);

            Assert.AreEqual(3, FindLosObstruction(path, 0, path.Length - 1, sightLineHeightChangePerMeter));
        }

        [TestMethod]
        public void FindLosObstructionTxRxNear()
        {
            var path = new[]
            {
                new PointUtm(0, 0, 10),
                new PointUtm(1, 0, 16),
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

            var tx = path[0];
            var rx = path.Last();

            var sightLineHeightChangePerMeter = (rx.Z - tx.Z) / tx.Distance2d(rx);

            Assert.AreEqual(1, FindLosObstruction(path, 0, path.Length - 1, sightLineHeightChangePerMeter));
        }

        [TestMethod]
        public void FindLosObstructionTxRxRemoved()
        {
            var path = new[]
            {
                new PointUtm(0, 0, 10),
                new PointUtm(1, 0, 6),
                new PointUtm(2, 0, 8),
                new PointUtm(3, 0, 3),
                new PointUtm(4, 0, 2),
                new PointUtm(5, 0, 1),
                new PointUtm(6, 0, 1),
                new PointUtm(7, 0, 3),
                new PointUtm(8, 0, 2),
                new PointUtm(9, 0, 6),
                new PointUtm(10, 0, 2),
            };


            var tx = path[0];
            var rx = path.Last();

            var sightLineHeightChangePerMeter = (rx.Z - tx.Z) / tx.Distance2d(rx);

            Assert.AreEqual(9, FindLosObstruction(path, 0, path.Length - 1, sightLineHeightChangePerMeter));
        }

        [TestMethod]
        public void FindLosObstructionBetweenNone()
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

            var tx = path[3];
            var rx = path[8];
            var sightLineHeightChangePerMeter = (rx.Z - tx.Z) / tx.Distance2d(rx);

            Assert.AreEqual(-1, FindLosObstruction(path, 3, 8, sightLineHeightChangePerMeter));
        }

        [TestMethod]
        public void FindLosObstructionBetweenOne()
        {
            var path = new[]
            {
                new PointUtm(0, 0, 10),
                new PointUtm(1, 0, 6),
                new PointUtm(2, 0, 8),
                new PointUtm(3, 0, 12),
                new PointUtm(4, 0, 2),
                new PointUtm(5, 0, 15),
                new PointUtm(6, 0, 1),
                new PointUtm(7, 0, 3),
                new PointUtm(8, 0, 6),
                new PointUtm(9, 0, 2),
                new PointUtm(10, 0, 2),
            };

            var tx = path[3];
            var rx = path[8];
            var sightLineHeightChangePerMeter = (rx.Z - tx.Z) / tx.Distance2d(rx);

            Assert.AreEqual(5, FindLosObstruction(path, 3, 8, sightLineHeightChangePerMeter));
        }

        [TestMethod]
        public void FindLosObstructionRxTx()
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

            var tx = path[0];
            var rx = path.Last();

            var sightLineHeightChangePerMeter = (rx.Z - tx.Z) / tx.Distance2d(rx);

            Assert.AreEqual(8, FindLosObstruction(path, path.Length - 1, 0, -sightLineHeightChangePerMeter));
        }

        [TestMethod]
        public void FindLosObstructionRxTxNear()
        {
            var path = new[]
            {
                new PointUtm(0, 0, 10),
                new PointUtm(1, 0, 16),
                new PointUtm(2, 0, 8),
                new PointUtm(3, 0, 12),
                new PointUtm(4, 0, 2),
                new PointUtm(5, 0, 1),
                new PointUtm(6, 0, 1),
                new PointUtm(7, 0, 3),
                new PointUtm(8, 0, 6),
                new PointUtm(9, 0, 22),
                new PointUtm(10, 0, 2),
            };

            var tx = path[0];
            var rx = path.Last();

            var sightLineHeightChangePerMeter = (rx.Z - tx.Z) / tx.Distance2d(rx);

            Assert.AreEqual(9, FindLosObstruction(path, path.Length - 1, 0, -sightLineHeightChangePerMeter));
        }

        [TestMethod]
        public void FindLosObstructionRxTxRemoved()
        {
            var path = new[]
            {
                new PointUtm(0, 0, 10),
                new PointUtm(1, 0, 12),
                new PointUtm(2, 0, 2),
                new PointUtm(3, 0, 1),
                new PointUtm(4, 0, 2),
                new PointUtm(5, 0, 1),
                new PointUtm(6, 0, 1),
                new PointUtm(7, 0, 3),
                new PointUtm(8, 0, 1),
                new PointUtm(9, 0, 2),
                new PointUtm(10, 0, 2),
            };

            var tx = path[0];
            var rx = path.Last();

            var sightLineHeightChangePerMeter = (rx.Z - tx.Z) / tx.Distance2d(rx);

            Assert.AreEqual(1, FindLosObstruction(path, path.Length - 1, 0, -sightLineHeightChangePerMeter));
        }
    }
}