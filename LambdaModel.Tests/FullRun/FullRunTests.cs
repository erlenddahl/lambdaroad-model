using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using LambdaModel.General;
using LambdaModel.PathLoss;
using LambdaModel.Terrain.Tiff;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LambdaModel.Tests.FullRun
{
    [TestClass]
    public class FullRunTests
    {
        [TestMethod]
        public void Run()
        {
            var geotiff = new GeoTiff(@"C:\Users\Erlend\Desktop\Søppel\2021-06-01 - Lambda-test\DOM\12-14\33-126-145.tif");
            
            // Use a station placed in the center of this map tile
            var stationCoordinates = new PointUtm(geotiff.StartX + geotiff.Width / 2d + 1500, geotiff.StartY - geotiff.Height / 2d, 0);
            stationCoordinates.Z = geotiff.GetAltitude(stationCoordinates);

            var vector = geotiff.GetAltitudeVector(stationCoordinates, stationCoordinates.Move(5000, 0)).ToArray();
            var calc = new PathLossCalculator();
            var start = DateTime.Now;
            for (var i = 2; i < vector.Length; i++)
            {
                var loss = calc.CalculateLoss(vector.Take(i).ToArray(), 100, 2);
            }

            var ms = DateTime.Now.Subtract(start).TotalMilliseconds;
            Console.WriteLine("Calculation time: " + ms);
        }
    }
}
