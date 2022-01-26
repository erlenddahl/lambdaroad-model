using System;
using System.Collections.Generic;
using System.Text;
using LambdaModel.Utilities;
using no.sintef.SpeedModule.Geometry.SimpleStructures;

namespace LambdaModel.General
{
    public class Point4D<T> : Point3D
    {
        public T M;
        public int RoundedX;
        public int RoundedY;

        public Point4D(double x, double y, double z = 0, T m = default(T)) : base(x, y, z)
        {
            M = m;
        }
    }
}
