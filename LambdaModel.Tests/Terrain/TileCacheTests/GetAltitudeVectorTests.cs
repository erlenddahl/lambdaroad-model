﻿using System.Diagnostics;
using System.Linq;
using LambdaModel.General;
using LambdaModel.Terrain;
using LambdaModel.Terrain.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using no.sintef.SpeedModule.Geometry.SimpleStructures;

namespace LambdaModel.Tests.Terrain.TileCacheTests
{
    [TestClass]
    public class GetAltitudeVectorTests
    {
        private OnlineTileCache _tiles;

        [TestInitialize]
        public void Init()
        {
            _tiles = new OnlineTileCache(@"..\..\..\..\Data\Testing\CacheTest");
        }

        [TestMethod]
        public void RandomCoordinateTests()
        {
            var v = _tiles.GetAltitudeVector(new Point3D(290344, 7100903), new Point3D(290763, 7101079));

            foreach (var h in v)
                Debug.WriteLine(h.Z);

            // Verified visually using hoydedata.no/LaserInnsyn
            var correct = new[] { 440.5254211425781, 440.5042724609375, 440.5888977050781, 440.5932312011719, 440.6075134277344, 440.624267578125, 440.6047668457031, 440.71527099609375, 440.66656494140625, 440.6436767578125, 440.5066833496094, 440.48712158203125, 440.4833984375, 440.4700012207031, 440.4709777832031, 440.45025634765625, 440.49420166015625, 440.50970458984375, 440.5320129394531, 440.5142822265625, 440.5302734375, 440.4311218261719, 440.4759826660156, 440.4710388183594, 440.507568359375, 440.56512451171875, 440.62890625, 440.6765441894531, 440.6938171386719, 440.5549011230469, 440.4568786621094, 440.5676574707031, 440.6474304199219, 440.6302185058594, 440.6678466796875, 440.66943359375, 440.7626647949219, 440.917236328125, 441.3820495605469, 441.65972900390625, 441.9537353515625, 442.44927978515625, 442.61846923828125, 443.06494140625, 443.63458251953125, 443.63458251953125, 443.99755859375, 446.4127197265625, 446.47259521484375, 445.4029235839844, 445.5943298339844, 445.7797546386719, 445.77447509765625, 446.55743408203125, 446.80767822265625, 446.5892333984375, 447.0287170410156, 447.8688659667969, 447.8688659667969, 447.39056396484375, 447.2434387207031, 448.02386474609375, 448.35943603515625, 448.8856201171875, 449.2862548828125, 449.46490478515625, 450.28765869140625, 450.77081298828125, 451.0423278808594, 451.5843200683594, 451.663330078125, 451.663330078125, 452.04644775390625, 452.3125305175781, 452.81109619140625, 453.15008544921875, 453.25933837890625, 453.7861633300781, 454.1156921386719, 454.5000305175781, 454.731201171875, 455.0428466796875, 455.4213562011719, 455.5648498535156, 455.6256408691406, 455.8036193847656, 456.00299072265625, 456.2332763671875, 456.3610534667969, 456.5247802734375, 456.4602966308594, 456.6512145996094, 456.83648681640625, 456.7739562988281, 456.781005859375, 457.43560791015625, 457.4994201660156, 457.7394104003906, 457.8025207519531, 457.9673767089844, 458.46197509765625, 458.6239013671875, 459.1836853027344, 459.4223327636719, 459.5201721191406, 459.9465026855469, 460.2604675292969, 460.6161193847656, 461.1940002441406, 461.1940002441406, 461.69775390625, 461.8019714355469, 461.9439392089844, 462.39154052734375, 462.6771545410156, 463.1509094238281, 463.4053955078125, 463.7011413574219, 464.1095275878906, 464.1143798828125, 464.1551818847656, 464.18670654296875, 464.18670654296875, 464.33404541015625, 464.3515625, 464.4525146484375, 464.8680419921875, 464.9775085449219, 465.1630859375, 465.3094787597656, 465.4477233886719, 465.83380126953125, 466.0256652832031, 466.48065185546875, 466.5880432128906, 466.5880432128906, 466.9263000488281, 466.8946228027344, 466.9176940917969, 466.77459716796875, 466.87060546875, 466.92559814453125, 467.0345153808594, 467.15087890625, 467.1686706542969, 467.22235107421875, 467.6165466308594, 467.6149597167969, 467.6149597167969, 467.8227233886719, 468.1091613769531, 468.25408935546875, 468.6228332519531, 468.69384765625, 468.8510437011719, 468.9871826171875, 469.18463134765625, 469.3862609863281, 469.42535400390625, 469.7425537109375, 470.03265380859375, 470.03265380859375, 470.3353576660156, 470.4280090332031, 470.7295837402344, 470.8199462890625, 470.93255615234375, 471.2869567871094, 471.4741516113281, 471.614013671875, 471.9781799316406, 472.06207275390625, 472.2281494140625, 472.2005615234375, 472.2005615234375, 472.25, 472.2514953613281, 472.4662170410156, 472.50262451171875, 472.4580078125, 472.6235046386719, 472.6454162597656, 472.7210998535156, 473.02435302734375, 472.76519775390625, 472.6061096191406, 472.6061096191406, 472.53973388671875, 472.441162109375, 472.5087890625, 472.50311279296875, 472.4789733886719, 472.4500427246094, 472.3178405761719, 472.3930969238281, 472.4332275390625, 472.4360046386719, 472.3124084472656, 472.1326904296875, 472.1326904296875, 471.89947509765625, 472.0966796875, 472.0039978027344, 471.9986267089844, 471.84613037109375, 471.7716369628906, 471.764404296875, 471.76275634765625, 471.76715087890625, 471.792724609375, 471.858642578125, 471.99176025390625, 471.99176025390625, 472.1270751953125, 471.9549560546875, 472.0305480957031, 472.04315185546875, 472.1256103515625, 472.0677185058594, 471.98907470703125, 472.0161437988281, 472.0027770996094, 472.1134948730469, 472.0732727050781, 471.9631042480469, 471.9631042480469, 471.9390869140625, 471.9107971191406, 471.8838806152344, 471.8297119140625, 471.81805419921875, 471.74407958984375, 471.6589660644531, 471.6164855957031, 471.5899963378906, 471.421142578125, 471.2603454589844, 471.0999755859375, 471.0999755859375, 470.9236755371094, 470.7348327636719, 470.4285888671875, 470.256103515625, 470.11297607421875, 469.7989196777344, 469.4657897949219, 468.99945068359375, 468.4833679199219, 468.164306640625, 467.6941833496094, 467.53131103515625, 466.8089294433594, 466.19635009765625, 465.5116271972656, 464.8814697265625, 463.5487060546875, 462.36273193359375, 461.5457763671875, 461.20904541015625, 460.8172607421875, 460.61053466796875, 460.46905517578125, 460.1834411621094, 460.2337951660156, 459.9532775878906, 459.6758728027344, 459.4563903808594, 458.90960693359375, 458.63580322265625, 458.177978515625, 457.9665222167969, 457.744140625, 457.57159423828125, 457.30908203125, 457.1302795410156, 456.9266357421875, 456.8616638183594, 456.7020263671875, 456.56634521484375, 456.43475341796875, 456.17156982421875, 455.897216796875, 455.64202880859375, 455.29241943359375, 455.10797119140625, 454.9410095214844, 454.5485534667969, 454.4464416503906, 454.37060546875, 454.37060546875, 454.01348876953125, 453.395751953125, 453.1308898925781, 453.0908508300781, 453.04180908203125, 452.9667663574219, 452.8219299316406, 452.67156982421875, 452.4750061035156, 452.435302734375, 452.3741149902344, 452.30419921875, 452.30419921875, 452.0903625488281, 451.9361572265625, 451.96197509765625, 452.0069885253906, 452.06787109375, 452.07421875, 451.9835205078125, 451.79791259765625, 451.8266296386719, 451.5533752441406, 451.4993591308594, 451.1959228515625, 451.1225891113281, 450.9516906738281, 450.3134460449219, 449.98150634765625, 449.7505187988281, 449.5771179199219, 449.1379089355469, 448.4507141113281, 448.0905456542969, 447.87445068359375, 447.476806640625, 447.5401306152344, 447.3916320800781, 447.4111633300781, 447.3981018066406, 447.4095458984375, 447.4161376953125, 447.39508056640625, 447.409423828125, 447.4048156738281, 447.38262939453125, 447.4039611816406, 447.47760009765625, 447.4347839355469, 447.476806640625, 447.4659729003906, 447.4935607910156, 447.51141357421875, 447.4359436035156, 447.4220275878906, 447.3246154785156, 447.3313293457031, 447.3316345214844, 447.167236328125, 447.14971923828125, 446.9396667480469, 446.95843505859375, 446.95269775390625, 446.845458984375, 446.7660217285156, 446.7100524902344, 446.6352844238281, 446.5467529296875, 446.3076171875, 446.2852783203125, 446.2749328613281, 446.114013671875, 446.0111083984375, 446.0218505859375, 446.3338623046875, 446.18011474609375, 445.9549865722656, 446.3245849609375, 445.5359802246094, 445.4253234863281, 445.3799133300781, 445.2181396484375, 445.2519226074219, 445.324462890625, 445.33587646484375, 445.32513427734375, 445.30914306640625, 445.27734375, 445.21875, 445.05426025390625, 444.98614501953125, 444.8349914550781, 444.8779602050781, 444.8710021972656, 444.6289978027344, 444.63690185546875, 444.5004577636719, 444.2792663574219, 444.1361389160156, 443.91497802734375, 443.8536682128906, 443.8536682128906, 443.8920593261719, 443.85205078125, 443.636962890625, 443.61285400390625, 443.5912170410156, 443.3763427734375, 443.1217956542969, 443.2158203125, 443.4564514160156, 443.4978332519531, 443.3426208496094, 443.29241943359375, 443.29241943359375, 443.3086242675781, 443.44580078125, 443.6064453125, 443.7070007324219, 443.7628479003906, 443.708984375, 443.7506408691406, 443.68096923828125, 443.6859436035156, 443.6998291015625, 443.7113342285156, 443.8162536621094, 443.8162536621094, 443.7587890625, 443.74481201171875, 443.7152099609375, 443.8196716308594, 443.91973876953125, 443.8840026855469, 443.9611511230469, 443.8434143066406, 443.91168212890625, 444.0043029785156, 444.215576171875, 444.22637939453125, 444.1534423828125, 444.2370910644531, 444.2942199707031, 444.3421325683594, 444.45166015625, 444.5669250488281, 444.9180603027344, 445.0230712890625, 445.1728820800781, 445.2666320800781, 445.34588623046875, 445.7027587890625, 445.81201171875, 445.8159484863281, 445.8054504394531, 445.7994079589844, 445.821044921875, 445.89935302734375, 445.9605712890625, 445.953369140625, 446.033203125, 445.9931335449219, 445.9686279296875, 445.92291259765625, 445.87457275390625 }
                .Select(p => (float)p).ToArray();

            Assert.AreEqual(correct.Length, v.Length);

            for (var i = 0; i < v.Length; i++)
                Assert.AreEqual(correct[i], v[i].Z, 0.001);
        }

