﻿using System.Diagnostics;
using System.Linq;
using LambdaModel.General;
using LambdaModel.Terrain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LambdaModel.Tests.Terrain.TileCacheTests
{
    [TestClass]
    public class GetAltitudeVectorTests
    {
        private TileCache _tiles;

        [TestInitialize]
        public void Init()
        {
            _tiles = new TileCache(@"..\..\..\..\Data\Testing\CacheTest");
        }

        [TestMethod]
        public void RandomCoordinateTests()
        {
            var v = _tiles.GetAltitudeVector(new PointUtm(290344, 7100903), new PointUtm(290763, 7101079));

            foreach (var h in v)
                Debug.WriteLine(h.Z);

            // Verified visually using hoydedata.no/LaserInnsyn
            var correct = new[] { 440.5254211, 440.5042725, 440.5888977, 440.5932312, 440.6075134, 440.6242676, 440.6047668, 440.715271, 440.6665649, 440.6436768, 440.5066833, 440.4871216, 440.4833984, 440.4700012, 440.4709778, 440.4502563, 440.4942017, 440.5097046, 440.5320129, 440.5142822, 440.5302734, 440.4311218, 440.4759827, 440.4710388, 440.5075684, 440.5651245, 440.6289063, 440.6765442, 440.6938171, 440.5549011, 440.4568787, 440.5676575, 440.6474304, 440.6302185, 440.6678467, 440.6694336, 440.7626648, 440.9172363, 441.3820496, 441.659729, 441.9537354, 442.4492798, 442.6184692, 443.0649414, 443.6345825, 443.6345825, 443.9975586, 446.4127197, 446.4725952, 445.4029236, 445.5943298, 445.7797546, 445.7744751, 446.5574341, 446.8076782, 446.5892334, 447.028717, 447.868866, 447.868866, 447.390564, 447.2434387, 448.0238647, 448.359436, 448.8856201, 449.2862549, 449.4649048, 450.2876587, 450.770813, 451.0423279, 451.5843201, 451.6633301, 451.6633301, 452.0464478, 452.3125305, 452.8110962, 453.1500854, 453.2593384, 453.7861633, 454.1156921, 454.5000305, 454.7312012, 455.0428467, 455.4213562, 455.5648499, 455.6256409, 455.8036194, 456.0029907, 456.2332764, 456.3610535, 456.5247803, 456.4602966, 456.6512146, 456.8364868, 456.7739563, 456.7810059, 457.4356079, 457.4994202, 457.7394104, 457.8025208, 457.9673767, 458.4619751, 458.6239014, 459.1836853, 459.4223328, 459.5201721, 459.9465027, 460.2604675, 460.6161194, 461.1940002, 461.1940002, 461.6977539, 461.8019714, 461.9439392, 462.3915405, 462.6771545, 463.1509094, 463.4053955, 463.7011414, 464.1095276, 464.1143799, 464.1551819, 464.1867065, 464.1867065, 464.3340454, 464.3515625, 464.4525146, 464.868042, 464.9775085, 465.1630859, 465.3094788, 465.4477234, 465.8338013, 466.0256653, 466.4806519, 466.5880432, 466.5880432, 466.9263, 466.8946228, 466.9176941, 466.7745972, 466.8706055, 466.9255981, 467.0345154, 467.1508789, 467.1686707, 467.2223511, 467.6165466, 467.6149597, 467.6149597, 467.8227234, 468.1091614, 468.2540894, 468.6228333, 468.6938477, 468.8510437, 468.9871826, 469.1846313, 469.386261, 469.425354, 469.7425537, 470.0326538, 470.0326538, 470.3353577, 470.428009, 470.7295837, 470.8199463, 470.9325562, 471.2869568, 471.4741516, 471.6140137, 471.9781799, 472.0620728, 472.2281494, 472.2005615, 472.2005615, 472.25, 472.2514954, 472.466217, 472.5026245, 472.4580078, 472.6235046, 472.6454163, 472.7210999, 473.024353, 472.7651978, 472.6061096, 472.6061096, 472.5397339, 472.4411621, 472.5087891, 472.5031128, 472.4789734, 472.4500427, 472.3178406, 472.3930969, 472.4332275, 472.4360046, 472.3124084, 472.1326904, 472.1326904, 471.8994751, 472.0966797, 472.0039978, 471.9986267, 471.8461304, 471.771637, 471.7644043, 471.7627563, 471.7671509, 471.7927246, 471.8586426, 471.9917603, 471.9917603, 472.1270752, 471.9549561, 472.0305481, 472.0431519, 472.1256104, 472.0677185, 471.9890747, 472.0161438, 472.0027771, 472.1134949, 472.0732727, 471.9631042, 471.9631042, 471.9390869, 471.9107971, 471.8838806, 471.8297119, 471.8180542, 471.7440796, 471.6589661, 471.6164856, 471.5899963, 471.4211426, 471.2603455, 471.0999756, 471.0999756, 470.9236755, 470.7348328, 470.4285889, 470.2561035, 470.1129761, 469.7989197, 469.4657898, 468.9994507, 468.4833679, 468.1643066, 467.6941833, 467.531311, 466.8089294, 466.1963501, 465.5116272, 464.8814697, 463.5487061, 462.3627319, 461.5457764, 461.2090454, 460.8172607, 460.6105347, 460.4690552, 460.1834412, 460.2337952, 459.9532776, 459.6758728, 459.4563904, 458.9096069, 458.6358032, 458.1779785, 457.9665222, 457.7441406, 457.5715942, 457.309082, 457.1302795, 456.9266357, 456.8616638, 456.7020264, 456.5663452, 456.4347534, 456.1715698, 455.8972168, 455.6420288, 455.2924194, 455.1079712, 454.9410095, 454.5485535, 454.4464417, 454.3706055, 454.3706055, 454.0134888, 453.395752, 453.1308899, 453.0908508, 453.0418091, 452.9667664, 452.8219299, 452.6715698, 452.4750061, 452.4353027, 452.374115, 452.3041992, 452.3041992, 452.0903625, 451.9361572, 451.9619751, 452.0069885, 452.0678711, 452.0742188, 451.9835205, 451.7979126, 451.8266296, 451.5533752, 451.4993591, 451.1959229, 451.1225891, 450.9516907, 450.313446, 449.9815063, 449.7505188, 449.5771179, 449.1379089, 448.4507141, 448.0905457, 447.8744507, 447.4768066, 447.5401306, 447.3916321, 447.4111633, 447.3981018, 447.4095459, 447.4161377, 447.3950806, 447.4094238, 447.4048157, 447.3826294, 447.4039612, 447.4776001, 447.4347839, 447.4768066, 447.4659729, 447.4935608, 447.5114136, 447.4359436, 447.4220276, 447.3246155, 447.3313293, 447.3316345, 447.1672363, 447.1497192, 446.9396667, 446.9584351, 446.9526978, 446.845459, 446.7660217, 446.7100525, 446.6352844, 446.5467529, 446.3076172, 446.2852783, 446.2749329, 446.1140137, 446.0111084, 446.0218506, 446.3338623, 446.1801147, 445.9549866, 446.324585, 445.5359802, 445.4253235, 445.3799133, 445.2181396, 445.2519226, 445.3244629, 445.3358765, 445.3251343, 445.3091431, 445.2773438, 445.21875, 445.0542603, 444.986145, 444.8349915, 444.8779602, 444.8710022, 444.6289978, 444.6369019, 444.5004578, 444.2792664, 444.1361389, 443.914978, 443.8536682, 443.8536682, 443.8920593, 443.8520508, 443.6369629, 443.612854, 443.591217, 443.3763428, 443.1217957, 443.2158203, 443.4564514, 443.4978333, 443.3426208, 443.2924194, 443.2924194, 443.3086243, 443.4458008, 443.6064453, 443.7070007, 443.7628479, 443.7089844, 443.7506409, 443.6809692, 443.6859436, 443.6998291, 443.7113342, 443.8162537, 443.8162537, 443.7587891, 443.744812, 443.71521, 443.8196716, 443.9197388, 443.8840027, 443.9611511, 443.8434143, 443.9116821, 444.004303, 444.2155762, 444.2263794, 444.1534424, 444.2370911, 444.29422, 444.3421326, 444.4516602, 444.566925, 444.9180603, 445.0230713, 445.1728821, 445.2666321, 445.3458862, 445.7027588, 445.8120117, 445.8159485, 445.8054504, 445.799408, 445.8210449, 445.899353, 445.9605713, 445.9533691, 446.0332031, 445.9931335, 445.9686279, 445.9229126, 445.8745728 }
                .Select(p => (float)p).ToArray();

            Assert.AreEqual(correct.Length, v.Count);

            for (var i = 0; i < v.Count; i++)
                Assert.AreEqual(correct[i], v[i].Z, 0.001);
        }

        [TestMethod]
        public void LeftEdge()
        {
            var v = _tiles.GetAltitudeVector(new PointUtm(290425, 7100995), new PointUtm(290425, 7100995 + 99));

            foreach (var h in v)
                Debug.WriteLine(h.Z);
            
            // Verified visually using hoydedata.no/LaserInnsyn
            var correct = new[] { 462.562, 462.544, 462.591, 462.572, 462.57, 462.566, 462.529, 462.574, 462.572, 462.642, 462.517, 462.339, 462.245, 462.045, 461.905, 461.851, 461.929, 461.786, 461.742, 461.962, 461.779, 461.811, 461.867, 461.838, 461.73, 461.51, 461.483, 461.346, 461.235, 461.153, 461.003, 460.651, 460.57, 460.536, 459.823, 459.582, 459.444, 459.397, 459.384, 459.328, 459.258, 459.267, 459.304, 459.284, 458.886, 458.699, 458.611, 458.625, 458.704, 458.763, 458.806, 459.006, 459.049, 459.172, 459.349, 459.558, 459.631, 459.618, 459.532, 459.527, 459.566, 459.611, 459.701, 459.805, 459.814, 459.785, 459.768, 459.693, 459.555, 459.547, 459.415, 459.316, 459.249, 459.193, 459.099, 459.048, 458.92, 458.722, 458.597, 458.282, 458.104, 458.019, 458.002, 457.991, 457.799, 457.262, 456.893, 456.608, 456.114, 455.822, 455.636, 455.32, 455.238, 455.092, 454.932, 454.777, 454.658, 454.332, 453.936, 453.667, }
                .Select(p => (float) p).ToArray();

            Assert.AreEqual(correct.Length, v.Count);

            for (var i = 0; i < v.Count; i++)
                Assert.AreEqual(correct[i], v[i].Z, 0.001);
        }

        [TestMethod]
        public void BottomEdge()
        {
            var v = _tiles.GetAltitudeVector(new PointUtm(290425, 7100995), new PointUtm(290425 + 99, 7100995));

            foreach (var h in v)
                Debug.WriteLine(h.Z);

            // Verified visually using hoydedata.no/LaserInnsyn
            var correct = new[] { 462.5619507, 462.7725525, 463.044281, 463.2858276, 463.5961609, 463.9570313, 464.2276001, 464.5385742, 464.6919861, 464.8699036, 465.0542908, 465.2119446, 465.2680359, 465.2505798, 465.4008789, 465.5308533, 465.6977539, 465.9275513, 466.0993652, 466.4350891, 466.8096619, 467.0539551, 467.3977661, 467.631958, 467.8251648, 467.9779968, 468.0511475, 468.1373901, 468.2877502, 468.4060974, 468.4510803, 468.6258545, 468.8323059, 469.1273804, 469.3821411, 469.5527344, 469.7662354, 470.0121765, 470.1600952, 470.2122803, 470.3950195, 470.4993896, 470.4816895, 470.5072021, 470.512146, 470.650238, 470.8219604, 471.00177, 471.2116699, 471.4023438, 471.6483459, 471.7663269, 471.6766052, 471.5773621, 471.5976563, 471.8383484, 472.1517029, 472.3072205, 472.3293762, 472.3859863, 472.3653259, 472.4231873, 472.6222839, 472.7841187, 472.9602661, 473.1721497, 473.3139343, 473.3959045, 473.459198, 473.5235291, 473.5831299, 473.6485291, 473.7088623, 473.7433472, 473.7823181, 473.8311462, 473.8544922, 473.8916931, 474.0163269, 474.1322021, 474.2390442, 474.3103943, 474.3155212, 474.2652283, 474.1477356, 474.0698242, 473.9918518, 473.8992615, 473.7828979, 473.577179, 473.3905945, 473.2559204, 473.1158752, 473.119812, 473.1210632, 473.1559448, 473.1825256, 473.2121887, 473.1807861, 473.1006775 }
                .Select(p => (float)p).ToArray();

            Assert.AreEqual(correct.Length, v.Count);

            for (var i = 0; i < v.Count; i++)
                Assert.AreEqual(correct[i], v[i].Z, 0.001);
        }

        [TestMethod]
        public void TopEdge()
        {
            var v = _tiles.GetAltitudeVector(new PointUtm(290425, 7100995 + 99), new PointUtm(290425 + 99, 7100995 + 99));

            foreach (var h in v)
                Debug.WriteLine(h.Z);

            // Verified visually using hoydedata.no/LaserInnsyn
            var correct = new[] { 453.6667785644531, 453.65545654296875, 453.72637939453125, 453.83782958984375, 453.8252258300781, 453.8349914550781, 453.9550476074219, 454.13800048828125, 454.4871826171875, 454.6706237792969, 454.61328125, 454.77252197265625, 454.8779602050781, 454.9288024902344, 455.0129089355469, 455.0821533203125, 455.1436462402344, 455.2997131347656, 455.4102783203125, 455.451904296875, 455.534912109375, 455.6550598144531, 455.75323486328125, 455.78143310546875, 455.7600402832031, 455.7566223144531, 455.630615234375, 455.64404296875, 455.76947021484375, 455.8865661621094, 456.07940673828125, 456.1148986816406, 456.0177001953125, 456.23150634765625, 456.5251159667969, 456.7765808105469, 456.9591369628906, 457.19146728515625, 457.3645324707031, 457.54852294921875, 457.5195617675781, 457.540283203125, 457.49609375, 457.4402770996094, 457.3959655761719, 457.25140380859375, 457.1854248046875, 457.0092468261719, 457.0605163574219, 457.2371520996094, 457.25787353515625, 457.1833190917969, 457.1059875488281, 457.15875244140625, 457.20941162109375, 457.31829833984375, 457.3871765136719, 457.3517150878906, 457.2954406738281, 457.203369140625, 457.1581726074219, 457.0372009277344, 456.91302490234375, 456.6705017089844, 457.01104736328125, 456.5006408691406, 455.5909118652344, 455.617919921875, 454.833984375, 454.1697692871094, 453.9722900390625, 454.1636657714844, 453.86480712890625, 453.5457763671875, 453.20159912109375, 453.0281066894531, 453.07305908203125, 452.9806823730469, 453.0047607421875, 453.14208984375, 453.2882385253906, 453.4344177246094, 453.71417236328125, 453.8348693847656, 453.92987060546875, 453.99932861328125, 453.8685302734375, 453.9478759765625, 454.0813293457031, 453.8841857910156, 453.8730773925781, 453.9593811035156, 453.93316650390625, 454.1813049316406, 454.02801513671875, 454.045166015625, 454.0122375488281, 453.3480224609375, 453.0684509277344, 453.0776062011719 }
                .Select(p => (float)p).ToArray();

            Assert.AreEqual(correct.Length, v.Count);

            for (var i = 0; i < v.Count; i++)
                Assert.AreEqual(correct[i], v[i].Z, 0.001);
        }

        [TestMethod]
        public void RightEdge()
        {
            var v = _tiles.GetAltitudeVector(new PointUtm(290425 + 99, 7100995), new PointUtm(290425 + 99, 7100995 + 99));

            foreach (var h in v)
                Debug.WriteLine(h.Z);

            // Verified visually using hoydedata.no/LaserInnsyn
            var correct = new[] { 473.1006774902344, 473.0924987792969, 473.06195068359375, 473.0021057128906, 472.85296630859375, 472.75433349609375, 472.7338562011719, 472.6384582519531, 472.62420654296875, 472.6849670410156, 472.64837646484375, 472.5372619628906, 472.3709716796875, 472.14605712890625, 472.013427734375, 471.9720153808594, 471.9167785644531, 471.8307800292969, 471.7252502441406, 471.5909118652344, 471.4437561035156, 471.292724609375, 471.15460205078125, 471.0333557128906, 470.9000244140625, 470.7734375, 470.70184326171875, 470.6458740234375, 470.5731201171875, 470.4518127441406, 470.20428466796875, 469.9656982421875, 469.8500061035156, 469.8182067871094, 469.7867126464844, 469.6453857421875, 469.580078125, 469.5436706542969, 469.4551086425781, 469.4830322265625, 469.4210205078125, 469.2380065917969, 469.0223083496094, 468.8211975097656, 468.5966796875, 468.28363037109375, 467.99359130859375, 467.7746887207031, 467.6334533691406, 467.4512634277344, 467.1656188964844, 466.8134765625, 466.354248046875, 465.77362060546875, 465.35467529296875, 465.04132080078125, 464.5851135253906, 463.962646484375, 463.3188781738281, 462.8249816894531, 462.2629089355469, 461.5954284667969, 461.1471252441406, 460.8621826171875, 460.66259765625, 460.4260559082031, 460.087646484375, 459.8024597167969, 459.4559631347656, 459.2441711425781, 459.1297607421875, 458.9494323730469, 458.7134094238281, 458.4576110839844, 458.2691650390625, 458.1335754394531, 457.91644287109375, 457.6450500488281, 457.5076904296875, 457.4425964355469, 457.3134460449219, 457.44757080078125, 457.4613037109375, 457.3171081542969, 456.93511962890625, 456.67578125, 456.4121398925781, 456.1142883300781, 455.9311218261719, 455.5765686035156, 455.1949768066406, 454.9576721191406, 454.7405700683594, 454.49737548828125, 454.181640625, 454.0858154296875, 453.89483642578125, 453.7230529785156, 453.4411926269531, 453.0776062011719 }
                .Select(p => (float)p).ToArray();

            Assert.AreEqual(correct.Length, v.Count);

            for (var i = 0; i < v.Count; i++)
                Assert.AreEqual(correct[i], v[i].Z, 0.001);
        }
    }
}
