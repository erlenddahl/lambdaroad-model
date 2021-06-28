using System;
using System.Collections.Generic;
using System.Text;
using no.sintef.SpeedModule.Geometry.SimpleStructures;

namespace LambdaModel.General
{
    public class Point4D : Point3D
    {
        public double M { get; set; }
        public double? CumulativeDistance { get; set; }

        public Point4D(double x, double y, double z = 0, double m = 0) : base(x, y, z)
        {
            M = m;
        }
    }
}
