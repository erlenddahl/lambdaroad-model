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
        public Point4D[] Geometry { get; }

        private ShapeLink(int id, IEnumerable<Point4D> geometry)
        {
            ID = id;
            Geometry = geometry.ToArray();
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

                    var dx = reader.ReadInt32() - cc.X;
                    var dy = reader.ReadInt32() - cc.Y;
                    var length = reader.ReadInt32();

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

                    if (anyInside)
                        yield return new ShapeLink(ix, geometry);
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
