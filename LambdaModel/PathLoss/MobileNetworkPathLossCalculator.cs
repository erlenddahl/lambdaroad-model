using System;
using System.Linq;
using LambdaModel.General;
using LambdaModel.Utilities;
using no.sintef.SpeedModule.Geometry.SimpleStructures;

namespace LambdaModel.PathLoss
{
    /// <summary>
    /// Calculates the path loss of received mobile signal strength.
    /// </summary>
    public class MobileNetworkPathLossCalculator : IPathLossCalculator
    {
        public int DistanceScale { get; set; } = 1;

        public double CalculateLoss(Point4D<double>[] path, double txHeightAboveTerrain, double rxHeightAboveTerrain, int rxIndex = -1)
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

        protected (double horizontalDistance, double txa, double rxa, double txi, double rxi, int nobs) GetParameters(Point4D<double>[] path, int rxIndex = -1)
        {
            if (rxIndex == -1) rxIndex = path.Length - 1;

            var horizontalDistance = rxIndex * DistanceScale;

            var rxa = 0d;
            var txa = 0d;
            var rxi = 0d;
            var txi = 0d;
            var nobs = 0;

            // Tested twice; combining this into a single call that calculates the fresnel obstructions both ways simultaneously
            // is slower than doing it once in each direction.
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

        protected (int index, double angle, double distance3d) FindFresnelObstruction(Point4D<double>[] path, bool direction, int rxIndex = -1)
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
                // Checking if this is a top (z >= previousZ && z >= nextZ) seems to be a much more expensive operation
                // than just performing the calculations below for all points.

                distance += distanceBetweenPoints;
                var dzz = path[i].Z - source.Z;
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

        /// <summary>
        /// Draws a straight line between the two points at fromIx and toIx, and checks if any points between them
        /// crosses this line (obstructs line of sight).
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fromIx"></param>
        /// <param name="toIx"></param>
        /// <param name="sightLineHeightChangePerMeter"></param>
        /// <returns></returns>
        protected int FindLosObstruction(Point4D<double>[] path, int fromIx, int toIx, double sightLineHeightChangePerMeter)
        {
            var inc = Math.Sign(toIx - fromIx);
            
            // Calculate how much the sight line height changes for every point.
            var sightLineHeightChangePerPoint = DistanceScale * sightLineHeightChangePerMeter;

            // The sight line starts at the Z value of the first point.
            var sightLineHeight = path[fromIx].Z;

            for (var i = fromIx + inc; i != toIx; i += inc)
            {
                // For every point, the sight line increases (or decreases if the change is negative) by a constant value.
                sightLineHeight += sightLineHeightChangePerPoint;

                // If the point at this location is above the current sight line altitude, return it.
                if (path[i].Z >= sightLineHeight)
                    return i;
            }

            return -1;
        }
    }
}