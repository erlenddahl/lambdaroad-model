using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using DotSpatial.Data;
using DotSpatial.Topology;
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
        public Point4D[] Geometry { get; }

        private ShapeLink(int id, string name, IEnumerable<Point4D> geometry, int cx, int cy, int length)
        {
            ID = id;
            Name = name;
            Cx = cx;
            Cy = cy;
            Length = length;

            var line = new CachedLineTools(geometry.ToArray());
            Geometry = new Point4D[(int) line.Length + 1];
            for (var i = 0; i < line.Length; i++)
            {
                var pi = line.QueryPointInfo(i);
                Geometry[i] = new Point4D(pi.X, pi.Y, pi.Z);
            }
        }


        /// <summary>
        /// Reads all links that are within the given radius from the center point.
        /// </summary>
        /// <param name="geometryPath"></param>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static IEnumerable<ShapeLink> ReadLinks(string geometryPath, Point3D center, double radius)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var cc = new Coordinate(center.X, center.Y);
            if (!File.Exists(geometryPath + ".cache"))
                GenerateShapeCache(geometryPath);

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

                    var dx = cx - cc.X;
                    var dy = cy - cc.Y;

                    var name = reader.ReadString();

                    var pointCount = reader.ReadInt32();

                    var pointBytes = reader.ReadBytes(pointCount * 24);
                    position += 4 + 4 * 3 + 4 + name.Length + 4 + pointBytes.Length;

                    if (Math.Sqrt(dx * dx + dy * dy) - length > radius)
                        continue;

                    var geometry = new Point4D[pointCount];
                    var pix = 0;
                    var anyInside = false;
                    for (var i = 0; i < pointBytes.Length; i += 24)
                    {
                        var x = BitConverter.ToDouble(pointBytes, i);
                        var y = BitConverter.ToDouble(pointBytes, i + 8);
                        var z = BitConverter.ToDouble(pointBytes, i + 16);
                        geometry[pix++] = new Point4D(x, y, z);

                        if (!anyInside)
                        {
                            dx = x - cc.X;
                            dy = y - cc.Y;
                            if (Math.Sqrt(dx * dx + dy * dy) <= radius)
                                anyInside = true;
                        }
                    }

                    if (anyInside && length > 1)
                        yield return new ShapeLink(ix, name, geometry, cx, cy, length);
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
