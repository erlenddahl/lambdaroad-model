﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using BitMiracle.LibTiff.Classic;
using ConsoleUtilities.ConsoleInfoPanel;
using DotSpatial.Data;
using DotSpatial.Topology;
using DotSpatial.Topology.Operation.Distance;
using Extensions.StringExtensions;
using LambdaModel.General;
using LambdaModel.PathLoss;
using LambdaModel.Stations;
using LambdaModel.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LambdaModel.Config
{
    public class RoadNetworkConfig : GeneralConfig
    {
        public string RoadShapeLocation { get; set; }
        public new RoadLinkBaseStation[] BaseStations { get; set; }
        public int LinkCalculationPointFrequency { get; set; } = 20;
        public string ShapeFileName { get; set; } = "results.shp";
        public string CsvFileName { get; set; } = "results.csv";
        public string ApiResultInnerFolderName { get; set; } = "links";
        public string CsvSeparator { get; set; } = ";";
        public bool WriteShape { get; set; } = true;
        public bool WriteCsv { get; set; } = true;
        public bool WriteApiResults { get; set; } = true;
        public bool IncludeEmptyPoints { get; set; } = false;

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
                    if (bs.Calculator is MobileNetworkPathLossCalculator m && MobileRegression.HasValue)
                    {
                        m.RegressionType = MobileRegression.Value;
                        Cip.Set("Regression type", MobileRegression.Value.ToString());
                    }
                }

                if (CalculationThreads.HasValue)
                    Cip.Set("Calculation threads", CalculationThreads.Value);
                Cip.Set("Minimum RSRP", MinimumAllowableRsrp);
                Cip.Set("Calc point frequency", LinkCalculationPointFrequency);
                Cip.Set("Receiver height", ReceiverHeightAboveTerrain);

                Cip?.Set("Road network source", Path.GetFileName(RoadShapeLocation));
                ShapeLink.ReadLinks(RoadShapeLocation, BaseStations, Cip);

                var calculations = 0L;
                var distance = 0L;

                Cancellor = new CancellationTokenSource();

                var start = DateTime.Now;
                using (var pb = Cip.SetProgress("Processing base stations", max: BaseStations.Length))
                {
                    var query = BaseStations.AsParallel();
                    
                    if (CalculationThreads.HasValue)
                        query = query.WithDegreeOfParallelism(CalculationThreads.Value);

                    query.ForAll(bs =>
                    {
                        Cip?.Set("Calculation radius", bs.MaxRadius);
                        Cip?.Set("Relevant road links", bs.Links.Count);

                        if (bs.AntennaType != AntennaType.Los)
                        {
                            bs.RemoveLinksTooFarAway(MinimumAllowableRsrp, ReceiverHeightAboveTerrain);
                        }

                        using (var tiles = Terrain.CreateCache(Cip))
                        {
                            var (bsCalcs, bsDist) = bs.Calculate(tiles, LinkCalculationPointFrequency, ReceiverHeightAboveTerrain, Cancellor.Token, BaseStations.Length, bs.BaseStationIndex);

                            Interlocked.Add(ref calculations, bsCalcs);
                            Interlocked.Add(ref distance, bsDist);

                            Cip?.Increment("Seconds lost to cache removal", tiles.SecondsLostToRemovals);
                        }

                        var secs = DateTime.Now.Subtract(start).TotalSeconds;
                        Cip?.Set("Calculation time", secs);
                        Cip?.Set("Calculations per second", $"{(long)(calculations / secs):n0} c/s");
                        Cip?.Set("Terrain lookups per second", $"{(long)(distance / secs):n0} tl/s");

                        if (bs.AntennaType != AntennaType.Los)
                        {
                            bs.RemoveLinksWithTooLowRsrp(MinimumAllowableRsrp);
                        }

                        pb.Increment();
                    });
                }

                start = DateTime.Now;
                
                if (WriteShape)
                    SaveShape(Path.Combine(OutputDirectory, ShapeFileName), BaseStations, BaseStations.SelectMany(p => p.Links).Distinct().ToArray(), Cip);
                if (WriteApiResults)
                {
                    var dir = OutputDirectory;
                    if (!string.IsNullOrWhiteSpace(ApiResultInnerFolderName))
                        dir = Path.Combine(dir, ApiResultInnerFolderName);
                    Directory.CreateDirectory(dir);
                    SaveApiResults(dir, BaseStations, Cip);
                }

                if (WriteCsv)
                    SaveCsv(Path.Combine(OutputDirectory, CsvFileName), CsvSeparator, BaseStations, BaseStations.SelectMany(p => p.Links).Distinct().ToArray(), IncludeEmptyPoints, Cip);

                Cip.Set("Saving time", $"{DateTime.Now.Subtract(start).TotalSeconds:n2} seconds.");

                FinalSnapshot = Cip.GetSnapshot();

                if (WriteLog)
                    File.WriteAllText(Path.Combine(OutputDirectory, LogFileName), JObject.FromObject(FinalSnapshot).ToString());
            }
        }

        public override GeneralConfig Validate(string configLocation = null)
        {
            if (WriteShape && ShapeFileName.Contains("\\")) throw new ConfigException("ShapeFileName must be a file name only, not a path.");
            if (LinkCalculationPointFrequency < 1) throw new ConfigException("Link calculation point frequency must be at least 1.");
            if (BaseStations?.Any() != true) throw new ConfigException("No BaseStations defined.");
            if (BaseStations.GroupBy(p => p.Id).Any(c => c.Count() > 1)) throw new ConfigException("BaseStation Ids must be unique");
            if (Terrain == null) throw new ConfigException("Missing Terrain config.");
            foreach (var bs in BaseStations)
            {
                bs.Validate();
                bs.Initialize();
            }

            RoadShapeLocation = GetFullPath(configLocation, RoadShapeLocation);
            if (LinkCalculationPointFrequency < 1) throw new Exception("Link calculation point frequency must be at least 1.");
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
            }).ToString(Formatting.None));

            // Not used for anything; disabled for now.
            /*using (var pb = cip?.SetProgress("Saving results", max: links.Length))
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
                            writer.Write(c.M.MaxRsrp);
                            writer.Write(c.M.BaseStationRsrp.Length);
                            foreach (var bs in c.M.BaseStationRsrp)
                                writer.Write(bs);
                        }
                    }

                    pb?.Increment();
                }*/
        }

        public static void SaveShape(string pathFile, IList<RoadLinkBaseStation> baseStations, IList<ShapeLink> links, ConsoleInformationPanel cip = null)
        {
            var shp = new FeatureSet(FeatureType.Point);

            var table = shp.DataTable;
            table.Columns.Add("RoadLinkId", typeof(string));
            table.Columns.Add("Max RSRP", typeof(double));

            foreach (var bs in baseStations)
                table.Columns.Add(bs.Id, typeof(double));

            table.AcceptChanges();

            using (var pb = cip?.SetProgress("Saving results", max: links.Count))
                foreach (var link in links)
                {
                    foreach (var c in link.Geometry)
                    {
                        if (c.M == null) continue;
                        var feature = shp.AddFeature(new Point(new Coordinate(c.X, c.Y, c.Z)));
                        feature.DataRow["Max RSRP"] = c.M.MaxRsrp;
                        feature.DataRow["RoadLinkId"] = link.Name;

                        foreach (var bs in baseStations)
                            feature.DataRow[bs.Id] = c.M.BaseStationRsrp[bs.BaseStationIndex];
                    }

                    pb?.Increment();
                }

            using (var _ = cip?.SetUnknownProgress("Writing shape"))
                shp.SaveAs(pathFile, true);

            using (var _ = cip?.SetUnknownProgress("Zipping shape"))
            {
                using (var zipStream = File.Create(Path.Combine(pathFile.ChangeExtension(".zip"))))
                using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                    foreach (var file in new[] {pathFile, pathFile.ChangeExtension(".dbf"), pathFile.ChangeExtension(".shx")})
                    {
                        using (var entryStream = archive.CreateEntry(Path.GetFileName(file)).Open())
                        using (var fileStream = File.OpenRead(file))
                        {
                            fileStream.CopyTo(entryStream);
                        }
                    }
            }
        }

        public static void SaveCsv(string pathFile, string separator, IList<RoadLinkBaseStation> baseStations, IList<ShapeLink> links, bool includeEmptyPoints, ConsoleInformationPanel cip = null)
        {
            var ci = CultureInfo.InvariantCulture;
            using (var results = new StreamWriter(File.Create(pathFile)))
            {
                results.WriteLine(string.Join(separator, "RoadLinkId", "X", "Y", "Z", "Max RSRP") + separator + string.Join(separator, baseStations.Select(p => p.Id)));

                using (var pb = cip?.SetProgress("Writing CSV", max: links.Count))
                    foreach (var link in links)
                    {
                        foreach (var c in link.Geometry)
                        {
                            if (!includeEmptyPoints && c.M == null) continue;
                            results.WriteLine(link.Name + separator +
                                              c.X.ToString(ci) + separator +
                                              c.Y.ToString(ci) + separator +
                                              c.Z.ToString(ci) + separator +
                                              (c.M == null ? "0" : c.M.MaxRsrp.ToString(ci)) + separator +
                                              string.Join(separator, c.M == null ? baseStations.Select(p => "0") : c.M.BaseStationRsrp.Select(p => p.ToString(ci))));
                        }

                        pb?.Increment();
                    }
            }
        }
    }
}