using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
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

        public TileCache Tiles { get; }
        public int Radius { get; }
        public Point3D Center { get; }
        public ShapeLink[] RoadLinks { get; }

        public RoadNetworkCalculator(TileCache tiles, string roadShapeLocation, int radius, Point3D center)
        {
            Tiles = tiles;
            Radius = radius;
            Center = center;
            RoadLinks = ShapeLink.ReadLinks(roadShapeLocation, Center, radius).ToArray();

            center.Z = tiles.GetAltitude(center);

            // Initialize a PointUtm array that is to be (re)used as the vector of points from
            // the center to each of the points that should be calculated.
            _vector = new Point4D[(int)Math.Sqrt((long)radius * radius * 2L) + 1];
            for (var i = 0; i < _vector.Length; i++)
                _vector[i] = new Point4D(0, 0);

            _calc = new PathLossCalculator();
        }

        public int Calculate(ConsoleInformationPanel cip = null)
        {
            var calculations = 0;

            using (var pb = cip?.SetProgress("Calculating path loss", 0, RoadLinks.Length, true))
            {
                foreach (var link in RoadLinks)
                {
                    var linkCalcs = 0;
                    foreach (var c in link.Geometry)
                    {
                        if (Center.DistanceTo2D(c) > Radius)
                        {
                            cip?.Increment("Points outside of radius");
                            continue;
                        }

                        // Get the X,Y,Z vector from the center to these coordinates.
                        var vectorLength = Tiles.FillVector(_vector, Center.X, Center.Y, c.X, c.Y, withHeights: true);

                        // Calculate the loss for this point, and store it in the results matrix
                        c.M = _calc.CalculateLoss(_vector, 100, 2, vectorLength);
                        linkCalcs++;
                    }

                    pb?.Increment();
                    calculations += linkCalcs;
                    cip?.Increment("Points calculated", linkCalcs);
                }
            }

            return calculations;
        }

        public void SaveResults(string outputLocation, ConsoleInformationPanel cip = null)
        {
            var shp = new FeatureSet(FeatureType.Point);

            var table = shp.DataTable;
            table.Columns.Add("Loss", typeof(double));
            table.AcceptChanges();

            using(var pb = cip.SetProgress("Saving results", max:RoadLinks.Length))
                foreach (var link in RoadLinks)
                {
                    foreach (var c in link.Geometry)
                    {
                        if (double.IsNaN(c.M)) continue;
                        var feature = shp.AddFeature(new Point(new Coordinate(c.X, c.Y, c.Z)));
                        feature.DataRow["Loss"] = c.M;
                    }

                    pb?.Increment();
                }

            shp.SaveAs(outputLocation, true);
        }
    }
}