        [TestMethod]
        public void LeftEdge()
        {
            var v = _tiles.GetAltitudeVector(new Point3D(290425, 7100995), new Point3D(290425, 7100995 + 99));

            foreach (var h in v)
                Debug.WriteLine(h.Z);
            
            // Verified visually using hoydedata.no/LaserInnsyn
            var correct = new[] { 462.562, 462.544, 462.591, 462.572, 462.57, 462.566, 462.529, 462.574, 462.572, 462.642, 462.517, 462.339, 462.245, 462.045, 461.905, 461.851, 461.929, 461.786, 461.742, 461.962, 461.779, 461.811, 461.867, 461.838, 461.73, 461.51, 461.483, 461.346, 461.235, 461.153, 461.003, 460.651, 460.57, 460.536, 459.823, 459.582, 459.444, 459.397, 459.384, 459.328, 459.258, 459.267, 459.304, 459.284, 458.886, 458.699, 458.611, 458.625, 458.704, 458.763, 458.806, 459.006, 459.049, 459.172, 459.349, 459.558, 459.631, 459.618, 459.532, 459.527, 459.566, 459.611, 459.701, 459.805, 459.814, 459.785, 459.768, 459.693, 459.555, 459.547, 459.415, 459.316, 459.249, 459.193, 459.099, 459.048, 458.92, 458.722, 458.597, 458.282, 458.104, 458.019, 458.002, 457.991, 457.799, 457.262, 456.893, 456.608, 456.114, 455.822, 455.636, 455.32, 455.238, 455.092, 454.932, 454.777, 454.658, 454.332, 453.936, 453.667, }
                .Select(p => (float) p).ToArray();

            Assert.AreEqual(correct.Length, v.Length);

            for (var i = 0; i < v.Length; i++)
                Assert.AreEqual(correct[i], v[i].Z, 0.001);
        }

