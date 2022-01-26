using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using BitMiracle.LibTiff.Classic;
using ConsoleUtilities.ConsoleInfoPanel;
using DotSpatial.Data;
using DotSpatial.Topology;
using Extensions.StringExtensions;
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

                var bix = 0;
                foreach (var bs in BaseStations)
                {
                    bs.Cip = Cip;
                    bs.BaseStationIndex = bix++;
                }

                var calculations = 0;

                if (CalculationThreads.HasValue)
                    Cip.Set("Calculation threads", CalculationThreads.Value);
                Cip.Set("Minimum signal", MinimumAllowableSignalValue);

                Cip?.Set("Road network source", Path.GetFileName(RoadShapeLocation));
                ShapeLink.ReadLinks(RoadShapeLocation, BaseStations);

                var start = DateTime.Now;
                using (var pb = Cip.SetProgress("Processing base stations", max: BaseStations.Length))
                {
                    var query = BaseStations.AsParallel();
                    
                    if (CalculationThreads.HasValue)
                        query = query.WithDegreeOfParallelism(CalculationThreads.Value);

                    query.ForAll(bs =>
                    {
                        var tiles = Terrain.CreateCache(Cip);

                        Cip?.Set("Calculation radius", bs.MaxRadius);
                        Cip?.Set("Relevant road links", bs.Links.Count);
                        
                        bs.RemoveLinksTooFarAway(bs.TotalTransmissionLevel - MinimumAllowableSignalValue);

                        calculations += bs.Calculate(tiles, BaseStations.Length, bs.BaseStationIndex);

                        var secs = DateTime.Now.Subtract(start).TotalSeconds;
                        Cip?.Set("Calculation time", secs);
                        Cip?.Set("Calculations per second", $"{(calculations / secs):n2} c/s");

                        bs.RemoveLinksWithTooLowRssi(MinimumAllowableSignalValue);

                        pb.Increment();
                    });
                }

                start = DateTime.Now;

                if (WriteShape)
                    SaveShape(Path.Combine(OutputDirectory, ShapeFileName), BaseStations, BaseStations.SelectMany(p => p.Links).Distinct().ToArray(), Cip);
                if (WriteApiResults)
                    SaveApiResults(OutputDirectory, BaseStations, Cip);
                if (WriteCsv)
                    SaveCsv(Path.Combine(OutputDirectory, CsvFileName), CsvSeparator, BaseStations, BaseStations.SelectMany(p => p.Links).Distinct().ToArray(), Cip);

                Cip.Set("Saving time", $"{DateTime.Now.Subtract(start).TotalSeconds:n2} seconds.");

                FinalSnapshot = Cip.GetSnapshot();

                if (WriteLog)
                    File.WriteAllText(Path.Combine(OutputDirectory, LogFileName), JObject.FromObject(FinalSnapshot).ToString());
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

        public static void SaveApiResults(string outputDirectory, RoadLinkBaseStation[] baseStations, ConsoleInformationPanel cip = null)
        {
            var metaFile = Path.Combine(outputDirectory, "meta.json");
            var links = baseStations.SelectMany(p => p.Links).Distinct().ToArray();
            File.WriteAllText(metaFile, JObject.FromObject(new
            {
                baseStations,
                links = JArray.FromObject(links.Select(p => new RoadLinkResultMetadata(p)))
            }).ToString());

            using (var pb = cip?.SetProgress("Saving results", max: links.Length))
                foreach (var link in links)
                {
                    var linkFile = Path.Combine(outputDirectory, link.ID + ".lnkres");
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
                            if (c.M == null) continue;
                            writer.Write(c.X);
                            writer.Write(c.Y);
                            writer.Write(c.Z);
                            writer.Write(c.M);
                        }
                    }

                    pb?.Increment();
                }
        }

        public static void SaveShape(string pathFile, IList<RoadLinkBaseStation> baseStations, IList<ShapeLink> links, ConsoleInformationPanel cip = null)
        {
            var shp = new FeatureSet(FeatureType.Point);

            var table = shp.DataTable;
            table.Columns.Add("RoadLinkId", typeof(string));
            table.Columns.Add("Max RSSI", typeof(double));

            foreach (var bs in baseStations)
                table.Columns.Add("RSSI_" + bs.Name, typeof(double));

            table.AcceptChanges();

            using (var pb = cip?.SetProgress("Saving results", max: links.Count))
                foreach (var link in links)
                {
                    foreach (var c in link.Geometry)
                    {
                        if (c.M == null) continue;
                        var feature = shp.AddFeature(new Point(new Coordinate(c.X, c.Y, c.Z)));
                        feature.DataRow["Max RSSI"] = c.M.MaxRssi;
                        feature.DataRow["RoadLinkId"] = link.Name;

                        foreach (var bs in baseStations)
                            feature.DataRow["RSSI_" + bs.Name] = c.M.BaseStationRssi[bs.BaseStationIndex];
                    }

                    pb?.Increment();
                }

            using (var _ = cip?.SetUnknownProgress("Writing shape"))
                shp.SaveAs(pathFile, true);
        }
    }
}