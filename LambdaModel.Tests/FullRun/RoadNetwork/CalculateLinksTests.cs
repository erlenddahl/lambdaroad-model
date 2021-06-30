using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using LambdaModel.Calculations;
using LambdaModel.General;
using LambdaModel.Terrain;
using LambdaModel.Terrain.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using no.sintef.SpeedModule.Geometry.SimpleStructures;

namespace LambdaModel.Tests.FullRun.RoadNetwork
{
    [TestClass]
    public class CalculateLinksTests
    {
        public void TinyTest(int tileSize)
        {
            var tiles = new OnlineTileCache(@"..\..\..\..\Data\Testing\CacheTest", tileSize);
            var road = new RoadNetworkCalculator(tiles, @"..\..\..\..\Data\RoadNetwork\2021-05-28_smaller.shp", 500, new Point3D(271327, 7040324), 100);

            var start = DateTime.Now;
            var calculations = road.Calculate();
            var secs = DateTime.Now.Subtract(start).TotalSeconds;
            Console.WriteLine($"Calculation time: {secs:n2} seconds.");
            Console.WriteLine($"Calculations: {calculations:n0}, {(calculations / secs):n2} c/s");

            start = DateTime.Now;
            road.SaveResults(@"..\..\..\..\Data\RoadNetwork\test-results-tiny.shp");
            Console.WriteLine($"Saving time: {DateTime.Now.Subtract(start).TotalSeconds:n2} seconds.");
        }

        [TestMethod]
        public void Size_050()
        {
            TinyTest(50);
        }

        [TestMethod]
        public void Size_100()
        {
            TinyTest(100);
        }

        [TestMethod]
        public void Size_256()
        {
            TinyTest(256);
        }

        [TestMethod]
        public void Size_512()
        {
            TinyTest(512);
        }
    }
}