        [TestMethod]
        public void BottomEdge()
        {
            var v = _tiles.GetAltitudeVector(new Point3D(290425, 7100995), new Point3D(290425 + 99, 7100995));

            foreach (var h in v)
                Debug.WriteLine(h.Z);

            // Verified visually using hoydedata.no/LaserInnsyn
            var correct = new[] { 462.5619507, 462.7725525, 463.044281, 463.2858276, 463.5961609, 463.9570313, 464.2276001, 464.5385742, 464.6919861, 464.8699036, 465.0542908, 465.2119446, 465.2680359, 465.2505798, 465.4008789, 465.5308533, 465.6977539, 465.9275513, 466.0993652, 466.4350891, 466.8096619, 467.0539551, 467.3977661, 467.631958, 467.8251648, 467.9779968, 468.0511475, 468.1373901, 468.2877502, 468.4060974, 468.4510803, 468.6258545, 468.8323059, 469.1273804, 469.3821411, 469.5527344, 469.7662354, 470.0121765, 470.1600952, 470.2122803, 470.3950195, 470.4993896, 470.4816895, 470.5072021, 470.512146, 470.650238, 470.8219604, 471.00177, 471.2116699, 471.4023438, 471.6483459, 471.7663269, 471.6766052, 471.5773621, 471.5976563, 471.8383484, 472.1517029, 472.3072205, 472.3293762, 472.3859863, 472.3653259, 472.4231873, 472.6222839, 472.7841187, 472.9602661, 473.1721497, 473.3139343, 473.3959045, 473.459198, 473.5235291, 473.5831299, 473.6485291, 473.7088623, 473.7433472, 473.7823181, 473.8311462, 473.8544922, 473.8916931, 474.0163269, 474.1322021, 474.2390442, 474.3103943, 474.3155212, 474.2652283, 474.1477356, 474.0698242, 473.9918518, 473.8992615, 473.7828979, 473.577179, 473.3905945, 473.2559204, 473.1158752, 473.119812, 473.1210632, 473.1559448, 473.1825256, 473.2121887, 473.1807861, 473.1006775 }
                .Select(p => (float)p).ToArray();

            Assert.AreEqual(correct.Length, v.Length);

            for (var i = 0; i < v.Length; i++)
                Assert.AreEqual(correct[i], v[i].Z, 0.001);
        }

