using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ConsoleUtilities.ConsoleInfoPanel;
using DotSpatial.Topology;
using Extensions.ListExtensions;
using Extensions.Utilities.Statistics;
using LambdaModel.General;
using LambdaModel.PathLoss;
using LambdaModel.Stations;
using LambdaModel.Terrain.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LambdaModel.Tests.Validation
{
    [TestClass]
    public class Calculations : MobileNetworkPathLossCalculator
    {
        [TestMethod]
        public void MinPathLossCalculations()
        {
            var cip = new ConsoleInformationPanel();
            var tiles = new LocalTileCache(@"I:\\Jobb\\Lambda\\Tiles_512", 512, cip, 300, 100);
            var bs = new RoadLinkBaseStation(271327, 7040324, 100, 100_000);

            ShapeLink.ReadLinks(@"C:\\Code\\LambdaModel\\Data\\RoadNetwork\\2021-05-28_smaller.shp", new[] {bs});

            // Initialize a PointUtm array that is to be (re)used as the vector of points from
            // the center to each of the points that should be calculated.
            var _vector = new Point4D<double>[(int)Math.Sqrt((long)bs.MaxRadius * bs.MaxRadius * 2L) + 1];
            for (var i = 0; i < _vector.Length; i++)
                _vector[i] = new Point4D<double>(0, 0);

            var start = DateTime.Now;

            var stats = new IncrementalStatisticsCollection();

            foreach (var link in bs.Links.OrderBy(p=>bs.Center.DistanceTo2D(p.Cx,p.Cy)).Thin(5000))
            {
                for (var i = 0; i < link.Geometry.Length; i++)
                {
                    var c = link.Geometry[i];

                    // Get the X,Y,Z vector from the center to these coordinates.
                    var vectorLength = tiles.FillVector(_vector, bs.Center.X, bs.Center.Y, c.X, c.Y, withHeights: true);

                    // Calculate the loss for this point, and store it in the results matrix
                    //var angle = bs.AngleTo(c);
                    
                    var parameters = GetParameters(_vector, bs.HeightAboveTerrain, 2, vectorLength - 1);
                    stats.AddObservation(parameters.txi, "txi");
                    stats.AddObservation(parameters.txa, "txa");
                    stats.AddObservation(parameters.rxi, "rxi");
                    stats.AddObservation(parameters.rxa, "rxa");
                    stats.AddObservation(parameters.nobs, "nobs");

                    var distanceLoss = CalculateMinPossibleLoss(parameters.horizontalDistance, bs.HeightAboveTerrain);
                    stats.AddObservation(distanceLoss, "distanceLoss");
                    stats.AddObservation(CalculateLoss(bs.HeightAboveTerrain, parameters) - distanceLoss, "nonDistanceLoss");
                }
            }

            Console.WriteLine(stats.ToString());

            var secs = DateTime.Now.Subtract(start).TotalSeconds;
            Console.WriteLine($"Calculation time: {secs:n2} seconds.");
        }
    }
}
