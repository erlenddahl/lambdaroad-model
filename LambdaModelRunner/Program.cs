using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using BitMiracle.LibTiff.Classic;
using ConsoleUtilities.ConsoleInfoPanel;
using DotSpatial.Data;
using DotSpatial.Topology;
using LambdaModel.Calculations;
using LambdaModel.Config;
using LambdaModel.General;
using LambdaModel.Stations;
using LambdaModel.Terrain;
using LambdaModel.Terrain.Cache;
using LambdaModel.Terrain.Tiff;
using LambdaModel.Utilities;
using no.sintef.SpeedModule.Geometry.SimpleStructures;

namespace LambdaModelRunner
{
    class Program
    {
        async static Task Main(string[] args)
        {
            /*
             * TODOS:
             * Eliminate further by looking at fresnel obstructions and eliminating once we know path loss gets too big
             */

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

            var tconfig = new GenerateTilesConfig()
            {
                RawDataLocation = @"I:\Jobb\Lambda\Unpacked",
                OutputDirectory = @"I:\Jobb\Lambda\Tiles_512",
                TileSize = 512
            };
            //tconfig.Run();

            var config = GeneralConfig.ParseConfigFile(@"..\..\..\..\Data\Configs\basic_road_network_test.json");
            config.Run();
        }
    }
}
