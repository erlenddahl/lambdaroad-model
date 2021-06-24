using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
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
        private int _maxTries = 10;

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
        
        /// <summary>
        /// Downloads a TIFF file starting at the given coordinates with the 2D size tileSize (set in the constructor), saves it to the given path.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private async Task DownloadTileForCoordinate(int x, int y, string filePath)
        {
            var bbox = $"{x},{y},{x + _tileSize},{y + _tileSize}";
            var url = $"https://wms.geonorge.no/skwms1/wms.hoyde-dom1?bbox={bbox}&format=image/tiff&service=WMS&version=1.1.1&request=GetMap&srs=EPSG:25833&transparent=true&width={_tileSize}&height={_tileSize}&layers=dom1_33:None";
            await _wc.DownloadFileTaskAsync(url, filePath);
            
            // File contents may be XML, containing among other stuff "Overforbruk på kort tid" and "Vent litt, prøv igjen".
            // Probably better to use bigger files to reduce chance of this. Testing shows that 100x100 is marginally faster than 50x50 anyway, probably more so when running grid instead of line.
            CheckTiff(filePath);
            
            TilesDownloaded++;
        }

        private void CheckTiff(string filePath)
        {
            if (new System.IO.FileInfo(filePath).Length < 500)
            {
                try
                {
                    var xml = XElement.Parse(System.IO.File.ReadAllText(filePath));
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

            if (System.IO.File.Exists(fn))
            {
                if (new System.IO.FileInfo(fn).Length < 500)
                {
                    try
                    {
                        CheckTiff(fn);
                    }
                    catch (TiffTileTooManyDownloadsException ex)
                    {
                        System.IO.File.Delete(fn);
                    }
                }
            }

            if (!System.IO.File.Exists(fn))
            {
                for (var i = 0; i < _maxTries; i++)
                {
                    try
                    {
                        await DownloadTileForCoordinate(ix, iy, fn);
                        break;
                    }
                    catch (TiffTileTooManyDownloadsException ex)
                    {
                        Debug.WriteLine(ex.Message);
                        await Task.Delay(1000);
                    }
                }
            }
            
            tiff = new GeoTiff(fn);
            _tiffCache.Add((ix, iy), tiff);
            return tiff;
        }

        public PointUtm[] GetAltitudeVector(PointUtm a, PointUtm b, int incMeter = 1)
        {
            return GetAltitudeVector(a.X, a.Y, b.X, b.Y, incMeter);
        }

        public PointUtm[] GetAltitudeVector(double aX, double aY, double bX, double bY, int incMeter = 1)
        {
            var v = GetVector(aX, aY, bX, bY, incMeter);

            GeoTiff tiff = null;

            foreach(var p in v)
            {
                if (tiff?.Contains(p.X, p.Y) != true)
                    tiff = GetTiff(p.X, p.Y).Result;

                p.Z = tiff.GetAltitude(p.X, p.Y);
            }

            return v;
        }

        public void FillAltitudeVector(PointUtm[] vector, int tillIndex)
        {

            GeoTiff tiff = null;
            for (var i = 0; i <= tillIndex; i++)
            {
                if (vector[i].Z != double.MinValue) continue;

                if (tiff?.Contains(vector[i].X, vector[i].Y) != true)
                    tiff = GetTiff(vector[i].X, vector[i].Y).Result;
                
                vector[i].Z = tiff.GetAltitude(vector[i]);
            }
        }

        public PointUtm[] GetVector(PointUtm a, PointUtm b, int incMeter = 1)
        {
            return GetVector(a.X, a.Y, b.X, b.Y, incMeter);
        }

        public PointUtm[] GetVector(double aX, double aY, double bX, double bY, int incMeter = 1)
        {
            var dx = bX - aX;
            var dy = bY - aY;
            var l = Math.Sqrt(dx*dx + dy*dy);
            var v = new PointUtm[(int) l + 1];

            var xInc = dx / l * incMeter;
            var yInc = dy / l * incMeter;
            var m = 0;

            var (x, y) = (aX, aY);

            while (m <= l)
            {
                v[m] = new PointUtm(x, y, double.MinValue, m);

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
