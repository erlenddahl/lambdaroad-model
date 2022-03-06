using LambdaModel.Stations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LambdaModel.Tests.Stations
{

    [TestClass]
    public class ConstantAntennaGainTests
    {

        [TestMethod]
        public void RestrictAngle_Range()
        {
            for (var i = 0; i < 360; i++)
                Assert.AreEqual(i, AntennaGain.RestrictAngle(i));

            for (var i = 360; i < 720; i++)
                Assert.AreEqual(i - 360, AntennaGain.RestrictAngle(i));
            for (var i = 720; i < 1080; i++)
                Assert.AreEqual(i - 720, AntennaGain.RestrictAngle(i));
        }

        [TestMethod]
        public void ConstantGain()
        {
            var g = AntennaGain.FromConstant(17.6);
            for (var i = 0; i < 360; i++)
                Assert.AreEqual(17.6, g.GetGainAtAngle(i));
        }

        [TestMethod]
        public void LargerThan360()
        {
            var g = AntennaGain.FromConstant(17.6);
            for (var i = 360; i < 918; i += 8)
                Assert.AreEqual(17.6, g.GetGainAtAngle(i));
        }
    }
}