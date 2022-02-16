using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using BitMiracle.LibTiff.Classic;
using ConsoleUtilities;
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

            args = new[] {@"..\..\..\..\Data\Configs\gui_test.json"};

            new ConsoleConfigHelper(args)
                .AutoResize()
                .Run("Processing config files - Lambda Model Runner", GeneralConfig.ParseConfigFile)
                .PrintSummary();

            return;

            var tconfig = new GenerateTilesConfig()
            {
                RawDataLocation = @"I:\Jobb\Lambda\Unpacked",
                OutputDirectory = @"I:\Jobb\Lambda\Tiles_512",
                TileSize = 512
            };
            //tconfig.Run();

            foreach (var cacheSize in new[] {300, 500, 1000, 2000, 3000, 5000, 10000})
            foreach (var remove in new[] {100, 200, 500, 1000, 2000})
            {
                if (remove >= cacheSize) continue;

                var config = GeneralConfig.ParseConfigFile(@"..\..\..\..\Data\Configs\gui_test.json");
                config.Terrain.MaxCacheItems = cacheSize;
                config.Terrain.RemoveCacheItemsWhenFull = remove;
                config.PrepareOutputDirectory();
                config.Run();
            }
        }
    }
}
