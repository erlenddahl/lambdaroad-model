using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BitMiracle.LibTiff.Classic;
using ConsoleUtilities.ConsoleInfoPanel;
using DotSpatial.Data;
using DotSpatial.Topology;
using LambdaModel.General;
using LambdaModel.Stations;
using LambdaModel.Utilities;
using Newtonsoft.Json.Linq;

namespace LambdaModel.Config
{
    public class RoadNetworkConfig : GeneralConfig
    {
        public string RoadShapeLocation { get; set; }
        public new RoadLinkBaseStation[] BaseStations { get; set; }

        public override void Run()
        {
            using (Cip = new ConsoleInformationPanel("Running road network signal loss calculations"))
            {
                Tiff.SetErrorHandler(new LambdaTiffErrorHandler(Cip));

                foreach (var bs in BaseStations)
                    bs.Cip = Cip;

                var calculations = 0;

                Cip.Set("Minimum signal", MinimumAllowableSignalValue);

                Cip?.Set("Road network source", Path.GetFileName(RoadShapeLocation));
                ShapeLink.ReadLinks(RoadShapeLocation, BaseStations);

                var tiles = Terrain.CreateCache(Cip);

                var start = DateTime.Now;
                foreach (var bs in Cip.Run("Processing base stations", BaseStations))
                {
                    Cip?.Set("Calculation radius", bs.MaxRadius);
                    Cip?.Set("Relevant road links", bs.Links.Count);

                    var maxLoss = bs.TotalTransmissionLevel - MinimumAllowableSignalValue;

                    bs.RemoveLinksTooFarAway(maxLoss);

                    calculations += bs.Calculate(tiles);

                    var secs = DateTime.Now.Subtract(start).TotalSeconds;
                    Cip?.Set("Calculation time", secs);
                    Cip?.Set("Calculations per second", $"{(calculations / secs):n2} c/s");

                    bs.RemoveLinksWithTooMuchPathLoss(maxLoss);
                }

                start = DateTime.Now;

                if (OutputLocation.ToLower().EndsWith(".shp"))
                {
                    SaveShape(OutputLocation, BaseStations.SelectMany(p => p.Links).ToArray(), Cip);
                }
                else
                {
                    if (!Directory.Exists(OutputLocation))
                        Directory.CreateDirectory(OutputLocation);
                    SaveApiResults(OutputLocation, BaseStations.SelectMany(p => p.Links).ToArray(), Cip);
                }

                Cip.Set("Saving time", $"{DateTime.Now.Subtract(start).TotalSeconds:n2} seconds.");

                FinalSnapshot = Cip.GetSnapshot();
            }
        }

        public override GeneralConfig Validate(string configLocation = null)
        {
            if (BaseStations?.Any() != true) throw new ConfigException("No BaseStations defined.");
            if (Terrain == null) throw new ConfigException("Missing Terrain config.");
            foreach (var bs in BaseStations)
                bs.Validate();
            RoadShapeLocation = GetFullPath(configLocation, RoadShapeLocation);
            return base.Validate(configLocation);
        }

        public static void SaveApiResults(string outputDirectory, IList<ShapeLink> links, ConsoleInformationPanel cip = null)
        {
            var metaFile = Path.Combine(outputDirectory, "meta.json");
            File.WriteAllText(metaFile, JArray.FromObject(links.Select(p => new RoadLinkResultMetadata(p))).ToString());

            using (var pb = cip?.SetProgress("Saving results", max: links.Count))
                foreach (var link in links)
                {
                    var linkFile = Path.Combine(outputDirectory, link.ID.ToString() + ".lnkres");
                    using (var writer = new BinaryWriter(File.Create(linkFile)))
                    {
                        writer.Write(link.ID);
                        writer.Write(link.Cx);
                        writer.Write(link.Cy);
                        writer.Write(link.Length);
                        writer.Write(link.Name);
                        writer.Write(link.Geometry.Length);

                        foreach (var c in link.Geometry)
                        {
                            if (double.IsNaN(c.M)) continue;
                            writer.Write(c.X);
                            writer.Write(c.Y);
                            writer.Write(c.Z);
                            writer.Write(c.M);
                        }
                    }

                    pb?.Increment();
                }
        }

        public static void SaveShape(string pathFile, IList<ShapeLink> links, ConsoleInformationPanel cip = null)
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
                shp.SaveAs(pathFile, true);
        }
    }
}