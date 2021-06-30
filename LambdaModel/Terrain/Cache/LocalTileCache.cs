using System.IO;
using ConsoleUtilities.ConsoleInfoPanel;

namespace LambdaModel.Terrain.Cache
{
    public class LocalTileCache : TileCacheBase<(int x, int y)>
    {
        public LocalTileCache(string cacheLocation, int tileSize = 512, ConsoleInformationPanel cip = null, int maxCacheItems = 1000, int removeCacheItemsWhenFull = 5) : base(cacheLocation, tileSize, cip, maxCacheItems, removeCacheItemsWhenFull)
        {
        }

        protected override string GetFilename((int, int) key)
        {
            return Path.Combine(_cacheLocation, $"{key.Item1},{key.Item2}_{TileSize}x{TileSize}.tiff");
        }

        public override (int x, int y) GetTileKey(int x, int y)
        {
            var ix = x - x % TileSize;
            var iy = y - y % TileSize;
            return (ix, iy);
        }
    }
}
