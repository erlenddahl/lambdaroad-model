using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Extensions.Utilities.Csv;
using LambdaModel.General;
using LambdaModel.PathLoss;
using LambdaModel.Stations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LambdaModel.Tests.Validation
{
    [TestClass]
    public class ValidationTests : MobileNetworkPathLossCalculator
    {
        private ValidationItem[] _data;
        private List<ValidationItem> _results;

        public class ValidationItem
        {
            public int Distance { get; set; }
            public double TerrainHeight { get; set; }
            public double RxA { get; set; }
            public double TxA { get; set; }
            public double RxI { get; set; }
            public double TxI { get; set; }
            public double Nobs { get; set; }
            public double PL1 { get; set; }
            public double PL2 { get; set; }
            public double PL3 { get; set; }
            public double PL4 { get; set; }
            public double RSRP1 { get; set; }
            public double RSRP2 { get; set; }
        }

        [TestInitialize]
        public void CalculateTestResults()
        {
            var reader = new CsvReader();
            _data = reader
                .ReadFile(@"..\..\..\..\Data\2022-02-28 - validation.csv")
                .Select(p => new ValidationItem
                {
                    Distance = int.Parse(p["distance from antenna"]), 
                    TerrainHeight = double.Parse(p["terrain height"]), 
                    RxA = double.Parse(p["rx_a"]),
                    TxA = double.Parse(p["tx_a"]),
                    RxI = double.Parse(p["rx_i"]),
                    TxI = double.Parse(p["tx_i"]),
                    Nobs = double.Parse(p["nobs"]),
                    PL1 = double.Parse(p["PL1"]),
                    PL2 = double.Parse(p["PL2"]),
                    RSRP1 = double.Parse(p["RSRP1"]),
                    RSRP2 = double.Parse(p["RSRP2"])
                })
                .ToArray();

            _results = new List<ValidationItem>();
            _results.Add(new ValidationItem());

            var txHeightAboveTerrain = 23;
            var rxHeightAboveTerrain = 2;

            var all = new MobileNetworkPathLossCalculator() { RegressionType = MobileNetworkRegressionType.All };
            var los = new MobileNetworkPathLossCalculator() {RegressionType = MobileNetworkRegressionType.LineOfSight};
            var nlos = new MobileNetworkPathLossCalculator() { RegressionType = MobileNetworkRegressionType.NoLineOfSight };
            var dynamic = new MobileNetworkPathLossCalculator() { RegressionType = MobileNetworkRegressionType.Dynamic };

            var path = _data.Select((p,i) => new Point4D<double>(0, i, p.TerrainHeight)).ToArray();
            for (var i = 1; i < path.Length; i++)
            {
                var features = GetParameters(path, txHeightAboveTerrain, rxHeightAboveTerrain, i);
                
                _results.Add(new ValidationItem()
                {
                    Distance = i,
                    Nobs = features.nobs,
                    RxA = features.rxa,
                    TxA = features.txa,
                    RxI = features.rxi,
                    TxI = features.txi,
                    PL1 = dynamic.CalculateLoss(path, txHeightAboveTerrain, rxHeightAboveTerrain, i),
                    PL2 = all.CalculateLoss(path, txHeightAboveTerrain, rxHeightAboveTerrain, i),
                    PL3 = los.CalculateLoss(path, txHeightAboveTerrain, rxHeightAboveTerrain, i),
                    PL4 = nlos.CalculateLoss(path, txHeightAboveTerrain, rxHeightAboveTerrain, i)
                });
            }
        }

        [TestMethod]
        public void SetupVerification()
        {
            Debug.WriteLine("ix;terrain;rx_a;tx_a;rx_i;tx_i;nobs;pl1;pl2;r_rx_a;r_tx_a;r_rx_i;r_tx_i;r_nobs;r_pl_dyn;r_pl_all;r_pl_los;r_pl_nlos");
            for (var i = 1; i < _data.Length; i++)
            {
                var d = _data[i];
                var r = _results[i];
                Debug.WriteLine(i + ";" + d.TerrainHeight + ";" + d.RxA + ";" + d.TxA + ";" + d.RxI + ";" + d.TxI + ";" + d.Nobs + ";" + d.PL1 + ";" + d.PL2
                                + ";" + r.RxA + ";" + r.TxA + ";" + r.RxI + ";" + r.TxI + ";" + r.Nobs + ";" + r.PL1 + ";" + r.PL2 + ";" + r.PL3 + ";" + r.PL4);
            }

            Assert.AreEqual(_data.Length, _results.Count);
        }

        [TestMethod]
        public void PathLossAll()
        {
            for (var i = 1; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i].PL2, _results[i].PL2, 0.01, "At index " + i);
            }
        }

        [TestMethod]
        public void Rxi()
        {
            for (var i = 1; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i].RxI, _results[i].RxI, "At index " + i);
            }
        }

        [TestMethod]
        public void Rxa()
        {
            for (var i = 1; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i].RxA, _results[i].RxA, 0.0001, "At index " + i);
            }
        }

        [TestMethod]
        public void Txi()
        {
            for (var i = 1; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i].TxI, _results[i].TxI, "At index " + i);
            }
        }

        [TestMethod]
        public void Txa()
        {
            for (var i = 1; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i].TxA, _results[i].TxA, 0.0001, "At index " + i);
            }
        }

        [TestMethod]
        public void Nobs()
        {
            for (var i = 1; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i].Nobs, _results[i].Nobs, "At index " + i);
            }
        }
    }
}