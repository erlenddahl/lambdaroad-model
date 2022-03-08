using System;
using System.Collections.Generic;
using System.Text;
using LambdaModel.Stations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LambdaModel.Tests.Stations
{
    [TestClass]
    public class AntennaGainTests
    {

        [TestMethod]
        public void SimpleDefinition()
        {
            var g = AntennaGain.FromDefinition("0:180:10|180:360:20");
            for (var i = 0; i < 180; i++)
                Assert.AreEqual(10, g.GetGainAtAngle(i));
            for (var i = 180; i < 360; i++)
                Assert.AreEqual(20, g.GetGainAtAngle(i));
        }
    }
}