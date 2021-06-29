using System.Linq;
using LambdaModel.Terrain.Tiff;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LambdaModel.Tests.Terrain.Tiff
{
    [TestClass]
    public class GeoTiffHeaderTests
    {
        private void Check(string filename, string str, int size)
        {
            var bounds = str.Replace(":", ",").Split(',').Select(p => p.Split('.')[0].Trim()).Select(int.Parse).ToArray();

            var tiff = new GeoTiff(@"..\..\..\..\Data\Testing\HeaderTests\" + filename + ".tiff");
            Assert.AreEqual(bounds[0], tiff.StartX);
            Assert.AreEqual(size, tiff.Width);
            Assert.AreEqual(bounds[2], tiff.EndX);

            Assert.AreEqual(bounds[3], tiff.StartY);
            Assert.AreEqual(size, tiff.Height);
            Assert.AreEqual(bounds[1], tiff.EndY);
        }

        [TestMethod]
        public void FileA()
        {
            Check("A", "221184.0000000000000000,6993920.0000000000000000 : 221696.0000000000000000,6994432.0000000000000000", 512);
        }

        [TestMethod]
        public void FileB()
        {
            Check("B", "262656.0000000000000000,7045120.0000000000000000 : 263168.0000000000000000,7045632.0000000000000000", 512);
        }

        [TestMethod]
        public void FileC()
        {
            Check("C", "261120.0000000000000000,7044608.0000000000000000 : 261632.0000000000000000,7045120.0000000000000000", 512);
        }

        [TestMethod]
        public void FileD()
        {
            Check("D", "301750.0000000000000000,7108450.0000000000000000 : 301800.0000000000000000,7108500.0000000000000000", 50);
        }

        [TestMethod]
        public void FileE()
        {
            Check("E", "301900.0000000000000000,7108400.0000000000000000 : 302000.0000000000000000,7108500.0000000000000000", 100);
        }

        [TestMethod]
        public void FileF()
        {
            Check("F", "264704.0000000000000000,7037440.0000000000000000 : 265216.0000000000000000,7037952.0000000000000000", 512);
        }
    }
}