using System;
using System.Linq;
using LambdaModel.General;
using LambdaModel.Utilities;
using no.sintef.SpeedModule.Geometry.SimpleStructures;

namespace LambdaModel.PathLoss
{
    public class ItsG5PathLossCalculator : IPathLossCalculator
    {
        public int DistanceScale { get; set; } = 1;
        public TrafficCase TrafficCase { get; set; } = TrafficCase.NoTraffic;

        public double CalculateLoss(Point4D<double>[] path, double txHeightAboveTerrain, double rxHeightAboveTerrain, int rxIndex = -1)
        {
            var p = GetParameters(path, rxIndex);
            return 15.9 * Math.Log10(p.horizontalDistance) - 0.55 * txHeightAboveTerrain + 1.83 * (int) TrafficCase + 0.66 * p.dmax + 9.66e-03 * p.dmax_tx + 9.66e-03 * p.dmax_rx + 59.2;
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
            // Using extreme values for all constants and features to find the lowest feasible path loss for a given horizontal distance and tx height.
            var dmax = Math.Max(txHeightAboveTerrain, rxHeightAboveTerrain);
            var dmax_tx = 0;
            var dmax_rx = horizontalDistance;

            return 15.9 * Math.Log10(horizontalDistance) - 0.55 * txHeightAboveTerrain + 1.83 * (int)TrafficCase + 0.66 * dmax + 9.66e-03 * dmax_tx + 9.66e-03 * dmax_rx + 59.2;
        }

        protected (double horizontalDistance, double dmax, double dmax_tx, double dmax_rx) GetParameters(Point4D<double>[] path, int rxIndex = -1)
        {
            if (rxIndex == -1) rxIndex = path.Length - 1;

            var horizontalDistance = rxIndex * DistanceScale;
            var (index, dmax) = FindLosObstruction(path, horizontalDistance, rxIndex);

            var dmax_tx = index * DistanceScale;

            return (horizontalDistance, dmax, dmax_tx, horizontalDistance - dmax_tx);
        }

        /// <summary>
        /// Draws a straight line between the two points at fromIx and toIx, and checks if any points between them
        /// crosses this line (obstructs line of sight).
        /// </summary>
        /// <param name="path"></param>
        /// <param name="horizontalDistance"></param>
        /// <param name="rxIndex"></param>
        /// <returns></returns>
        protected (int index, double dmax) FindLosObstruction(Point4D<double>[] path, double horizontalDistance, int rxIndex)
        {
            var sightLineHeightChangePerMeter = (path[0].Z - path[rxIndex].Z) / horizontalDistance;

            // Calculate how much the sight line height changes for every point.
            var sightLineHeightChangePerPoint = DistanceScale * sightLineHeightChangePerMeter;

            // The sight line starts at the Z value of the first point.
            var sightLineHeight = path[0].Z;

            var dmax = double.MinValue;
            var dmaxIndex = -1;

            for (var i = 1; i < rxIndex; i++)
            {
                // For every point, the sight line increases (or decreases if the change is negative) by a constant value.
                sightLineHeight += sightLineHeightChangePerPoint;

                // If the point at this location is above the current sight line altitude, return it.
                var d = path[i].Z - sightLineHeight;
                if (d > dmax)
                {
                    dmax = d;
                    dmaxIndex = i;
                }
            }

            return (dmaxIndex, dmax);
        }
    }
}