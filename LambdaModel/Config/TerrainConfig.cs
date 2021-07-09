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
        public GeneralConfig Config { get; set; }

        public TileCacheBase<(int x, int y)> CreateCache(ConsoleInformationPanel cip)
        {
            if (Type == TerrainType.OnlineCache)
                return new OnlineTileCache(Location, Config.TileSize, cip);
            return new LocalTileCache(Location, Config.TileSize, cip, MaxCacheItems, RemoveCacheItemsWhenFull);
        }
    }
}