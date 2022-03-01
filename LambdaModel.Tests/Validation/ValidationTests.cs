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

            var path = _data.Select((p,i) => new Point4D<double>(0, i, p.TerrainHeight)).ToArray();
            for (var i = 1; i < path.Length; i++)
            {
                var modifiedPath = new Point4D<double>[path.Length];
                Array.Copy(path, modifiedPath, path.Length);
                modifiedPath[0] = modifiedPath[0].Offset(0, 0, txHeightAboveTerrain);
                modifiedPath[i] = modifiedPath[i].Offset(0, 0, rxHeightAboveTerrain);
                var features = GetParameters(modifiedPath, i);

                var loss = CalculateLoss(path, txHeightAboveTerrain, rxHeightAboveTerrain, i);

                _results.Add(new ValidationItem()
                {
                    Distance = i,
                    Nobs = features.nobs,
                    RxA = features.rxa,
                    TxA = features.txa,
                    RxI = features.rxi,
                    TxI = features.txi,
                    PL1 = loss
                });
            }
        }

        [TestMethod]
        public void SetupVerification()
        {
            Debug.WriteLine("ix;terrain;rx_a;tx_a;rx_i;tx_i;nobs;pl;r_rx_a;r_tx_a;r_rx_i;r_tx_i;r_nobs;r_pl");
            for (var i = 1; i < _data.Length; i++)
            {
                var d = _data[i];
                var r = _results[i];
                Debug.WriteLine(i + ";" + d.TerrainHeight + ";" + d.RxA + ";" + d.TxA + ";" + d.RxI + ";" + d.TxI + ";" + d.Nobs + ";" + d.PL1
                                + ";" + r.RxA + ";" + r.TxA + ";" + r.RxI + ";" + r.TxI + ";" + r.Nobs + ";" + r.PL1);
            }

            Assert.AreEqual(_data.Length, _results.Count);
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