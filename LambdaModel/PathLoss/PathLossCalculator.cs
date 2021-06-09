using System;
using System.Linq;
using LambdaModel.General;

namespace LambdaModel.PathLoss
{
    public class PathLossCalculator
    {
        public double CalculateLoss(PointUtm[] path, double txHeightAboveTerrain, double rxHeightAboveTerrain, int rxIndex = -1)
        {
            var p = GetParameters(path, rxIndex);

            return 25.1 * Math.Log(p.horizontalDistance) - 1.8e-01 * txHeightAboveTerrain + 1.3e+01 * p.rxa - 1.4e-04 * p.txa - 1.4e-04 * p.rxi - 3.0e-05 * p.txi + 4.9 * p.nobs + 29.3;
        }

        protected (double horizontalDistance, double txa, double rxa, double txi, double rxi, int nobs) GetParameters(PointUtm[] path, int rxIndex = -1)
        {
            if (rxIndex == -1) rxIndex = path.Length - 1;
            var tx = path[0];
            var rx = path[rxIndex];

            var horizontalDistance = tx.Distance2d(rx);

            var rxa = 0d;
            var txa = 0d;
            var rxi = 0d;
            var txi = 0d;
            var nobs = 0;

            var txObstruction = FindFresnelObstruction(path, true, rxIndex);
            var rxObstruction = FindFresnelObstruction(path, false, rxIndex);

            if (txObstruction.angle < 0 && rxObstruction.angle < 0)
            {
                nobs = 0; // Clear line of sight, no obstructions
            }
            else if (txObstruction.index == rxObstruction.index)
                nobs = 1; // A single obstruction
            else
            {

                // Two or more obstructions. Find LOS crossing obstructions between the two already detected obstructions to determine if there is three or more.

                var txo = path[txObstruction.index];
                var rxo = path[rxObstruction.index];
                var hdist = txo.Distance2d(rxo);

                if (FindLosObstruction(path, txObstruction.index, rxObstruction.index, (rxo.Z - txo.Z) / hdist) >= 0)
                {
                    nobs = 3; // There is at least one obstruction between txo and rxo, making no clear line of sight between the obstructions
                }
                else
                {
                    nobs = 2; // There are no obstructions between txo and rxo, making a clear line of sight between the obstructions
                }
            }

            if (nobs != 0)
            {
                txi = txObstruction.distance3d;
                rxi = rxObstruction.distance3d;
                txa = txObstruction.angle;
                rxa = rxObstruction.angle;
            }

            return (horizontalDistance, txa, rxa, txi, rxi, nobs);
        }

        protected (int index, double angle, double distance2d, double distance3d) FindFresnelObstruction(PointUtm[] path, bool direction, int rxIndex = -1)
        {
            if (rxIndex == -1) rxIndex = path.Length - 1;
            
            var fromIx = direction ? 0 : rxIndex;
            var toIx = !direction ? 0 : rxIndex;

            var source = path[fromIx];
            var last = path[toIx];

            var inc = direction ? 1 : -1;
            fromIx += inc;

            var max = (slope: double.MinValue, index: -1);
            for (var i = fromIx; i != toIx; i += inc)
            {
                var dzz = path[i].Z - source.Z;
                var dxy = source.Distance2d(path[i]);
                var slope = dzz / dxy;

                if (slope > max.slope)
                {
                    max = (slope, i);
                }
            }

            if (max.index <= 0) return (-1, 0, 0, 0);

            var point = path[max.index];
            var angle = GetAngle(source, last, point);

            return (max.index, angle.angle, angle.distance2d, angle.distance3d);
        }

        protected (double angle, double distance2d, double distance3d) GetAngle(PointUtm source, PointUtm last, PointUtm max)
        {
            var d2d = source.Distance2d(max);
            var dx = source.Distance3d(max);

            var horizontalDistance = last.Distance2d(source);
            var sightLineSlope = (last.Z - source.Z) / horizontalDistance;
            var sightLineAtMax = source.Z + sightLineSlope * d2d;

            var dz = max.Z - sightLineAtMax;

            var angle = Math.Atan(dz / dx);

            return (angle, d2d, dx);
        }

        protected int FindLosObstruction(PointUtm[] path, int fromIx, int toIx, double sightLineHeightChangePerMeter)
        {
            var inc = Math.Sign(toIx - fromIx);
            var source = path[fromIx];

            fromIx += inc;

            for (var i = fromIx; i != toIx; i += inc)
            {
                var distanceFromTx = source.Distance2d(path[i]);
                var sightLineHeight = source.Z + distanceFromTx * sightLineHeightChangePerMeter;

                if (path[i].Z >= sightLineHeight)
                    return i;
            }

            return -1;
        }
    }
}