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

        public Point4D<T> Offset(int x, int y, double z)
        {
            return new Point4D<T>(X + x, Y + y, Z + z, M)
            {
                RoundedX = RoundedX + x,
                RoundedY = RoundedY + y,
            };
        }
    }
}
