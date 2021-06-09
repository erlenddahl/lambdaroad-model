using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using LambdaModel.General;
using LambdaModel.Terrain.Tiff;

namespace LambdaModel.Terrain
{
    public class TileCache:ITiffReader
    {
        private readonly string _cacheLocation;
        private readonly int _tileSize;
        private WebClient _wc;
        private readonly Dictionary<(int x, int y), GeoTiff> _tiffCache = new Dictionary<(int x, int y), GeoTiff>();

        public int TilesDownloaded { get; private set; }
        public int TilesRetrievedFromCache { get; private set; }

        public TileCache(string cacheLocation, int tileSize = 100)
        {
            _cacheLocation = cacheLocation;
            _tileSize = tileSize;
            _wc = new WebClient();

            if (!System.IO.Directory.Exists(_cacheLocation))
                System.IO.Directory.CreateDirectory(_cacheLocation);
        }
        
        private async Task DownloadTileForCoordinate(int x, int y, string filename)
        {
            var bbox = $"{x},{y},{x + _tileSize},{y + _tileSize}";
            var url = $"https://wms.geonorge.no/skwms1/wms.hoyde-dom1?bbox={bbox}&format=image/tiff&service=WMS&version=1.1.1&request=GetMap&srs=EPSG:25833&transparent=true&width={_tileSize}&height={_tileSize}&layers=dom1_33:None";
            await _wc.DownloadFileTaskAsync(url, filename);
            TilesDownloaded++;
        }

        private string GetFilename(double x, double y)
        {
            return System.IO.Path.Combine(_cacheLocation, $"{x},{y}_{_tileSize}x{_tileSize}.tiff");
        }

        public float GetAltitude(PointUtm p)
        {
            return GetAltitude(p.X, p.Y);
        }

        public float GetAltitude(double x, double y)
        {
            return GetTiff(x, y).Result.GetAltitude(x, y);
        }

        private async Task<GeoTiff> GetTiff(double x, double y)
        {
            var (ix, iy) = ((int)Math.Round(x, 0), (int)Math.Round(y, 0));
            ix -= ix % _tileSize;
            iy -= iy % _tileSize;

            if (_tiffCache.TryGetValue((ix, iy), out var tiff))
            {
                TilesRetrievedFromCache++;
                return tiff;
            }

            var fn = GetFilename(ix, iy);
            if (!System.IO.File.Exists(fn))
                await DownloadTileForCoordinate(ix, iy, fn);
            
            tiff = new GeoTiff(fn);
            _tiffCache.Add((ix, iy), tiff);
            return tiff;
        }

        public List<PointUtm> GetAltitudeVector(PointUtm a, PointUtm b, double incMeter = 1)
        {
            return GetAltitudeVector(a.X, a.Y, b.X, b.Y, incMeter);
        }

        public List<PointUtm> GetAltitudeVector(double aX, double aY, double bX, double bY, double incMeter = 1)
        {
            var v = new List<PointUtm>();

            var dx = bX - aX;
            var dy = bY - aY;
            var l = Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));

            var xInc = dx / l * incMeter;
            var yInc = dy / l * incMeter;
            var m = 0d;

            var (x, y) = (aX, aY);

            var tiff = GetTiff(x, y).Result;

            while (m <= l)
            {
                if (!tiff.Contains(x, y))
                    tiff = GetTiff(x, y).Result;

                v.Add(new PointUtm(x, y, tiff.GetAltitude(x, y), m));

                m += incMeter;
                x += xInc;
                y += yInc;
            }

            return v;
        }

        public void Clear()
        {
            System.IO.Directory.Delete(_cacheLocation, true);
            System.IO.Directory.CreateDirectory(_cacheLocation);
            _tiffCache.Clear();
            TilesDownloaded = 0;
            TilesRetrievedFromCache = 0;
        }
    }
}
