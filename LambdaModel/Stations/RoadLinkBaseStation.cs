using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public int BaseStationIndex { get; set; }

        public RoadLinkBaseStation(double x, double y, int heightAboveTerrain, int maxRadius, ConsoleInformationPanel cip = null) : base(x, y, heightAboveTerrain, maxRadius, cip)
        {
        }

        public void SortLinks()
        {
            Links = Links.OrderBy(p => Center.AngleFromHorizon(new Point3D(p.Cx, p.Cy))).ToList();
        }

        private void RemoveLinksBy(string desc, string removalDesc, Func<ShapeLink, bool> filter)
        {
            using (var pb = Cip?.SetProgress(desc + " [" + Name + "]", max: Links.Count))
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
        public void RemoveLinksTooFarAway(double maxPathLoss)
        {
            RemoveLinksBy("Checking road link min possible path loss", "Road links removed (max loss)", p =>
            {
                var minDistToCenter = Center.DistanceTo2D(p.Cx, p.Cy) - p.Length;
                var minPossiblePathLoss = Calculator.CalculateMinPossibleLoss(minDistToCenter, HeightAboveTerrain);
                return minPossiblePathLoss > maxPathLoss;
            });
        }

        /// <summary>
        /// Check all links and removes those that has too much path loss due to distance alone.
        /// </summary>
        /// <returns></returns>
        public void RemoveLinksWithTooLowRssi(double minRssi)
        {
            RemoveLinksBy("Checking road link calculated path loss", "Road links removed (actual loss)", p =>
            {
                var minPathLoss = p.Geometry.Max(c => c.M?.MaxRssi ?? double.MinValue);
                return minPathLoss < minRssi;
            });
        }
        
        public (long calculations, long distance) Calculate(ITiffReader tiles, double receiverHeightAboveTerrain, int numBaseStations = 1, int baseStationIx = 0)
        {
            SortLinks();
            var calculations = 0;
            var distance = 0L;

            Center.Z = tiles.GetAltitude(Center);
            var transmitPower = TotalTransmissionLevel;
            var process = System.Diagnostics.Process.GetCurrentProcess();

            foreach (var link in Cip.Run("Calculating path loss [" + Name + "]", Links))
            {
                var linkCalcs = 0;
                var linkDist = 0L;

                foreach (var c in link.Geometry)
                {
                    if (Center.DistanceTo2D(c) > MaxRadius)
                    {
                        Cip?.Increment("Points outside of radius");
                        continue;
                    }

                    if (c.M == null) c.M = new CalculationDetails() {BaseStationRssi = new double[numBaseStations]};

                    // Get the X,Y,Z vector from the center to these coordinates.
                    var vectorLength = tiles.FillVector(_vector, Center.X, Center.Y, c.X, c.Y, withHeights: true);

                    // Calculate the loss for this point, and store it in the results matrix
                    var loss = Calculator.CalculateLoss(_vector, HeightAboveTerrain, receiverHeightAboveTerrain, vectorLength - 1);
                    var value = transmitPower - loss;
                    c.M.BaseStationRssi[baseStationIx] = value;
                    if (value > c.M.MaxRssi)
                        c.M.MaxRssi = value;

                    linkCalcs++;
                    linkDist += vectorLength;
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
