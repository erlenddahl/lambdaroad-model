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

            using (var cip = new ConsoleInformationPanel("Running road network signal loss calculations"))
            {
                Tiff.SetErrorHandler(new LambdaTiffErrorHandler(cip));

                var stations = new[]
                {
                    new RoadLinkBaseStation(263062, 7041212, 100, 50_000, cip),
                    new RoadLinkBaseStation(271327, 7040324, 50, 50_000, cip)
                };

                //new TileGenerator(@"I:\Jobb\Lambda\Unpacked", @"I:\Jobb\Lambda\Tiles_512", 512, cip).Generate();
                //new TileGenerator(@"I:\Jobb\Lambda\Unpacked\1213-13", @"I:\Jobb\Lambda\Tiles_512", 256, cip).Generate();

                //TODO: Solve repeating progress bars and info (both here and inside bs.Calculate)

                var tileSize = 512;
                var minAllowableValue = -150;

                var roadShapeLocation = @"..\..\..\..\Data\RoadNetwork\2021-05-28_smaller.shp";
                cip?.Set("Road network source", System.IO.Path.GetFileName(roadShapeLocation));
                ShapeLink.ReadLinks(roadShapeLocation, stations);

                var tiles = new LocalTileCache(@"I:\Jobb\Lambda\Tiles_" + tileSize, tileSize, cip, 300, 100);

                var start = DateTime.Now;
                foreach (var bs in stations)
                {
                    cip?.Set("Calculation radius", bs.MaxRadius);
                    cip?.Set("Relevant road links", bs.Links.Count);

                    var maxLoss = bs.TotalTransmissionLevel - minAllowableValue;

                    bs.RemoveLinksTooFarAway(maxLoss);

                    var calculations = bs.Calculate(tiles);
                    var secs = DateTime.Now.Subtract(start).TotalSeconds;
                    
                    cip?.Set("Calculation time", $"{secs:n2} seconds");
                    cip?.Set("Calculations", calculations);
                    cip?.Set("Calculations per second", $"{(calculations / secs):n2} c/s");

                    bs.RemoveLinksWithTooMuchPathLoss(maxLoss);
                }

                start = DateTime.Now;
                SaveResults(@"..\..\..\..\Data\RoadNetwork\test-results-huge.shp", stations.SelectMany(p => p.Links).ToArray(), cip);
                cip.Set("Saving time", $"{DateTime.Now.Subtract(start).TotalSeconds:n2} seconds.");
            }
        }



        public static void SaveResults(string outputLocation, IList<ShapeLink> links, ConsoleInformationPanel cip = null)
        {
            var shp = new FeatureSet(FeatureType.Point);

            var table = shp.DataTable;
            table.Columns.Add("Loss", typeof(double));
            table.Columns.Add("RoadLinkId", typeof(string));
            table.AcceptChanges();

            using (var pb = cip?.SetProgress("Saving results", max: links.Count))
                foreach (var link in links)
                {
                    foreach (var c in link.Geometry)
                    {
                        if (double.IsNaN(c.M)) continue;
                        var feature = shp.AddFeature(new Point(new Coordinate(c.X, c.Y, c.Z)));
                        feature.DataRow["Loss"] = c.M;
                        feature.DataRow["RoadLinkId"] = link.Name;
                    }

                    pb?.Increment();
                }

            using (var _ = cip?.SetUnknownProgress("Writing shape"))
                shp.SaveAs(outputLocation, true);
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
