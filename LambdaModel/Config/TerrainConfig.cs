using ConsoleUtilities.ConsoleInfoPanel;
using LambdaModel.Terrain.Cache;

namespace LambdaModel.Config
{
    public class TerrainConfig
    {
        public TerrainType Type { get; set; } = TerrainType.LocalCache;
        public string Location { get; set; }
        public int MaxCacheItems { get; set; }
        public int RemoveCacheItemsWhenFull { get; set; }
        public string WmsUrl { get; set; }
        public GeneralConfig Config { get; set; }
        public int TileSize { get; set; } = 512;

        public TileCacheBase<(int x, int y)> CreateCache(ConsoleInformationPanel cip)
        {
            if (Type == TerrainType.OnlineCache)
            {
                var cache = new OnlineTileCache(Location, TileSize, cip, MaxCacheItems, RemoveCacheItemsWhenFull);
                if (!string.IsNullOrWhiteSpace(WmsUrl))
                    cache.WmsUrl = WmsUrl;
                return cache;
            }
            return new LocalTileCache(Location, TileSize, cip, MaxCacheItems, RemoveCacheItemsWhenFull);
        }
    }
}