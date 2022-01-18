using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;
using ConsoleUtilities.ConsoleInfoPanel;

namespace LambdaModel.Terrain.Cache
{
    public class OnlineTileCache : TileCacheBase<(int x, int y)>
    {
        private WebClient _wc = new WebClient();
        private int _maxTries = 10;
        public int TilesDownloaded { get; private set; }
        public string WmsUrl { get; set; } = "https://wms.geonorge.no/skwms1/wms.hoyde-dom?bbox={0}&format=image/tiff&service=WMS&version=1.1.1&request=GetMap&srs=EPSG:25833&transparent=true&width={1}&height={2}&layers=dom1_33:None";

        public OnlineTileCache(string cacheLocation, int tileSize = 512, ConsoleInformationPanel cip = null, int maxCacheItems = 1000, int removeCacheItemsWhenFull = 5) : base(cacheLocation, tileSize, cip, maxCacheItems, removeCacheItemsWhenFull)
        {
        }

        public async Task<bool> Preload(int ix, int iy)
        {
            var fn = GetFilename((ix, iy));
            if (!HasCached(fn))
            {
                await DownloadTileForCoordinate(ix, iy, fn);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Downloads a TIFF file starting at the given coordinates with the 2D size tileSize (set in the constructor), saves it to the given path.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private async Task DownloadTileForCoordinate(int x, int y, string filePath)
        {
            var bbox = $"{x},{y},{x + TileSize},{y + TileSize}";
            var url = string.Format(WmsUrl, bbox, TileSize, TileSize);

            Exception lastException = null;
            for (var i = 0; i < _maxTries; i++)
            {
                try
                {
                    lastException = null;

                    await _wc.DownloadFileTaskAsync(url, filePath);

                    // File contents may be XML, containing among other stuff "Overforbruk på kort tid" and "Vent litt, prøv igjen".
                    // Probably better to use bigger files to reduce chance of this. Testing shows that 100x100 is marginally faster than 50x50 anyway, probably more so when running grid instead of line.
                    CheckTiff(filePath);

                    _cip?.Increment("New tiles downloaded");

                    break;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Failed to retrieve url '" + url + "'; " + ex.Message);
                    System.IO.File.Delete(filePath);
                    lastException = ex;

                    _cip?.Increment(ex is TiffTileTooManyDownloadsException ? "Tile errors (too often)" : "Tile errors (other)");

                    await Task.Delay(1000);
                }
            }

            if (lastException != null) throw lastException;
            
            TilesDownloaded++;
        }

        private void CheckTiff(string filePath)
        {
            var size = new System.IO.FileInfo(filePath).Length;
            if (size < 1) throw new TiffTileDownloadException("Empty tile file.");
            if (size < 500)
            {
                var contents = System.IO.File.ReadAllText(filePath);
                if (!contents.Trim().StartsWith("<")) return;
                try
                {
                    var xml = XElement.Parse(contents);
                    var message = xml.Element("ServiceException").Value;
                    if (message.Contains("Overforbruk p"))
                        throw new TiffTileTooManyDownloadsException(message);
                    throw new TiffTileDownloadException(message);
                }
                catch (TiffTileDownloadException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new Exception("Unknown TIFF parsing exception: " + ex.Message, ex);
                }
            }
        }

        protected override string GetFilename((int, int) key)
        {
            var fn = System.IO.Path.Combine(_cacheLocation, $"{key.Item1},{key.Item2}_{TileSize}x{TileSize}.tiff");
            if (!HasCached(fn))
                DownloadTileForCoordinate(key.Item1, key.Item2, fn).Wait();
            return fn;
        }

        public override (int x, int y) GetTileKey(int x, int y)
        {
            var ix = x - x % TileSize;
            var iy = y - y % TileSize;
            return (ix, iy);
        }

        private bool HasCached(string fn)
        {
            if (!System.IO.File.Exists(fn)) return false;
            if (new System.IO.FileInfo(fn).Length >= 500) return true;

            try
            {
                CheckTiff(fn);
                return true;
            }
            catch (TiffTileTooManyDownloadsException ex)
            {
                System.IO.File.Delete(fn);
                return false;
            }

        }
        public override void Clear()
        {
            base.Clear();
            TilesDownloaded = 0;
        }
    }
}
