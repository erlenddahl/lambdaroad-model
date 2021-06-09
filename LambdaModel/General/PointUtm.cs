using System;

namespace LambdaModel.General
{
    public class PointUtm
    {
        public double X;
        public double Y;
        public double Z;
        public double M;

        public PointUtm(double x, double y, double z = 0, double m = 0)
        {
            X = x;
            Y = y;
            Z = z;
            M = m;
        }

        public double Distance2d(PointUtm p)
        {
            return Math.Sqrt(Math.Pow(X - p.X, 2) + Math.Pow(Y - p.Y, 2));
        }

        public double Distance3d(PointUtm p)
        {
            return Math.Sqrt(Math.Pow(X - p.X, 2) + Math.Pow(Y - p.Y, 2) + Math.Pow(Z - p.Z, 2));
        }

        public double Distance3d(double x, double y, double z)
        {
            return Math.Sqrt(Math.Pow(X - x, 2) + Math.Pow(Y - y, 2) + Math.Pow(Z - z, 2));
        }

        public override string ToString()
        {
            return $"{X:n3}, {Y:n3}, {Z:n3}";
        }

        public PointUtm Move(double dx = 0, double dy = 0, double dz = 0, double dm = 0)
        {
            return new PointUtm(X + dx, Y + dy, Z + dz, M + dm);
        }
    }
}
