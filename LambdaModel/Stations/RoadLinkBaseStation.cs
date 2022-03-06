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
        public void RemoveLinksTooFarAway(double minimumAllowableRsrp)
        {
            Func<double, double, double, double> rsrp = (x, y, loss) => CalculateRsrpAtAngle(AngleTo(new Point3D(x, y)), loss);

            RemoveLinksBy("Checking road link min possible path loss", "Road links removed (max loss)", p =>
            {
                var bounds = BoundingBox2D.FromPoints(p.Geometry);

                var minDistToCenter = Center.DistanceTo2D(p.Cx, p.Cy) - p.Length;
                var minPossiblePathLoss = Calculator.CalculateMinPossibleLoss(minDistToCenter, HeightAboveTerrain);

                if (rsrp(bounds.Xmin, bounds.Ymin, minPossiblePathLoss) >= minimumAllowableRsrp) return false;
                if (rsrp(bounds.Xmin, bounds.Ymax, minPossiblePathLoss) >= minimumAllowableRsrp) return false;
                if (rsrp(bounds.Xmax, bounds.Ymin, minPossiblePathLoss) >= minimumAllowableRsrp) return false;
                if (rsrp(bounds.Xmax, bounds.Ymax, minPossiblePathLoss) >= minimumAllowableRsrp) return false;

                return true;
            });
        }

        /// <summary>
        /// Check all links and removes those that have no points inside of a gain sector with signal.
        /// </summary>
        /// <returns></returns>
        public void RemoveLinksOutsideGainSectors()
        {
            Func<double, double, double> gain = (x, y) => Gain.GetGainAtAngle(AngleTo(new Point3D(x, y)));

            RemoveLinksBy("Checking road links against gain sectors", "Road links removed (gain sector)", p =>
            {
                var bounds = BoundingBox2D.FromPoints(p.Geometry);
                if (gain(bounds.Xmin, bounds.Ymin) > 0) return false;
                if (gain(bounds.Xmin, bounds.Ymax) > 0) return false;
                if (gain(bounds.Xmax, bounds.Ymin) > 0) return false;
                if (gain(bounds.Xmax, bounds.Ymax) > 0) return false;

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
        
        public (long calculations, long distance) Calculate(ITiffReader tiles, int linkCalculationPointFrequency, double receiverHeightAboveTerrain, int numBaseStations = 1, int baseStationIx = 0)
        {
            SortLinks();
            var calculations = 0;
            var distance = 0L;

            Center.Z = tiles.GetAltitude(Center);
            var process = System.Diagnostics.Process.GetCurrentProcess();

            foreach (var link in Cip.Run("Calculating path loss [" + Name + "]", Links))
            {
                var linkCalcs = 0;
                var linkDist = 0L;

                for (var i = 0; i < link.Geometry.Length; i += linkCalculationPointFrequency)
                {
                    var c = link.Geometry[i];

                    if (Center.DistanceTo2D(c) > MaxRadius)
                    {
                        Cip?.Increment("Points outside of radius");
                        continue;
                    }

                    var angle = AngleTo(c);

                    if (Gain.GetGainAtAngle(angle) <= 0)
                    {
                        Cip?.Increment("Points outside of gain sectors");
                        continue;
                    }

                    if (c.M == null) c.M = new CalculationDetails() {BaseStationRsrp = new double[numBaseStations]};

                    // Get the X,Y,Z vector from the center to these coordinates.
                    var vectorLength = tiles.FillVector(_vector, Center.X, Center.Y, c.X, c.Y, withHeights: true);

                    // Calculate the loss for this point, and store it in the results matrix
                    var loss = Calculator.CalculateLoss(_vector, HeightAboveTerrain, receiverHeightAboveTerrain, vectorLength - 1);
                    var value = CalculateRsrpAtAngle(angle, loss);
                    c.M.BaseStationRsrp[baseStationIx] = value;
                    if (value > c.M.MaxRsrp)
                        c.M.MaxRsrp = value;

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
