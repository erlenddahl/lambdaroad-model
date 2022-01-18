using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleUtilities.ConsoleInfoPanel;
using DotSpatial.Data;
using DotSpatial.Topology;
using LambdaModel.General;
using LambdaModel.Terrain;
using no.sintef.SpeedModule.Geometry.SimpleStructures;

namespace LambdaModel.Stations
{
    public class RoadLinkBaseStation : BaseStation
    {
        public List<ShapeLink> Links { get; set; } = new List<ShapeLink>();

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
                var minPossiblePathLoss = _calc.CalculateMinPossibleLoss(minDistToCenter, HeightAboveTerrain);
                return minPossiblePathLoss > maxPathLoss;
            });
        }

        /// <summary>
        /// Check all links and removes those that has too much path loss due to distance alone.
        /// </summary>
        /// <returns></returns>
        public void RemoveLinksWithTooMuchPathLoss(double maxPathLoss)
        {
            RemoveLinksBy("Checking road link calculated path loss", "Road links removed (actual loss)", p =>
            {
                var minPathLoss = p.Geometry.Min(c => c.M);
                return minPathLoss > maxPathLoss;
            });
        }
        
        public int Calculate(ITiffReader tiles)
        {
            SortLinks();
            var calculations = 0;

            Center.Z = tiles.GetAltitude(Center);

            foreach (var link in Cip.Run("Calculating path loss [" + Name + "]", Links))
            {
                var linkCalcs = 0;
                foreach (var c in link.Geometry)
                {
                    if (Center.DistanceTo2D(c) > MaxRadius)
                    {
                        Cip?.Increment("Points outside of radius");
                        continue;
                    }

                    // Get the X,Y,Z vector from the center to these coordinates.
                    var vectorLength = tiles.FillVector(_vector, Center.X, Center.Y, c.X, c.Y, withHeights: true);

                    // Calculate the loss for this point, and store it in the results matrix
                    var value = _calc.CalculateLoss(_vector, HeightAboveTerrain, 2, vectorLength - 1);
                    if (value < c.M)
                        c.M = value;

                    linkCalcs++;
                }

                calculations += linkCalcs;
                Cip?.Increment("Points calculated", linkCalcs);
            }

            return calculations;
        }
    }
}
