﻿using System;
using System.Diagnostics;
using System.Linq;
using ConsoleUtilities.ConsoleInfoPanel;
using DotSpatial.Topology.Utilities;
using Extensions.ListExtensions;
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
        public TerrainConfig Terrain { get; set; }
        public MobileNetworkRegressionType? MobileRegression { get; set; } = MobileNetworkRegressionType.All;
        public double ReceiverHeightAboveTerrain { get; set; }

        public object Run()
        {
            using (var cip = new ConsoleInformationPanel("Running single point signal loss calculations"))
            using (var cache = Terrain.CreateCache(cip))
            {
                BaseStation.Initialize();

                if (BaseStation.Calculator is MobileNetworkPathLossCalculator m && MobileRegression.HasValue)
                {
                    m.RegressionType = MobileRegression.Value;
                    cip.Set("Regression type", MobileRegression.Value.ToString());
                }
                
                cip.Set("Receiver height", ReceiverHeightAboveTerrain);

                var start = DateTime.Now;

                var vector = cache.GetAltitudeVector(BaseStation.Center, TargetCoordinates).ToArray();
                var loss = new double[vector.Length];
                var rsrp = new double[vector.Length];
                var a = BaseStation.AngleTo(TargetCoordinates);
                for (var i = 2; i < vector.Length; i++)
                {
                    var pl = BaseStation.Calculator.CalculateLoss(vector, BaseStation.HeightAboveTerrain, ReceiverHeightAboveTerrain, i - 1);
                    loss[i] = pl;
                    rsrp[i] = BaseStation.CalculateRsrpAtAngle(a, pl);
                }

                cip.Set("Calculation time", DateTime.Now.Subtract(start).TotalMilliseconds + "ms");

                var mc = 1000;
                return new
                {
                    rsrp = rsrp.Thin(mc),
                    loss = loss.Thin(mc),
                    vector = vector.Thin(mc).Select(p => new {p.X, p.Y, Z = p.Z < -30000 ? 0 : p.Z}),
                    distance = (int) Math.Round(vector.First().DistanceTo2D(vector.Last()), 0),
                    snapshot = cip.GetSnapshot(),
                    config = this
                };
            }
        }
    }
}