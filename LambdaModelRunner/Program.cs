using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ConsoleUtilities.ConsoleInfoPanel;
using LambdaModel.Calculations;
using LambdaModel.Terrain;
using no.sintef.SpeedModule.Geometry.SimpleStructures;

namespace LambdaModelRunner
{
    class Program
    {
        async static Task Main(string[] args)
        {
            try
            {
                Console.BufferWidth = Math.Min(Console.LargestWindowWidth, 180);
                Console.WindowWidth = Math.Min(Console.LargestWindowWidth, 180);
                Console.BufferHeight = Math.Min(Console.LargestWindowHeight, 40);
                Console.WindowHeight = Math.Min(Console.LargestWindowHeight, 40);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            using (var cip = new ConsoleInformationPanel("Running road network signal loss calculations"))
            {
                var center = new Point3D(271327, 7040324);
                var radius = 50_000;
                var tileSize = 512;

                var tiles = new TileCache(@"..\..\..\..\Data\Testing\CacheTest", tileSize, cip);

                await tiles.Preload(center, radius);

                var road = new RoadNetworkCalculator(tiles, @"..\..\..\..\Data\RoadNetwork\2021-05-28_smaller.shp", radius, center, cip);

                var start = DateTime.Now;
                var calculations = road.Calculate();
                var secs = DateTime.Now.Subtract(start).TotalSeconds;
                cip.Set("Calculation time", $"{secs:n2} seconds");
                cip.Set("Calculations", calculations);
                cip.Set("Calculations per second", $"{(calculations / secs):n2} c/s");

                start = DateTime.Now;
                road.SaveResults(@"..\..\..\..\Data\RoadNetwork\test-results-huge.shp");
                cip.Set("Saving time", $"{DateTime.Now.Subtract(start).TotalSeconds:n2} seconds.");
            }
        }
    }
}
