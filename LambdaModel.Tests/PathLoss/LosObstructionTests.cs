using System;
using System.Linq;
using LambdaModel.General;
using LambdaModel.PathLoss;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using no.sintef.SpeedModule.Geometry.SimpleStructures;

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

            var tx = path[0];
            var rx = path.Last();

            var sightLineHeightChangePerMeter = (rx.Z - tx.Z) / tx.DistanceTo2D(rx);

            Assert.AreEqual(true, HasLosObstruction(path, 0, path.Length - 1, sightLineHeightChangePerMeter));
        }

        [TestMethod]
        public void FindLosObstructionTxRxNear()
        {
            var path = new[]
            {
                new Point4D(0, 0, 10),
                new Point4D(1, 0, 16),
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

            var tx = path[0];
            var rx = path.Last();

            var sightLineHeightChangePerMeter = (rx.Z - tx.Z) / tx.DistanceTo2D(rx);

            Assert.AreEqual(true, HasLosObstruction(path, 0, path.Length - 1, sightLineHeightChangePerMeter));
        }

        [TestMethod]
        public void FindLosObstructionTxRxRemoved()
        {
            var path = new[]
            {
                new Point4D(0, 0, 10),
                new Point4D(1, 0, 6),
                new Point4D(2, 0, 8),
                new Point4D(3, 0, 3),
                new Point4D(4, 0, 2),
                new Point4D(5, 0, 1),
                new Point4D(6, 0, 1),
                new Point4D(7, 0, 3),
                new Point4D(8, 0, 2),
                new Point4D(9, 0, 6),
                new Point4D(10, 0, 2),
            };

            var tx = path[0];
            var rx = path.Last();

            var sightLineHeightChangePerMeter = (rx.Z - tx.Z) / tx.DistanceTo2D(rx);

            Assert.AreEqual(true, HasLosObstruction(path, 0, path.Length - 1, sightLineHeightChangePerMeter));
        }

        [TestMethod]
        public void FindLosObstructionBetweenNone()
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

            var tx = path[3];
            var rx = path[8];
            var sightLineHeightChangePerMeter = (rx.Z - tx.Z) / tx.DistanceTo2D(rx);

            Assert.AreEqual(false, HasLosObstruction(path, 3, 8, sightLineHeightChangePerMeter));
        }

        [TestMethod]
        public void FindLosObstructionBetweenOne()
        {
            var path = new[]
            {
                new Point4D(0, 0, 10),
                new Point4D(1, 0, 6),
                new Point4D(2, 0, 8),
                new Point4D(3, 0, 12),
                new Point4D(4, 0, 2),
                new Point4D(5, 0, 15),
                new Point4D(6, 0, 1),
                new Point4D(7, 0, 3),
                new Point4D(8, 0, 6),
                new Point4D(9, 0, 2),
                new Point4D(10, 0, 2),
            };

            var tx = path[3];
            var rx = path[8];
            var sightLineHeightChangePerMeter = (rx.Z - tx.Z) / tx.DistanceTo2D(rx);

            Assert.AreEqual(true, HasLosObstruction(path, 3, 8, sightLineHeightChangePerMeter));
        }

        [TestMethod]
        public void FindLosObstructionRxTx()
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

            var tx = path[0];
            var rx = path.Last();

            var sightLineHeightChangePerMeter = (rx.Z - tx.Z) / tx.DistanceTo2D(rx);

            Assert.AreEqual(true, HasLosObstruction(path, 0, path.Length - 1, sightLineHeightChangePerMeter));
        }

        [TestMethod]
        public void FindLosObstructionRxTxNear()
        {
            var path = new[]
            {
                new Point4D(0, 0, 10),
                new Point4D(1, 0, 16),
                new Point4D(2, 0, 8),
                new Point4D(3, 0, 12),
                new Point4D(4, 0, 2),
                new Point4D(5, 0, 1),
                new Point4D(6, 0, 1),
                new Point4D(7, 0, 3),
                new Point4D(8, 0, 6),
                new Point4D(9, 0, 22),
                new Point4D(10, 0, 2),
            };

            var tx = path[0];
            var rx = path.Last();

            var sightLineHeightChangePerMeter = (rx.Z - tx.Z) / tx.DistanceTo2D(rx);

            Assert.AreEqual(true, HasLosObstruction(path, 0, path.Length - 1, sightLineHeightChangePerMeter));
        }

        [TestMethod]
        public void FindLosObstructionRxTxRemoved()
        {
            var path = new[]
            {
                new Point4D(0, 0, 10),
                new Point4D(1, 0, 12),
                new Point4D(2, 0, 2),
                new Point4D(3, 0, 1),
                new Point4D(4, 0, 2),
                new Point4D(5, 0, 1),
                new Point4D(6, 0, 1),
                new Point4D(7, 0, 3),
                new Point4D(8, 0, 1),
                new Point4D(9, 0, 2),
                new Point4D(10, 0, 2),
            };

            var tx = path[0];
            var rx = path.Last();

            var sightLineHeightChangePerMeter = (rx.Z - tx.Z) / tx.DistanceTo2D(rx);

            Assert.AreEqual(true, HasLosObstruction(path, 0, path.Length - 1, sightLineHeightChangePerMeter));
        }
    }
}