using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LambdaModel.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using no.sintef.SpeedModule.Geometry.SimpleStructures;

namespace LambdaModel.Tests.FullRun.RoadNetwork
{
    [TestClass]
    public class FindLinksTests
    {
        [TestMethod]
        public void NothingHere()
        {
            var links = ShapeLink.ReadLinks(@"..\..\..\..\Data\RoadNetwork\2021-05-28_smaller.shp", new Point3D(0, 0), 100).ToArray();
            Assert.AreEqual(0, links.Length);
        }

        [TestMethod]
        public void OneHere()
        {
            var links = ShapeLink.ReadLinks(@"..\..\..\..\Data\RoadNetwork\2021-05-28_smaller.shp", new Point3D(275007.95, 7042725.97), 50).ToArray();
            Assert.AreEqual(1, links.Length);
        }

        [TestMethod]
        public void ALotHere()
        {
            var links = ShapeLink.ReadLinks(@"..\..\..\..\Data\RoadNetwork\2021-05-28_smaller.shp", new Point3D(288608.1, 7033525.3), 1000).ToArray();
            Assert.AreEqual(42, links.Length);
        }

        [TestMethod]
        public void MoreHere()
        {
            var links = ShapeLink.ReadLinks(@"..\..\..\..\Data\RoadNetwork\2021-05-28_smaller.shp", new Point3D(271868.3, 7041337.0), 5_000).ToArray();
            Assert.AreEqual(8492, links.Length);
        }
    }
}
