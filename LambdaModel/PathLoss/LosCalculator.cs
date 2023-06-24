using System;
using System.Linq;
using LambdaModel.General;
using LambdaModel.Utilities;
using no.sintef.SpeedModule.Geometry.SimpleStructures;

namespace LambdaModel.PathLoss
{
    /// <summary>
    /// Calculates if a receiver has LOS to a transmitter (returns 1 for LOS or 0 for NLOS).
    /// </summary>
    public class LosCalculator : IPathLossCalculator
    {
        /// <summary>
        /// Intended to let the user pick different terrain resolutions (every 1 meter, every 10 meter, etc)
        /// for speedier calculations. Currently always 1, not tested for any other values.
        /// </summary>
        public int DistanceScale { get; set; } = 1;

        /// <summary>
        /// Calculates LOS/NLOS along the given path from an antenna at path[0] to a receiver at path[rxIndex].
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
            // If rxIndex is -1 (the default value), that is just a shortcut for saying we want to calculate to the end of the terrain profile.
            if (rxIndex == -1) rxIndex = path.Length - 1;

            // Calculate regression parameters/features for the path from 0 to rxIndex.
            var p = GetParameters(path, txHeightAboveTerrain, rxHeightAboveTerrain, rxIndex);

            return CalculateLoss(txHeightAboveTerrain, p);
        }

        protected double CalculateLoss(double txHeightAboveTerrain, (double horizontalDistance, int nobs) p)
        {
            // Return 1 if nobs (number of obstructions) is 0, or 0 otherwise.
            return p.nobs == 0 ? 1 : 0;
        }

        /// <summary>
        /// Calculates the minimum possible path loss at a given distance. Used to eliminate road links that are so far away that they are
        /// guaranteed to have too much path loss.
        /// </summary>
        /// <param name="horizontalDistance"></param>
        /// <param name="txHeightAboveTerrain"></param>
        /// <param name="rxHeightAboveTerrain"></param>
        /// <returns></returns>
        public double CalculateMinPossibleLoss(double horizontalDistance, double txHeightAboveTerrain, double rxHeightAboveTerrain)
        {
            // We don't want to eliminate anything yet.
            return double.MinValue;
        }

        /// <summary>
        /// Calculates parameters/features for the regression formula.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="txHeightAboveTerrain">How high above the terrain the TX antenna is.</param>
        /// <param name="rxHeightAboveTerrain">How high above the terrain the RX antenna is.</param>
        /// <param name="rxIndex"></param>
        /// <returns></returns>
        protected (double horizontalDistance, int nobs) GetParameters(Point4D<double>[] path, double txHeightAboveTerrain, double rxHeightAboveTerrain, int rxIndex = -1)
        {
            // If rxIndex is -1 (the default value), that is just a shortcut for saying we want to calculate to the end of the terrain profile.
            if (rxIndex == -1) rxIndex = path.Length - 1;

            // The total horizontal distance will be the requested index along the path, multiplied with the resolution.
            // In most cases the resolution is 1 meter, which means that each item in the path array represents one meter
            // out from the antenna.
            var horizontalDistance = rxIndex * DistanceScale;

            // Calculates fresnel obstructions between transmitter and receiver. Note that these "obstructions" don't have to be physical
            // obstructions. Even if there is a direct line of sight between transmitter and receiver, this will still keep track of the
            // parts of the terrain nearest to the transmitter (tx) and receiver (rx) that are closest to being an obstruction.
            var txObstruction = FindFresnelObstruction(path, CalculationDirection.TxToRx, txHeightAboveTerrain, rxHeightAboveTerrain, rxIndex);
            var rxObstruction = FindFresnelObstruction(path, CalculationDirection.RxToTx, txHeightAboveTerrain, rxHeightAboveTerrain, rxIndex);
            // Optimization note: Tested twice; combining this into a single call that calculates the fresnel obstructions both ways
            // simultaneously is slower than doing it once in each direction. Keep it simple (at least when that is also fastest).

            // Check if there is a line of sight, or if there are one or more actual obstructions
            int nobs = 1;
            if (txObstruction.angle < 0 && rxObstruction.angle < 0)
            {
                nobs = 0; // Clear line of sight, no obstructions
            }

            return (horizontalDistance, nobs);
        }

