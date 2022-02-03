using System;
using System.Diagnostics;
using System.Linq;
using ConsoleUtilities.ConsoleInfoPanel;
using DotSpatial.Topology.Utilities;
using LambdaModel.General;
using LambdaModel.PathLoss;
using LambdaModel.Stations;
using no.sintef.SpeedModule.Geometry.SimpleStructures;

namespace LambdaModel.Config
{
    public class SinglePointConfig
    {
        public BaseStation BaseStation { get; set; }
        public Point3D TargetCoordinates { get; set; }
        public double ReceiverHeightAboveTerrain { get; set; }
        public TerrainConfig Terrain { get; set; }

        public object Run()
        {
            using (var cip = new ConsoleInformationPanel("Running single point signal loss calculations"))
            using (var cache = Terrain.CreateCache(cip))
            {
                BaseStation.Initialize();

                var start = DateTime.Now;

                var vector = cache.GetAltitudeVector(BaseStation.Center, TargetCoordinates).ToArray();
                var loss = BaseStation.Calculator.CalculateLoss(vector, BaseStation.HeightAboveTerrain, ReceiverHeightAboveTerrain);
                var rssi = BaseStation.TotalTransmissionLevel - loss;

                cip.Set("Calculation time", DateTime.Now.Subtract(start).TotalMilliseconds + "ms");

                return new {rssi, vector = vector.Select(p => new {p.X, p.Y, p.Z}), snapshot = cip.GetSnapshot()};
            }
        }
    }
}