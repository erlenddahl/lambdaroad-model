using System;
using LambdaModel.General;
using LambdaModel.PathLoss;
using LambdaModel.Terrain;
using LambdaModel.Terrain.Cache;
using no.sintef.SpeedModule.Geometry.SimpleStructures;

namespace LambdaModel.Calculations
{
    public class GridCalculator
    {
        private readonly Point4D<double>[] _vector;
        private readonly MobileNetworkPathLossCalculator _calc;

        public OnlineTileCache Tiles { get; }
        public int Radius { get; }
        public Point3D Center { get; }
        public double[,] Results { get; }

        public GridCalculator(OnlineTileCache tiles, int radius, Point3D center)
        {
            Tiles = tiles;
            Radius = radius;
            Center = center;

            center.Z = tiles.GetAltitude(center);

            // Initialize a PointUtm array that is to be (re)used as the vector of points from
            // the center to each of the points that should be calculated.
            _vector = new Point4D<double>[(int)Math.Sqrt(radius * radius * 2) + 1];
            for (var i = 0; i < _vector.Length; i++)
                _vector[i] = new Point4D<double>(0, 0);

            Results = new double[radius * 2 + 1, radius * 2 + 1];
            _calc = new MobileNetworkPathLossCalculator();
        }

        /// <summary>
        /// Calculates path loss for all points along the line from the center to the given coordinates.
        /// Skips any points along the line that has already been calculated.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>The number of new calculations that were made.</returns>
        public int CalculateTo(int x, int y)
        {
            // Get the X,Y vector from the center to these coordinates.
            var vectorLength = Tiles.FillVector(_vector, Center.X, Center.Y, Center.X + x, Center.Y + y);
            var calculations = 0;

            for (var i = 2; i < vectorLength; i++)
            {
                // Get the current receiver point
                var c = _vector[i];

                // Calculate the location of the results for this point in the results matrix
                var (xi, yi) = ((int)(c.X - Center.X) + Radius, (int)(c.Y - Center.Y) + Radius);

                // If this point has already been calculated, get out of here.
                if (Results[xi, yi] != 0) continue;

                // Otherwise, make sure the Z values in the vector are retrieved up until this point.
                Tiles.FillAltitudeVector(_vector, i);

                // Calculate the loss for this point, and store it in the results matrix
                Results[xi, yi] = _calc.CalculateLoss(_vector, 100, 2, i - 1);

                calculations++;
            }

            return calculations;
        }
    }
}