        [TestMethod]
        public void TopEdge()
        {
            var v = _tiles.GetAltitudeVector(new Point3D(290425, 7100995 + 99), new Point3D(290425 + 99, 7100995 + 99));

            foreach (var h in v)
                Debug.WriteLine(h.Z);

            // Verified visually using hoydedata.no/LaserInnsyn
            var correct = new[] { 453.6667785644531, 453.65545654296875, 453.72637939453125, 453.83782958984375, 453.8252258300781, 453.8349914550781, 453.9550476074219, 454.13800048828125, 454.4871826171875, 454.6706237792969, 454.61328125, 454.77252197265625, 454.8779602050781, 454.9288024902344, 455.0129089355469, 455.0821533203125, 455.1436462402344, 455.2997131347656, 455.4102783203125, 455.451904296875, 455.534912109375, 455.6550598144531, 455.75323486328125, 455.78143310546875, 455.7600402832031, 455.7566223144531, 455.630615234375, 455.64404296875, 455.76947021484375, 455.8865661621094, 456.07940673828125, 456.1148986816406, 456.0177001953125, 456.23150634765625, 456.5251159667969, 456.7765808105469, 456.9591369628906, 457.19146728515625, 457.3645324707031, 457.54852294921875, 457.5195617675781, 457.540283203125, 457.49609375, 457.4402770996094, 457.3959655761719, 457.25140380859375, 457.1854248046875, 457.0092468261719, 457.0605163574219, 457.2371520996094, 457.25787353515625, 457.1833190917969, 457.1059875488281, 457.15875244140625, 457.20941162109375, 457.31829833984375, 457.3871765136719, 457.3517150878906, 457.2954406738281, 457.203369140625, 457.1581726074219, 457.0372009277344, 456.91302490234375, 456.6705017089844, 457.01104736328125, 456.5006408691406, 455.5909118652344, 455.617919921875, 454.833984375, 454.1697692871094, 453.9722900390625, 454.1636657714844, 453.86480712890625, 453.5457763671875, 453.20159912109375, 453.0281066894531, 453.07305908203125, 452.9806823730469, 453.0047607421875, 453.14208984375, 453.2882385253906, 453.4344177246094, 453.71417236328125, 453.8348693847656, 453.92987060546875, 453.99932861328125, 453.8685302734375, 453.9478759765625, 454.0813293457031, 453.8841857910156, 453.8730773925781, 453.9593811035156, 453.93316650390625, 454.1813049316406, 454.02801513671875, 454.045166015625, 454.0122375488281, 453.3480224609375, 453.0684509277344, 453.0776062011719 }
                .Select(p => (float)p).ToArray();

            Assert.AreEqual(correct.Length, v.Length);

            for (var i = 0; i < v.Length; i++)
                Assert.AreEqual(correct[i], v[i].Z, 0.001);
        }

