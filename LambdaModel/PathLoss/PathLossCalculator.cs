using System;
using System.Linq;
using LambdaModel.General;
using LambdaModel.Utilities;
using no.sintef.SpeedModule.Geometry.SimpleStructures;

namespace LambdaModel.PathLoss
{
    public class PathLossCalculator
    {
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

            var horizontalDistance = rxIndex;

            var rxa = 0d;
            var txa = 0d;
            var rxi = 0d;
            var txi = 0d;
            var nobs = 0;

            var (txObstruction, rxObstruction) = FindFresnelObstruction(path, rxIndex);

            if (txObstruction.Angle < 0 && rxObstruction.Angle < 0)
            {
                nobs = 0; // Clear line of sight, no obstructions
            }
            else if (txObstruction.Index == rxObstruction.Index)
                nobs = 1; // A single obstruction
            else
            {

                // Two or more obstructions. Find LOS crossing obstructions between the two already detected obstructions to determine if there is three or more.

                var txo = path[txObstruction.Index];
                var rxo = path[rxObstruction.Index];
                var hdist = txo.DistanceTo2D(rxo);

                if (HasLosObstruction(path, txObstruction.Index, rxObstruction.Index, (rxo.Z - txo.Z) / hdist))
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
                txi = txObstruction.Distance3d;
                rxi = rxObstruction.Distance3d;
                txa = txObstruction.Angle;
                rxa = rxObstruction.Angle;
            }

            return (horizontalDistance, txa, rxa, txi, rxi, nobs);
        }

        protected (FresnelResult tx, FresnelResult rx) FindFresnelObstruction(Point4D[] path, int rxIndex = -1)
        {
            if (rxIndex == -1) rxIndex = path.Length - 1;

            var tx = path[0];
            var rx = path[rxIndex];
            var totalDistance = rxIndex;

            var ground = Math.Min(tx.Z, rx.Z);

            // Go through each point at the height profile from rx to tx (or the other way around),
            // and find the point that has the largest slope from the source to the height profile.
            var (maxSlopeTx, maxIndexTx) = (double.MinValue, -1);
            var (maxSlopeRx, maxIndexRx) = (double.MinValue, -1);
            for (var i = 1; i < rxIndex; i++)
            {

                if (!(path[i].Z >= path[i - 1].Z && path[i].Z >= path[i + 1].Z))
                    continue;

                var altitudeDiff = path[i].Z - ground;

                var distanceFromTx = i;
                var distanceToRx = totalDistance - distanceFromTx;
                
                var slopeTx = altitudeDiff / distanceFromTx;
                var slopeRx = altitudeDiff / distanceToRx;

                if (slopeTx > maxSlopeTx)
                {
                    (maxSlopeTx, maxIndexTx) = (slopeTx, i);
                }
                if (slopeRx > maxSlopeRx)
                {
                    (maxSlopeRx, maxIndexRx) = (slopeRx, i);
                }
            }

            return (new FresnelResult(maxIndexTx, path, tx, rx), new FresnelResult(maxIndexRx, path, rx, tx));
        }

        private static (double angle, double distance3d) GetAngle(Point3D source, Point3D last, Point3D max)
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

        public struct FresnelResult
        {
            public int Index;
            public double Angle;
            public double Distance3d;

            public FresnelResult(int index, Point4D[] path, Point3D source, Point3D target)
            {
                Index = index;

                if (index >= 0)
                {
                    var point = path[index];
                    (Angle, Distance3d) = GetAngle(source, target, point);
                }
                else
                {
                    Angle = -1;
                    Distance3d = -1;
                }
            }
        }

        protected bool HasLosObstruction(Point4D[] path, int fromIx, int toIx, double sightLineHeightChangePerMeter)
        {
            var source = path[fromIx];
            var minHeight = Math.Min(source.Z, path[toIx].Z);

            for (var i = fromIx + 1; i < toIx; i++)
            {
                if (path[i].Z < minHeight) continue;

                var distanceFromTx = i - fromIx;
                var sightLineHeight = source.Z + distanceFromTx * sightLineHeightChangePerMeter;

                if (path[i].Z >= sightLineHeight)
                    return true;
            }

            return false;
        }
    }
}