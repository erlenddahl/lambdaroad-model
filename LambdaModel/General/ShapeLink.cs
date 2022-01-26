using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using DotSpatial.Data;
using DotSpatial.Topology;
using LambdaModel.Stations;
using no.sintef.SpeedModule.Geometry;
using no.sintef.SpeedModule.Geometry.SimpleStructures;

namespace LambdaModel.General
{
    public class ShapeLink
    {
        public int ID { get; }
        public string Name { get; set; }
        public int Cx { get; }
        public int Cy { get; }
        public int Length { get; }
        public Point4D<CalculationDetails>[] Geometry { get; }

        private ShapeLink(int id, string name, IEnumerable<Point4D<CalculationDetails>> geometry, int cx, int cy, int length)
        {
            ID = id;
            Name = name;
            Cx = cx;
            Cy = cy;
            Length = length;

            var line = new CachedLineTools(geometry.ToArray());
            Geometry = new Point4D<CalculationDetails>[(int) line.Length + 1];
            for (var i = 0; i < Geometry.Length; i++)
            {
                var pi = line.QueryPointInfo(i);
                Geometry[i] = new Point4D<CalculationDetails>(pi.X, pi.Y, pi.Z);
            }
        }


        /// <summary>
        /// Reads all links that are within the given radius from the center point.
        /// </summary>
        /// <param name="geometryPath"></param>
        /// <param name="stations"></param>
        /// <returns></returns>
        public static void ReadLinks(string geometryPath, IList<RoadLinkBaseStation> stations)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            if (!File.Exists(geometryPath + ".cache"))
                GenerateShapeCache(geometryPath);

            foreach (var bs in stations)
            {
                bs.Links.Clear();
            }

            using (var reader = new BinaryReader(File.OpenRead(geometryPath + ".cache")))
            {
                var fileSize = reader.BaseStream.Length;
                var position = 0;

                while (position < fileSize)
                {
                    var ix = reader.ReadInt32();

                    var cx = reader.ReadInt32();
                    var cy = reader.ReadInt32();
                    var length = reader.ReadInt32();

                    var name = reader.ReadString();

                    var pointCount = reader.ReadInt32();

                    var pointBytes = reader.ReadBytes(pointCount * 24);
                    position += 4 + 4 * 3 + 4 + name.Length + 4 + pointBytes.Length;

                    if (pointCount < 2) continue;

                    ShapeLink link = null;
                    foreach (var bs in stations)
                    {
                        var dist = bs.Center.DistanceTo2D(cx, cy);

                        // If this link is guaranteed to be outside of the station's radius, skip it immediately.
                        if (dist - length > bs.MaxRadius)
                            continue;

                        // Otherwise, parse the link geometry (which is stored outside of the link so that we only
                        // have to do it once, and so that each base stations gets the same link object reference.
                        if (link == null)
                        {
                            var geometry = new Point4D<CalculationDetails>[pointCount];
                            var pix = 0;

                            for (var i = 0; i < pointBytes.Length; i += 24)
                            {
                                var x = BitConverter.ToDouble(pointBytes, i);
                                var y = BitConverter.ToDouble(pointBytes, i + 8);
                                var z = BitConverter.ToDouble(pointBytes, i + 16);
                                geometry[pix++] = new Point4D<CalculationDetails>(x, y, z);
                            }

                            link = new ShapeLink(ix, name, geometry, cx, cy, length);
                        }

                        // Check if any points on the link is actually inside of the radius.
                        foreach (var g in link.Geometry)
                        {
                            dist = g.DistanceTo2D(bs.Center);
                            if (dist <= bs.MaxRadius)
                            {
                                // If so, add the link, and break this loop.
                                bs.Links.Add(link);
                                break;
                            }
                        }

                    }
                }
            }
        }

        private static void GenerateShapeCache(string geometryPath)
        {
            using(var writer = new BinaryWriter(File.Create(geometryPath + ".cache")))
            using (var fs = FeatureSet.Open(geometryPath))
            {
                //Note: using foreach() on fs.Features loads the entire file to memory. Slow and OOM.
                for (var i = 0; i < fs.NumRows(); i++)
                {
                    writer.Write(i);

                    var feature = fs.GetFeature(i);
                    // To read more features, do this: x = (int)feature.DataRow["A"];

                    var ec = feature.BasicGeometry.Envelope.Center();
                    writer.Write((int)ec.X);
                    writer.Write((int)ec.Y);
                    writer.Write((int)CalculateLength(feature.BasicGeometry.Coordinates));

                    var id = $"{feature.DataRow["FROM_M"]}-{feature.DataRow["TO_M"]}@{feature.DataRow["ROUTEID"]}";
                    writer.Write(id);

                    writer.Write(feature.BasicGeometry.Coordinates.Count);

                    foreach (var c in feature.BasicGeometry.Coordinates)
                    {
                        writer.Write(c.X);
                        writer.Write(c.Y);
                        writer.Write(c.Z);
                    }
                }
            }
        }

        private static double CalculateLength(IList<Coordinate> coordinates)
        {
            var l = 0d;
            for (var i = 1; i < coordinates.Count; i++)
                l += coordinates[i - 1].Distance(coordinates[i]);
            return l;
        }
    }
}
