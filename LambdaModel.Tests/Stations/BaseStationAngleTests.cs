using LambdaModel.Stations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using no.sintef.SpeedModule.Geometry.SimpleStructures;

namespace LambdaModel.Tests.Stations
{
    [TestClass]
    public class BaseStationAngleTests
    {
        [TestMethod]
        public void North()
        {
            Assert.AreEqual(90, new BaseStation() { Center = new Point3D(10, 10, 5) }.AngleTo(new Point3D(10, 20, 7)));
        }

        [TestMethod]
        public void West()
        {
            Assert.AreEqual(180, new BaseStation() { Center = new Point3D(10, 10, 5) }.AngleTo(new Point3D(00, 10, 7)));
        }

        [TestMethod]
        public void East()
        {
            Assert.AreEqual(0, new BaseStation() { Center = new Point3D(10, 10, 5) }.AngleTo(new Point3D(20, 10, 7)));
        }

        [TestMethod]
        public void South()
        {
            Assert.AreEqual(270, new BaseStation() { Center = new Point3D(10, 10, 5) }.AngleTo(new Point3D(10, 00, 7)));
        }
    }
}