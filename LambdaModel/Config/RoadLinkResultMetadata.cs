using System;
using System.Linq;
using DotSpatial.Topology;
using LambdaModel.General;
using Newtonsoft.Json.Linq;
using no.sintef.SpeedModule.Geometry.SimpleStructures;

namespace LambdaModel.Config
{
    public class RoadLinkResultMetadata
    {
        public int ID { get; set; }
        public int Cx { get; set; }
        public int Cy { get; set; }
        public int Length { get; set; }
        public double Min { get; set; } = double.MaxValue;
        public double Max { get; set; } = double.MinValue;
        public double Average { get; set; }

        public JArray Points { get; set; }

        public RoadLinkResultMetadata()
        {

        }

        public RoadLinkResultMetadata(ShapeLink link)
        {
            ID = link.ID;
            Cx = link.Cx;
            Cy = link.Cy;
            Length = link.Length;

            var sum = 0d;
            var count = 0;
            Points = new JArray();
            foreach (var v in link.Geometry.Where(p => p.M!=null))
            {
                sum += v.M.MaxRssi;
                count++;
                if (v.M.MaxRssi < Min) Min = v.M.MaxRssi;
                if (v.M.MaxRssi > Max) Max = v.M.MaxRssi;

                Points.Add(JArray.FromObject(new[] { v.X, v.Y }
                    .Concat(v.M.BaseStationRssi.Select(c => (double)(int)Math.Round(c)))
                    .ToArray()));
            }

            Average = sum / count;
        }
    }
}