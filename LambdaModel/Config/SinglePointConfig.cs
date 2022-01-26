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

        public (double rssi, Point4D<double>[] vector, ConsoleInformationPanelSnapshot) Run()
        {
            using (var cip = new ConsoleInformationPanel("Running single point signal loss calculations"))
            {
                var cache = Terrain.CreateCache(null);

                var vector = cache.GetAltitudeVector(BaseStation.Center, TargetCoordinates).ToArray();
                var loss = BaseStation.Calculator.CalculateLoss(vector, BaseStation.HeightAboveTerrain, ReceiverHeightAboveTerrain);
                var rssi = BaseStation.TotalTransmissionLevel - loss;

                return (rssi, vector, cip.GetSnapshot());
            }
        }
    }
}