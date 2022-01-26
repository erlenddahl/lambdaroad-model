using System.Linq;
using LambdaModel.General;

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
            foreach (var v in link.Geometry.Where(p => p.M!=null).Select(p => p.M))
            {
                sum += v.MaxRssi;
                count++;
                if (v.MaxRssi < Min) Min = v.MaxRssi;
                if (v.MaxRssi > Max) Max = v.MaxRssi;
            }

            Average = sum / count;
        }
    }
}