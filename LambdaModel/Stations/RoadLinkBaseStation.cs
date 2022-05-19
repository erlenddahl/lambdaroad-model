using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using ConsoleUtilities.ConsoleInfoPanel;
using DotSpatial.Data;
using DotSpatial.Topology;
using LambdaModel.General;
using LambdaModel.Terrain;
using Newtonsoft.Json;
using no.sintef.SpeedModule.Geometry.SimpleStructures;

namespace LambdaModel.Stations
{
    public class RoadLinkBaseStation : BaseStation
    {
        [JsonIgnore]
        public List<ShapeLink> Links { get; set; } = new List<ShapeLink>();

        [JsonIgnore]
        public int BaseStationIndex { get; set; }

        [JsonIgnore] public string CipName => Name + " (" + Id + ")";

        public RoadLinkBaseStation(double x, double y, int heightAboveTerrain, int maxRadius, ConsoleInformationPanel cip = null) : base(x, y, heightAboveTerrain, maxRadius, cip)
        {
        }

        public void SortLinks()
        {
            Links = Links.OrderBy(p => Center.AngleFromHorizon(new Point3D(p.Cx, p.Cy))).ToList();
        }

        private void RemoveLinksBy(string desc, string removalDesc, Func<ShapeLink, bool> filter)
        {
            using (var pb = Cip?.SetProgress(desc + " [" + CipName + "]", max: Links.Count))
            {
                var removed = Links.RemoveAll(p =>
                {
                    var res = filter(p);
                    pb?.Increment();
                    return res;
                });

                Cip?.Increment(removalDesc, removed);
            }
        }

        /// <summary>
        /// Check all links and removes those that has too much path loss due to distance alone.
        /// </summary>
        /// <returns></returns>
        public void RemoveLinksTooFarAway(double minimumAllowableRsrp, double rxHeightAboveTerrain)
        {
            Func<double, double, double> rsrp = (x, y) =>
            {
                var loss = Calculator.CalculateMinPossibleLoss(Center.DistanceTo2D(x, y), HeightAboveTerrain, rxHeightAboveTerrain);
                return CalculateRsrpAtAngle(AngleTo(new Point3D(x, y)), loss);
            };

            RemoveLinksBy("Checking road link min possible path loss", "Road links removed (min loss)", link =>
            {
                if (link.Geometry.Any(p => rsrp(p.X, p.Y) >= minimumAllowableRsrp)) return false;
                return true;
            });
        }

        /// <summary>
        /// Check all links and removes those that has too much path loss due to distance alone.
        /// </summary>
        /// <returns></returns>
        public void RemoveLinksWithTooLowRsrp(double minRsrp)
        {
            RemoveLinksBy("Checking road link calculated path loss", "Road links removed (actual loss)", p =>
            {
                var minPathLoss = p.Geometry.Max(c => c.M?.MaxRsrp ?? double.MinValue);
                return minPathLoss < minRsrp;
            });
        }
        
        public (long calculations, long distance) Calculate(ITiffReader tiles, int linkCalculationPointFrequency, double receiverHeightAboveTerrain, CancellationToken cancellationToken, int numBaseStations = 1, int baseStationIx = 0)
        {
            SortLinks();
            var calculations = 0;
            var distance = 0L;

            Center.Z = tiles.GetAltitude(Center);
            var process = System.Diagnostics.Process.GetCurrentProcess();

            foreach (var link in Cip.Run("Calculating path loss [" + CipName + "]", Links))
            {
                var linkCalcs = 0;
                var linkDist = 0L;

                var indexAdjustedForEndPoint = false;
                for (var i = 0; i < link.Geometry.Length; i += linkCalculationPointFrequency)
                {
                    if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException("Operation cancelled by user.");

                    var c = link.Geometry[i];

                    if (Center.DistanceTo2D(c) > MaxRadius)
                    {
                        Cip?.Increment("Points outside of radius");
                        continue;
                    }

                    if (c.M == null) c.M = new CalculationDetails() {BaseStationRsrp = new double[numBaseStations]};

                    // Get the X,Y,Z vector from the center to these coordinates.
                    var vectorLength = tiles.FillVector(_vector, Center.X, Center.Y, c.X, c.Y, withHeights: true);

                    // Calculate the loss for this point, and store it in the results matrix
                    var loss = Calculator.CalculateLoss(_vector, HeightAboveTerrain, receiverHeightAboveTerrain, vectorLength - 1);
                    var angle = AngleTo(c);
                    var value = CalculateRsrpAtAngle(angle, loss);
                    c.M.BaseStationRsrp[baseStationIx] = value;
                    if (value > c.M.MaxRsrp)
                        c.M.MaxRsrp = value;

                    linkCalcs++;
                    linkDist += vectorLength;

                    // Make sure the final point is calculated
                    if (!indexAdjustedForEndPoint && i + linkCalculationPointFrequency >= link.Geometry.Length)
                    {
                        i = link.Geometry.Length - 1 - linkCalculationPointFrequency;
                        indexAdjustedForEndPoint = true;
                    }
                }

                calculations += linkCalcs;
                distance += linkDist;

                Cip?.Increment("Points calculated", linkCalcs);
                Cip?.Increment("Terrain lookups", linkDist);
                Cip?.Max("Memory usage", process.WorkingSet64 / 1024d / 1024d);
            }

            return (calculations, distance);
        }
    }
}
