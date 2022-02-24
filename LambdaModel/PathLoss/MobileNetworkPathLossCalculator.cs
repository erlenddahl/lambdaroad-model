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
        /// <summary>
        /// Intended to let the user pick different terrain resolutions (every 1 meter, every 10 meter, etc)
        /// for speedier calculations. Currently always 1, not tested for any other values.
        /// </summary>
        public int DistanceScale { get; set; } = 1;

        /// <summary>
        /// Calculates the path loss along the given path from an antenna at path[0] to a receiver at path[rxIndex].
        /// </summary>
        /// <param name="path">Coordinates from the source (antenna) to the target (receiver). Each coordinate has geographical coordinates (XY),
        /// but the only relevant here is the Z value, which gives the terrain profile between source and target.</param>
        /// <param name="txHeightAboveTerrain">How high above the terrain the source (antenna) is placed.</param>
        /// <param name="rxHeightAboveTerrain">How high above the terrain the target (receiver) is placed.</param>
        /// <param name="rxIndex">Where on the height profile we want to calculate. This is usually set to path.Length - 1 (to calculate at the
        /// very end of the terrain profile), but in some cases it is useful to calculate for all points along the profile, and then rxIndex will
        /// go from 0 to path.Length - 1 with whatever increment we want.</param>
        /// <returns>The final path loss at rxIndex.</returns>
        public double CalculateLoss(Point4D<double>[] path, double txHeightAboveTerrain, double rxHeightAboveTerrain, int rxIndex = -1)
        {
            // Calculate regression parameters/features for the path from 0 to rxIndex.
            var p = GetParameters(path, rxIndex);

            // This is the actual regression formula, using the parameters from above.
            return 25.1 * Math.Log(p.horizontalDistance) - 1.8e-01 * txHeightAboveTerrain + 1.3e+01 * p.rxa - 1.4e-04 * p.txa - 1.4e-04 * p.rxi - 3.0e-05 * p.txi + 4.9 * p.nobs + 29.3;
        }

        /// <summary>
        /// Calculates the minimum possible path loss at a given distance. Used to eliminate road links that are so far away that they are
        /// guaranteed to have too much path loss.
        /// </summary>
        /// <param name="horizontalDistance"></param>
        /// <param name="txHeightAboveTerrain"></param>
        /// <returns></returns>
        public double CalculateMinPossibleLoss(double horizontalDistance, double txHeightAboveTerrain)
        {
            // RangeTxa: -0,00007 - -0,00000         RangeRxa: 0,00411 - 10,20096
            // RangeTxi: -0,25720 - -0,00003         RangeRxi: -4,26540 - -0,00014

            // Running approximately 200 000 point calculations on approximately 1 700 random road links showed that
            // nothing usually shrank the path loss by more than -5.
            // This means that the minimum possible loss is decided by horizontal distance, tx height, and -5.

            return 25.1 * Math.Log(horizontalDistance) - 1.8e-01 * txHeightAboveTerrain - 5;
        }

        /// <summary>
        /// Calculates parameters/features for the regression formula.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="rxIndex"></param>
        /// <returns></returns>
        protected (double horizontalDistance, double txa, double rxa, double txi, double rxi, int nobs) GetParameters(Point4D<double>[] path, int rxIndex = -1)
        {
            // If rxIndex is -1 (the default value), that is just a shortcut for saying we want to calculate to the end of the terrain profile.
            if (rxIndex == -1) rxIndex = path.Length - 1;

            // The total horizontal distance will be the requested index along the path, multiplied with the resolution.
            // In most cases the resolution is 1 meter, which means that each item in the path array represents one meter
            // out from the antenna.
            var horizontalDistance = rxIndex * DistanceScale;

            // Initialized the regression parameters
            var rxa = 0d;
            var txa = 0d;
            var rxi = 0d;
            var txi = 0d;
            var nobs = 0;

            
            // Calculates fresnel obstructions between transmitter and receiver. Note that these "obstructions" don't have to be physical
            // obstructions. Even if there is a direct line of sight between transmitter and receiver, this will still keep track of the
            // parts of the terrain nearest to the transmitter (tx) and receiver (rx) that are closest to being an obstruction.
            var txObstruction = FindFresnelObstruction(path, true, rxIndex);
            var rxObstruction = FindFresnelObstruction(path, false, rxIndex);
            // Optimization note: Tested twice; combining this into a single call that calculates the fresnel obstructions both ways
            // simultaneously is slower than doing it once in each direction. Keep it simple (at least when that is also fastest).

            // Check if there is a line of sight, or if there are one or more actual obstructions
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

            // If there is a direct line of sight, all values will be 0. Otherwise, they will get their values from the obstructions found above.
            if (nobs != 0)
            {
                txi = txObstruction.distance3d;
                rxi = rxObstruction.distance3d;
                txa = txObstruction.angle;
                rxa = rxObstruction.angle;
            }

            return (horizontalDistance, txa, rxa, txi, rxi, nobs);
        }

        /// <summary>
        /// Calculates fresnel obstructions between transmitter and receiver in the given direction.
        /// Note that these "obstructions" don't have to be physical obstructions. Even if there is a direct line of sight
        /// between transmitter and receiver, this will still keep track of the parts of the terrain nearest to the
        /// transmitter (tx) and receiver (rx) that are closest to being an obstruction.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="direction"></param>
        /// <param name="rxIndex"></param>
        /// <returns></returns>
        protected (int index, double angle, double distance3d) FindFresnelObstruction(Point4D<double>[] path, bool direction, int rxIndex = -1)
        {
            if (rxIndex == -1) rxIndex = path.Length - 1;

            int fromIx, toIx, inc;

            // Depending on the direction, we want to start from the transmitter and move towards the receiver,
            // or the other way around.
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

            // Keep track of the data at the transmitter and receiver.
            var source = path[fromIx];
            var last = path[toIx];

            var distanceBetweenPoints = DistanceScale;
            var distance = 0;

            // Go through each point at the height profile from rx to tx (or the other way around),
            // and find the point that has the largest slope from the source to the height profile.
            var (maxSlope, maxIndex) = (double.MinValue, -1);
            for (var i = fromIx + inc; i != toIx; i += inc)
            {
                // Optimization note: Checking if this is a top (z >= previousZ && z >= nextZ) seems to be a much
                // more expensive operation than just performing the calculations below for all points.

                // Calculate the slope between the start and the current position
                distance += distanceBetweenPoints;
                var dzz = path[i].Z - source.Z;
                var slope = dzz / distance;

                // If this slope is larger than the currently largest seen slope, store it.
                if (slope > maxSlope)
                {
                    (maxSlope, maxIndex) = (slope, i);
                }
            }

            if (maxIndex <= 0) return (-1, 0, 0);

            // Calculate the angle between the source, the target and the max slope point.
            var point = path[maxIndex];
            var (angle, distance3d) = GetAngle(source, last, point);

            return (maxIndex, angle, distance3d);
        }

        /// <summary>
        /// Given three points, calculate the angle at the max point between the source and the last points.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="last"></param>
        /// <param name="max"></param>
        /// <returns></returns>
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