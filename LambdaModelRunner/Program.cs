﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using BitMiracle.LibTiff.Classic;
using ConsoleUtilities.ConsoleInfoPanel;
using LambdaModel.Calculations;
using LambdaModel.Terrain;
using LambdaModel.Terrain.Tiff;
using no.sintef.SpeedModule.Geometry.SimpleStructures;

namespace LambdaModelRunner
{
    class Program
    {
        async static Task Main(string[] args)
        {
            /*
             * TODOS:
             * Order road links by angle in order to use spiral sequence
             * Then by distance from center?
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

            using (var cip = new ConsoleInformationPanel("Running road network signal loss calculations"))
            {
                Tiff.SetErrorHandler(new LambdaTiffErrorHandler(cip));

                var center = new Point3D(271327, 7040324);
                var radius = 50_000;
                var tileSize = 512;
                var txHeightAboveTerrain = 100;

                var tiles = new TileCache(@"..\..\..\..\Data\Testing\CacheTest", tileSize, cip);

                var road = new RoadNetworkCalculator(tiles, @"..\..\..\..\Data\RoadNetwork\2021-05-28_smaller.shp", radius, center, txHeightAboveTerrain, cip);
                road.RemoveLinksTooFarAway(200);

                //TODO: Preload with new radius (no need to download outside of links)
                await tiles.Preload(center, radius);

                var start = DateTime.Now;
                var calculations = road.Calculate();
                var secs = DateTime.Now.Subtract(start).TotalSeconds;
                cip.Set("Calculation time", $"{secs:n2} seconds");
                cip.Set("Calculations", calculations);
                cip.Set("Calculations per second", $"{(calculations / secs):n2} c/s");

                start = DateTime.Now;
                road.SaveResults(@"..\..\..\..\Data\RoadNetwork\test-results-huge-2.shp");
                cip.Set("Saving time", $"{DateTime.Now.Subtract(start).TotalSeconds:n2} seconds.");
            }

            Console.ReadLine();
        }
    }

    internal class LambdaTiffErrorHandler : TiffErrorHandler
    {
        private readonly ConsoleInformationPanel _cip;

        private AppendableStringInfoItem _log = null;

        public LambdaTiffErrorHandler(ConsoleInformationPanel cip)
        {
            _cip = cip;
        }

        private void Log(string text)
        {
            return;
            if (_log == null)
            {
                _log = _cip.Log("Tiff parsing", text, sequence: 999);
            }
            else
            {
                _log.AppendLine(text);
            }
        }

        public override void ErrorHandler(Tiff tif, string method, string format, params object[] args)
        {
            //Log("ERROR: " + string.Format(format, args) + " (" + method + ", " + System.IO.Path.GetFileName(tif.FileName()) + ")");
        }

        public override void ErrorHandlerExt(Tiff tif, object clientData, string method, string format, params object[] args)
        {
            Log("ERROR: " + string.Format(format, args) + " (" + method + ", " + System.IO.Path.GetFileName(tif.FileName()) + ")");
        }

        public override void WarningHandler(Tiff tif, string method, string format, params object[] args)
        {
            //Log("WARNING: " + string.Format(format, args) + " (" + method + ", " + System.IO.Path.GetFileName(tif.FileName()) + ")");
        }

        public override void WarningHandlerExt(Tiff tif, object clientData, string method, string format, params object[] args)
        {
            Log("WARNING: " + string.Format(format, args) + " (" + method + ", " + System.IO.Path.GetFileName(tif.FileName()) + ")");
        }
    }
}
