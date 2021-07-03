using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleUtilities.ConsoleInfoPanel;
using DotSpatial.Data;
using DotSpatial.Topology;
using LambdaModel.General;
using LambdaModel.PathLoss;
using LambdaModel.Terrain;
using no.sintef.SpeedModule.Geometry.SimpleStructures;

namespace LambdaModel.Calculations
{
    public class RoadNetworkCalculator
    {
        private readonly Point4D[] _vector;
        private readonly PathLossCalculator _calc;
        private readonly ConsoleInformationPanel _cip;

        public ITiffReader Tiles { get; }
        public int Radius { get; }
        public Point3D Center { get; }
        public ShapeLink[] RoadLinks { get; set; }
        public double TxHeightAboveTerrain { get; set; }

        public RoadNetworkCalculator(ITiffReader tiles, string roadShapeLocation, int radius, Point3D center, double txHeightAboveTerrain, ConsoleInformationPanel cip = null)
        {
            _cip = cip;

            cip?.Set("Road network source", System.IO.Path.GetFileName(roadShapeLocation));
            cip?.Set("Calculation radius", radius);

            Tiles = tiles;
            Radius = radius;
            Center = center;
            TxHeightAboveTerrain = txHeightAboveTerrain;
            RoadLinks = ShapeLink.ReadLinks(roadShapeLocation, Center, radius)
                .OrderBy(p => Center.AngleFromHorizon(new Point3D(p.Cx, p.Cy)))
                .ToArray();

            cip?.Set("Relevant road links", RoadLinks.Length);

            center.Z = tiles.GetAltitude(center);

            // Initialize a PointUtm array that is to be (re)used as the vector of points from
            // the center to each of the points that should be calculated.
            _vector = new Point4D[(int) Math.Sqrt((long) radius * radius * 2L) + 1];
            for (var i = 0; i < _vector.Length; i++)
                _vector[i] = new Point4D(0, 0);

            _calc = new PathLossCalculator();
        }

        /// <summary>
        /// Check all links and removes those that has too much path loss due to distance alone.
        /// </summary>
        /// <returns></returns>
        public void RemoveLinksTooFarAway(int maxPathLoss)
        {
            using (var pb = _cip?.SetProgress("Checking road link min possible path loss", max: RoadLinks.Length))
            {
                var linkIdsToRemove = new HashSet<int>(RoadLinks.Where(p =>
                {
                    var minDistToCenter = Center.DistanceTo2D(p.Cx, p.Cy) - p.Length;
                    var minPossiblePathLoss = _calc.CalculateMinPossibleLoss(minDistToCenter, TxHeightAboveTerrain);
                    pb?.Increment();
                    return minPossiblePathLoss > maxPathLoss;
                }).Select(p => p.ID));

                RoadLinks = RoadLinks.Where(p => !linkIdsToRemove.Contains(p.ID)).ToArray();

                _cip.Set("Road links removed", linkIdsToRemove.Count);
            }
        }

        public int Calculate()
        {
            var calculations = 0;

            using (var pb = _cip?.SetProgress("Calculating path loss", 0, RoadLinks.Length, true))
            {
                foreach (var link in RoadLinks)
                {
                    var linkCalcs = 0;
                    foreach (var c in link.Geometry)
                    {
                        if (Center.DistanceTo2D(c) > Radius)
                        {
                            _cip?.Increment("Points outside of radius");
                            continue;
                        }

                        // Get the X,Y,Z vector from the center to these coordinates.
                        var vectorLength = Tiles.FillVector(_vector, Center.X, Center.Y, c.X, c.Y, withHeights: true);

                        // Calculate the loss for this point, and store it in the results matrix
                        c.M = _calc.CalculateLoss(_vector, TxHeightAboveTerrain, 2, vectorLength - 1);

                        linkCalcs++;
                    }

                    pb?.Increment();
                    calculations += linkCalcs;
                    _cip?.Increment("Points calculated", linkCalcs);
                }
            }

            return calculations;
        }

        public void SaveResults(string outputLocation)
        {
            var shp = new FeatureSet(FeatureType.Point);

            var table = shp.DataTable;
            table.Columns.Add("Loss", typeof(double));
            table.Columns.Add("RoadLinkId", typeof(string));
            table.AcceptChanges();

            using (var pb = _cip?.SetProgress("Saving results", max: RoadLinks.Length))
                foreach (var link in RoadLinks)
                {
                    foreach (var c in link.Geometry)
                    {
                        if (double.IsNaN(c.M)) continue;
                        var feature = shp.AddFeature(new Point(new Coordinate(c.X, c.Y, c.Z)));
                        feature.DataRow["Loss"] = c.M;
                        feature.DataRow["RoadLinkId"] = link.Name;
                    }

                    pb?.Increment();
                }
            shp.SaveAs(outputLocation, true);
        }
    }
}
