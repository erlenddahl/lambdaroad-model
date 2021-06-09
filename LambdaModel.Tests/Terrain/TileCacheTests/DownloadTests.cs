using LambdaModel.Terrain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LambdaModel.Tests.Terrain.TileCacheTests
{
    [TestClass]
    public class DownloadTests
    {
        private TileCache _tiles;

        [TestInitialize]
        public void Init()
        {
            _tiles = new TileCache(@"..\..\..\..\Data\Testing\CacheTest");
        }

        [TestMethod]
        public void DownloadsNecessaryFile()
        {
            _tiles.Clear();
            Assert.AreEqual(0, _tiles.TilesDownloaded);
            _tiles.GetAltitude(290425, 7100995);
            Assert.AreEqual(1, _tiles.TilesDownloaded);
        }
        
        [TestMethod]
        public void DoesntDownloadUnnecessaryFile()
        {
            _tiles.Clear();
            
            // Cleared; no data
            Assert.AreEqual(0, _tiles.TilesDownloaded);
            Assert.AreEqual(0, _tiles.TilesRetrievedFromCache);
            
            _tiles.GetAltitude(290425, 7100815);
            
            // Downloaded a new tile
            Assert.AreEqual(1, _tiles.TilesDownloaded);
            Assert.AreEqual(0, _tiles.TilesRetrievedFromCache);

            _tiles.GetAltitude(290435, 7100805);

            // No need to download; use same tile
            Assert.AreEqual(1, _tiles.TilesDownloaded);
            Assert.AreEqual(1, _tiles.TilesRetrievedFromCache);

            _tiles.GetAltitude(290455, 7100865);

            // No need to download; use same tile
            Assert.AreEqual(1, _tiles.TilesDownloaded);
            Assert.AreEqual(2, _tiles.TilesRetrievedFromCache);
        }
    }
}
