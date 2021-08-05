using System;
using System.Collections.Generic;
using System.Linq;
using BitMiracle.LibTiff.Classic;
using ConsoleUtilities.ConsoleInfoPanel;
using DotSpatial.Data;
using DotSpatial.Topology;
using LambdaModel.General;
using LambdaModel.Stations;
using LambdaModel.Utilities;

namespace LambdaModel.Config
{
    public class RoadNetworkConfig : GeneralConfig
    {
        public string RoadShapeLocation { get; set; }
        public new RoadLinkBaseStation[] BaseStations { get; set; }

        public override void Run()
        {
            using (var cip = new ConsoleInformationPanel("Running road network signal loss calculations"))
            {
                Tiff.SetErrorHandler(new LambdaTiffErrorHandler(cip));

                foreach (var bs in BaseStations)
                    bs.Cip = cip;

                var calculations = 0;

                cip?.Set("Road network source", System.IO.Path.GetFileName(RoadShapeLocation));
                ShapeLink.ReadLinks(RoadShapeLocation, BaseStations);

                var tiles = Terrain.CreateCache(cip);

                var start = DateTime.Now;
                foreach (var bs in cip.Run("Processing base stations", BaseStations))
                {
                    cip?.Set("Calculation radius", bs.MaxRadius);
                    cip?.Set("Relevant road links", bs.Links.Count);

                    var maxLoss = bs.TotalTransmissionLevel - MinimumAllowableSignalValue;

                    bs.RemoveLinksTooFarAway(maxLoss);

                    calculations += bs.Calculate(tiles);

                    var secs = DateTime.Now.Subtract(start).TotalSeconds;
                    cip?.Increment("Calculation time", secs);
                    cip?.Set("Calculations per second", $"{(calculations / secs):n2} c/s");

                    bs.RemoveLinksWithTooMuchPathLoss(maxLoss);
                }

                start = DateTime.Now;
                SaveResults(OutputLocation, BaseStations.SelectMany(p => p.Links).ToArray(), cip);
                cip.Set("Saving time", $"{DateTime.Now.Subtract(start).TotalSeconds:n2} seconds.");
            }
        }

        protected override GeneralConfig Validate(string configLocation = null)
        {
            if (BaseStations?.Any() != true) throw new ConfigException("No BaseStations defined.");
            if (Terrain == null) throw new ConfigException("Missing Terrain config.");
            RoadShapeLocation = GetFullPath(configLocation, RoadShapeLocation);
            return base.Validate();
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
}