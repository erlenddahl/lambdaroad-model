using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using LambdaModel.Calculations;
using LambdaModel.General;
using LambdaModel.PathLoss;
using LambdaModel.Terrain;
using LambdaModel.Terrain.Tiff;
using LambdaModel.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using no.sintef.SpeedModule.Geometry.SimpleStructures;

namespace LambdaModel.Tests.FullRun.Grid
{
    [TestClass]
    public class SpiralPatternTileCacheTests
    {
        public void RunTileCache(int tileSize)
        {
            var tiles = new TileCache(@"..\..\..\..\Data\Testing\CacheTest", tileSize);
            var grid = new GridCalculator(tiles, 1500, new Point3D(299430, 7108499));

            var start = DateTime.Now;

            foreach (var (x, y) in SpiralGridEnumerator.Enumerate(grid.Radius))
            {
                var startLine = DateTime.Now;
                var calculations = grid.CalculateTo(x, y);
                var lms = DateTime.Now.Subtract(startLine).TotalMilliseconds;
                var cprms = calculations / lms;
                Console.WriteLine($"Vector to ({x}, {y}): {calculations:n0} calculations in {lms:n2} ms ({cprms:n2} c/ms)");
            }

            var ms = DateTime.Now.Subtract(start).TotalMilliseconds;
            Console.WriteLine($"Calculation time: {ms}, {grid.Results.Length / ms:n2} c/ms");
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

        [TestMethod]
        public void Size_1000()
        {
            RunTileCache(1000);
        }
    }
}
