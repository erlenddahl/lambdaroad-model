using System;
using LambdaModel.Stations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LambdaModel.Tests.Stations
{
    [TestClass]
    public class RestrictAngleTests
    {
        [TestMethod]
        public void WithinLimits()
        {
            Assert.AreEqual(330, AntennaGain.RestrictAngle(330));
        }

        [TestMethod]
        public void LargerThan360()
        {
            Assert.AreEqual(30, AntennaGain.RestrictAngle(390));
        }
    }
}