using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using LambdaModel.PathLoss;
using LambdaModel.Terrain;
using LambdaModel.Terrain.Tiff;
using LambdaModel.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using no.sintef.SpeedModule.Geometry.SimpleStructures;

namespace LambdaModel.Tests.PathLoss
{
    [TestClass]
    public class MinPathLossTests
    {
        [TestMethod]
        public void RunTileCache()
        {
            var tiles = new TileCache(@"..\..\..\..\Data\Testing\CacheTest", 512)
            {
                CreateTiff = fn => new LazyGeoTiff(fn)
            };

            // Use a station placed in the center of this map tile
            var center = new Point3D(299430, 7108499);
            center.Z = tiles.GetAltitude(center);

            var vector = tiles.GetAltitudeVector(center, center.Move(50000, 0)).ToArray();
            var calc = new PathLossCalculator();
            var start = DateTime.Now;
            var minDiff = double.MaxValue;
            for (var txHeightAboveTerrain = 0; txHeightAboveTerrain < 500; txHeightAboveTerrain += 15)
            {
                for (var i = 2; i < vector.Length; i++)
                {
                    var loss = calc.CalculateLoss(vector, txHeightAboveTerrain, 2, i - 1);
                    var minPossibleLoss = calc.CalculateMinPossibleLoss(vector[i].DistanceTo2D(vector[0]), txHeightAboveTerrain);

                    if (loss - minPossibleLoss < minDiff) minDiff = loss - minPossibleLoss;

                    Assert.IsTrue(loss > minPossibleLoss);
                }
            }

            Console.WriteLine(minDiff);

            var ms = DateTime.Now.Subtract(start).TotalMilliseconds;
            Console.WriteLine("Calculation time: " + ms);
        }
    }
}