        /// <summary>
        /// Calculates fresnel obstructions between transmitter and receiver in the given direction.
        /// Note that these "obstructions" don't have to be physical obstructions. Even if there is a direct line of sight
        /// between transmitter and receiver, this will still keep track of the parts of the terrain nearest to the
        /// transmitter (tx) and receiver (rx) that are closest to being an obstruction.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="direction">The direction of calculation</param>
        /// <param name="txHeightAboveTerrain">How high above the terrain the TX antenna is.</param>
        /// <param name="rxHeightAboveTerrain">How high above the terrain the RX antenna is.</param>
        /// <param name="rxIndex"></param>
        /// <returns></returns>
        protected (int index, double angle, double distance2d) FindFresnelObstruction(Point4D<double>[] path, CalculationDirection direction, double txHeightAboveTerrain, double rxHeightAboveTerrain, int rxIndex = -1)
        {
            if (rxIndex == -1) rxIndex = path.Length - 1;

            int fromIx, toIx, inc;
            double sourceHeightAboveTerrain;
            double targetHeightAboveTerrain;

            // Depending on the direction, we want to start from the transmitter and move towards the receiver,
            // or the other way around.
            if (direction == CalculationDirection.TxToRx)
            {
                fromIx = 0;
                toIx = rxIndex;
                inc = 1;
                sourceHeightAboveTerrain = txHeightAboveTerrain;
                targetHeightAboveTerrain = rxHeightAboveTerrain;
            }
            else
            {
                fromIx = rxIndex;
                toIx = 0;
                inc = -1;
                sourceHeightAboveTerrain = rxHeightAboveTerrain;
                targetHeightAboveTerrain = txHeightAboveTerrain;
            }

            // Keep track of the data at the transmitter and receiver.
            var source = path[fromIx];
            var target = path[toIx];

            var distanceBetweenPoints = DistanceScale;
            var distance = 0;

            // Go through each point at the height profile from rx to tx (or the other way around),
            // and find the point that has the largest slope from the source to the height profile.
            var (maxSlope, maxIndex) = (double.MinValue, -1);
            for (var i = fromIx + inc; i != toIx + inc; i += inc)
            {
                // Optimization note: Checking if this is a top (z >= previousZ && z >= nextZ) seems to be a much
                // more expensive operation than just performing the calculations below for all points.

                // Calculate the slope between the start and the current position
                distance += distanceBetweenPoints;
                var dz = path[i].Z - (source.Z + sourceHeightAboveTerrain);
                var slope = dz / distance;

                // If this slope is larger than the currently largest seen slope, store it.
                if (slope > maxSlope)
                {
                    (maxSlope, maxIndex) = (slope, i);
                }
            }

            if (maxIndex < 0) return (-1, 0, 0);

            // Calculate the angle between the sight line RX-TX/TX-RX and the sight line source-max.
            var pointOfMaxObstruction = path[maxIndex];
            var (angle, distance2d) = GetAngle(source, target, pointOfMaxObstruction, direction, sourceHeightAboveTerrain, targetHeightAboveTerrain);

            return (maxIndex, angle, distance2d);
        }

        /// <summary>
        /// Calculate the angle between the sight line source-target and the sight line source-max.
        /// </summary>
        /// <param name="source">The point we're calculating from, usually TX or RX, depending on calculation direction.</param>
        /// <param name="target">The point we're calculating to, usually RX or TX, depending on calculation direction.</param>
        /// <param name="pointOfMaxObstruction">The point of max (fresnel) obstruction between source and target.</param>
        /// <param name="direction"></param>
        /// <param name="sourceHeightAboveTerrain">How high above the terrain the source (TX or RX) is.</param>
        /// <param name="targetHeightAboveTerrain">How high above the terrain the target (RX or TX) is.</param>
        /// <returns></returns>
        protected (double angle, double distance2d) GetAngle(Point3D source, Point3D target, Point3D pointOfMaxObstruction, CalculationDirection direction, double sourceHeightAboveTerrain, double targetHeightAboveTerrain)
        {
            var dx = source.DistanceTo2D(target);
            var dz = source.Z + sourceHeightAboveTerrain - (target.Z + targetHeightAboveTerrain);
            var angleStations = Math.Atan(dz / dx);

            dx = source.DistanceTo2D(pointOfMaxObstruction);
            dz = source.Z + sourceHeightAboveTerrain - pointOfMaxObstruction.Z;
            var angleSightLine = Math.Atan(dz/dx);

            var angleDiff = angleStations - angleSightLine;

            return (angleDiff, dx);
        }
    }
}