using System;
using LambdaModel.General;
using LambdaModel.PathLoss;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using no.sintef.SpeedModule.Geometry.SimpleStructures;

namespace LambdaModel.Tests.PathLoss
{
    [TestClass]
    public class FresnelObstructionTests : MobileNetworkPathLossCalculator
    {
        [TestMethod]
        public void FindFresnelObstructionNearest()
        {
            var path = new[]
            {
                new Point4D<double>(0, 0, 0),
                new Point4D<double>(1, 0, 0),
                new Point4D<double>(2, 0, 0),
                new Point4D<double>(3, 0, 5),
                new Point4D<double>(4, 0, 0),
                new Point4D<double>(5, 0, 0),
                new Point4D<double>(6, 0, 3),
                new Point4D<double>(7, 0, 0),
                new Point4D<double>(8, 0, 0),
                new Point4D<double>(9, 0, 0),
                new Point4D<double>(10, 0, 0),
            };

            var r = FindFresnelObstruction(path, CalculationDirection.TxToRx, 1, 1);
            Assert.AreEqual(3, r.index);

            r = FindFresnelObstruction(path, CalculationDirection.RxToTx, 1, 1);
            Assert.AreEqual(3, r.index);
        }

        [TestMethod]
        public void FindFresnelObstructionNearest2()
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

            var r = FindFresnelObstruction(path, CalculationDirection.TxToRx, 1, 1);
            Assert.AreEqual(3, r.index);

            r = FindFresnelObstruction(path, CalculationDirection.RxToTx, 1, 1);
            Assert.AreEqual(6, r.index);
        }

        [TestMethod]
        public void FindFresnelObstructionTxRx()
        {
            var path = new[]
            {
                new Point4D<double>(0, 0, 0),
                new Point4D<double>(1, 0, 6),
                new Point4D<double>(2, 0, 8),
                new Point4D<double>(3, 0, 12),
                new Point4D<double>(4, 0, 2),
                new Point4D<double>(5, 0, 1),
                new Point4D<double>(6, 0, 1),
                new Point4D<double>(7, 0, 3),
                new Point4D<double>(8, 0, 6),
                new Point4D<double>(9, 0, 2),
                new Point4D<double>(10, 0, 2),
            };

            var r = FindFresnelObstruction(path, CalculationDirection.TxToRx, 10, 2);
            Assert.AreEqual(3, r.index);
        }

        [TestMethod]
        public void FindFresnelObstructionRxTx()
        {
            var path = new[]
            {
                new Point4D<double>(0, 0, 0),
                new Point4D<double>(1, 0, 6),
                new Point4D<double>(2, 0, 8),
                new Point4D<double>(3, 0, 12),
                new Point4D<double>(4, 0, 2),
                new Point4D<double>(5, 0, 1),
                new Point4D<double>(6, 0, 1),
                new Point4D<double>(7, 0, 3),
                new Point4D<double>(8, 0, 6),
                new Point4D<double>(9, 0, 2),
                new Point4D<double>(10, 0, 0),
            };

            var r = FindFresnelObstruction(path, CalculationDirection.RxToTx, 10, 2);
            Assert.AreEqual(8, r.index);
        }
    }
}