        [TestMethod]
        public void RightEdge()
        {
            var v = _tiles.GetAltitudeVector(new Point3D(290425 + 99, 7100995), new Point3D(290425 + 99, 7100995 + 99));

            foreach (var h in v)
                Debug.WriteLine(h.Z);

            // Verified visually using hoydedata.no/LaserInnsyn
            var correct = new[] { 473.1006774902344, 473.0924987792969, 473.06195068359375, 473.0021057128906, 472.85296630859375, 472.75433349609375, 472.7338562011719, 472.6384582519531, 472.62420654296875, 472.6849670410156, 472.64837646484375, 472.5372619628906, 472.3709716796875, 472.14605712890625, 472.013427734375, 471.9720153808594, 471.9167785644531, 471.8307800292969, 471.7252502441406, 471.5909118652344, 471.4437561035156, 471.292724609375, 471.15460205078125, 471.0333557128906, 470.9000244140625, 470.7734375, 470.70184326171875, 470.6458740234375, 470.5731201171875, 470.4518127441406, 470.20428466796875, 469.9656982421875, 469.8500061035156, 469.8182067871094, 469.7867126464844, 469.6453857421875, 469.580078125, 469.5436706542969, 469.4551086425781, 469.4830322265625, 469.4210205078125, 469.2380065917969, 469.0223083496094, 468.8211975097656, 468.5966796875, 468.28363037109375, 467.99359130859375, 467.7746887207031, 467.6334533691406, 467.4512634277344, 467.1656188964844, 466.8134765625, 466.354248046875, 465.77362060546875, 465.35467529296875, 465.04132080078125, 464.5851135253906, 463.962646484375, 463.3188781738281, 462.8249816894531, 462.2629089355469, 461.5954284667969, 461.1471252441406, 460.8621826171875, 460.66259765625, 460.4260559082031, 460.087646484375, 459.8024597167969, 459.4559631347656, 459.2441711425781, 459.1297607421875, 458.9494323730469, 458.7134094238281, 458.4576110839844, 458.2691650390625, 458.1335754394531, 457.91644287109375, 457.6450500488281, 457.5076904296875, 457.4425964355469, 457.3134460449219, 457.44757080078125, 457.4613037109375, 457.3171081542969, 456.93511962890625, 456.67578125, 456.4121398925781, 456.1142883300781, 455.9311218261719, 455.5765686035156, 455.1949768066406, 454.9576721191406, 454.7405700683594, 454.49737548828125, 454.181640625, 454.0858154296875, 453.89483642578125, 453.7230529785156, 453.4411926269531, 453.0776062011719 }
                .Select(p => (float)p).ToArray();

            Assert.AreEqual(correct.Length, v.Length);

            for (var i = 0; i < v.Length; i++)
                Assert.AreEqual(correct[i], v[i].Z, 0.001);
        }
    }
}
