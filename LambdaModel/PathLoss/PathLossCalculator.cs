using System;
using System.Linq;
using LambdaModel.General;
using LambdaModel.Utilities;
using no.sintef.SpeedModule.Geometry.SimpleStructures;

namespace LambdaModel.PathLoss
{
    public class PathLossCalculator
    {
        public int DistanceScale { get; set; } = 1;

        public double CalculateLoss(Point4D[] path, double txHeightAboveTerrain, double rxHeightAboveTerrain, int rxIndex = -1)
        {
            var p = GetParameters(path, rxIndex);
            return 25.1 * Math.Log(p.horizontalDistance) - 1.8e-01 * txHeightAboveTerrain + 1.3e+01 * p.rxa - 1.4e-04 * p.txa - 1.4e-04 * p.rxi - 3.0e-05 * p.txi + 4.9 * p.nobs + 29.3;
        }

        public double CalculateMinPossibleLoss(double horizontalDistance, double txHeightAboveTerrain)
        {
            // RangeTxa: -0,00007 - -0,00000         RangeRxa: 0,00411 - 10,20096
            // RangeTxi: -0,25720 - -0,00003         RangeRxi: -4,26540 - -0,00014

            // Running approximately 200 000 point calculations on approximately 1 700 random road links showed that
            // nothing usually shrank the path loss by more than -5.
            // This means that the minimum possible loss is decided by horizontal distance, tx height, and -5.

            return 25.1 * Math.Log(horizontalDistance) - 1.8e-01 * txHeightAboveTerrain - 5;
        }

        protected (double horizontalDistance, double txa, double rxa, double txi, double rxi, int nobs) GetParameters(Point4D[] path, int rxIndex = -1)
        {
            if (rxIndex == -1) rxIndex = path.Length - 1;
            var tx = path[0];
            var rx = path[rxIndex];

            var horizontalDistance = rxIndex * DistanceScale;

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
                var hdist = txo.DistanceTo2D(rxo);

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

        protected (int index, double angle, double distance3d) FindFresnelObstruction(Point4D[] path, bool direction, int rxIndex = -1)
        {
            if (rxIndex == -1) rxIndex = path.Length - 1;

            int fromIx, toIx, inc;

            if (direction)
            {
                fromIx = 0;
                toIx = rxIndex;
                inc = 1;
            }
            else
            {
                fromIx = rxIndex;
                toIx = 0;
                inc = -1;
            }

            var source = path[fromIx];
            var last = path[toIx];

            var distanceBetweenPoints = DistanceScale;
            var distance = 0;

            // Go through each point at the height profile from rx to tx (or the other way around),
            // and find the point that has the largest slope from the source to the height profile.
            var (maxSlope, maxIndex) = (double.MinValue, -1);
            for (var i = fromIx + inc; i != toIx; i += inc)
            {
                var dzz = path[i].Z - source.Z;
                distance += distanceBetweenPoints;
                var slope = dzz / distance;

                if (slope > maxSlope)
                {
                    (maxSlope, maxIndex) = (slope, i);
                }
            }

            if (maxIndex <= 0) return (-1, 0, 0);

            var point = path[maxIndex];
            var (angle, distance3d) = GetAngle(source, last, point);

            return (maxIndex, angle, distance3d);
        }

        protected (double angle, double distance3d) GetAngle(Point3D source, Point3D last, Point3D max)
        {
            var d2d = source.DistanceTo2D(max);
            var dx = source.DistanceTo(max);

            var horizontalDistance = last.DistanceTo2D(source);
            var sightLineSlope = (last.Z - source.Z) / horizontalDistance;
            var sightLineAtMax = source.Z + sightLineSlope * d2d;

            var dz = max.Z - sightLineAtMax;

            var angle = Math.Atan(dz / dx);

            return (angle, dx);
        }

        protected int FindLosObstruction(Point4D[] path, int fromIx, int toIx, double sightLineHeightChangePerMeter)
        {
            var inc = Math.Sign(toIx - fromIx);
            var source = path[fromIx];

            var sightLineHeight = source.Z;
            var sightLineHeightChangePerPoint = DistanceScale * sightLineHeightChangePerMeter;

            for (var i = fromIx + inc; i != toIx; i += inc)
            {
                sightLineHeight += sightLineHeightChangePerPoint;

                if (path[i].Z >= sightLineHeight)
                    return i;
            }

            return -1;
        }
    }
}