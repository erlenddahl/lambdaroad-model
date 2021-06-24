using System;
using System.Diagnostics;
using LambdaModel.General;
using LambdaModel.PathLoss;
using LambdaModel.Terrain;
using LambdaModel.Terrain.Tiff;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LambdaModel.Tests.FullRun.Grid
{
    [TestClass]
    public class TileCacheTests
    {
        public void RunTileCache(int tileSize)
        {
            var tiles = new TileCache(@"..\..\..\..\Data\Testing\CacheTest", tileSize);
            
            // Use a station placed in the center of this map tile
            var stationCoordinates = new PointUtm(299430, 7108499);
            stationCoordinates.Z = tiles.GetAltitude(stationCoordinates);

            var coverageRadius = 500;

            var start = DateTime.Now;
            var results = new double[coverageRadius * 2 + 1, coverageRadius * 2 + 1];
            
            for (var x = -coverageRadius; x <= coverageRadius; x++)
                for (var y = -coverageRadius; y <= coverageRadius; y++)
                {
                    var startLine = DateTime.Now;
                    var vector = tiles.GetVector(stationCoordinates, stationCoordinates.Move(x, y));
                    var calculations = 0;
                    var calc = new PathLossCalculator();
                    for (var i = 2; i < vector.Length; i++)
                    {
                        var c = vector[i];
                        var (xi, yi) = ((int) (c.X - stationCoordinates.X) + coverageRadius, (int) (c.Y - stationCoordinates.Y) + coverageRadius);
                        if (results[xi, yi] != 0) continue;
                        tiles.FillAltitudeVector(vector, i);
                        results[xi, yi] = calc.CalculateLoss(vector, 100, 2, i - 1);
                        calculations++;
                    }

                    var lms = DateTime.Now.Subtract(startLine).TotalMilliseconds;
                    var cprms = calculations / lms;
                    Console.WriteLine($"Vector to ({x}, {y}): {calculations:n0} calculations in {lms:n2} ms ({cprms:n2} c/ms)");
                }

            var ms = DateTime.Now.Subtract(start).TotalMilliseconds;
            Console.WriteLine($"Calculation time: {ms}, {results.Length / ms:n2} c/ms");
        }

        [TestMethod]
        public void Size_050()
        {
            RunTileCache(50);
        }

        [TestMethod]
        public void Size_100()
        {
            RunTileCache(100);
        }

        [TestMethod]
        public void Size_256()
        {
            RunTileCache(256);
        }

        [TestMethod]
        public void Size_512()
        {
            RunTileCache(512);
        }
    }
}
