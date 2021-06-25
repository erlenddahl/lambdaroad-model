using System;
using System.Collections.Generic;
using System.Text;
using no.sintef.SpeedModule.Geometry.SimpleStructures;

namespace LambdaModel.Utilities
{
    public static class Point3DExtensions
    {
        public static Point3D Move(this Point3D p, double dx = 0, double dy = 0, double dz = 0)
        {
            return new Point3D(p.X + dx, p.Y + dy, p.Z + dz);
        }
    }
}
