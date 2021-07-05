using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LambdaModel.General;
using LambdaModel.Stations;
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
            var bs = new RoadLinkBaseStation(0, 0, 100) {MaxRadius = 100};
            ShapeLink.ReadLinks(@"..\..\..\..\Data\RoadNetwork\2021-05-28_smaller.shp", new[] {bs});
            Assert.AreEqual(0, bs.Links.Count);
        }

        [TestMethod]
        public void OneHere()
        {
            var bs = new RoadLinkBaseStation(275007.95, 7042725.97, 100) { MaxRadius = 50 };
            ShapeLink.ReadLinks(@"..\..\..\..\Data\RoadNetwork\2021-05-28_smaller.shp", new[] { bs });
            Assert.AreEqual(1, bs.Links.Count);
        }

        [TestMethod]
        public void ALotHere()
        {
            var bs = new RoadLinkBaseStation(288608.1, 7033525.3, 100) { MaxRadius = 1000 };
            ShapeLink.ReadLinks(@"..\..\..\..\Data\RoadNetwork\2021-05-28_smaller.shp", new[] { bs });
            Assert.AreEqual(42, bs.Links.Count);
        }

        [TestMethod]
        public void MoreHere()
        {
            var bs = new RoadLinkBaseStation(271868.3, 7041337.0, 100) { MaxRadius = 5_000 };
            ShapeLink.ReadLinks(@"..\..\..\..\Data\RoadNetwork\2021-05-28_smaller.shp", new[] { bs });
            Assert.AreEqual(8492, bs.Links.Count);
        }
    }
}
