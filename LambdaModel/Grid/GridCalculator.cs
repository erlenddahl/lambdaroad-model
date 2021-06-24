using System;
using System.Collections.Generic;
using System.Text;
using LambdaModel.General;
using LambdaModel.PathLoss;
using LambdaModel.Terrain;

namespace LambdaModel.Grid
{
    public class GridCalculator
    {
        private readonly PointUtm[] _vector;
        private readonly PathLossCalculator _calc;

        public TileCache Tiles { get; }
        public int Radius { get; }
        public PointUtm Center { get; }
        public double[,] Results { get; }

        public GridCalculator(TileCache tiles, int radius, PointUtm center)
        {
            Tiles = tiles;
            Radius = radius;
            Center = center;

            center.Z = tiles.GetAltitude(center);

            _vector = new PointUtm[(int)Math.Sqrt(radius * radius * 2) + 1];
            for (var i = 0; i < _vector.Length; i++)
                _vector[i] = new PointUtm(0, 0);

            Results = new double[radius * 2 + 1, radius * 2 + 1];
            _calc = new PathLossCalculator();
        }

        public int CalculateTo(int x, int y)
        {
            var vectorLength = Tiles.FillVector(_vector, Center.X, Center.Y, Center.X + x, Center.Y + y);
            var calculations = 0;
            for (var i = 2; i < vectorLength; i++)
            {
                var c = _vector[i];
                var (xi, yi) = ((int)(c.X - Center.X) + Radius, (int)(c.Y - Center.Y) + Radius);
                if (Results[xi, yi] != 0) continue;
                Tiles.FillAltitudeVector(_vector, i);
                Results[xi, yi] = _calc.CalculateLoss(_vector, 100, 2, i - 1);
                calculations++;
            }

            return calculations;
        }
    }
}
