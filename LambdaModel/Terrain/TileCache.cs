using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ConsoleUtilities.ConsoleInfoPanel;
using LambdaModel.General;
using LambdaModel.Terrain.Tiff;
using LambdaModel.Utilities;
using no.sintef.SpeedModule.Geometry.SimpleStructures;

namespace LambdaModel.Terrain
{
    public class TileCache:ITiffReader
    {
        private readonly string _cacheLocation;
        public int TileSize { get; }
        private WebClient _wc;
        private readonly Dictionary<(int x, int y), GeoTiff> _tiffCache = new Dictionary<(int x, int y), GeoTiff>();
        private int _maxTries = 10;

        public int TilesDownloaded { get; private set; }
        public int TilesRetrievedFromCache { get; private set; }

        public TileCache(string cacheLocation, int tileSize = 512)
        {
            _cacheLocation = cacheLocation;
            TileSize = tileSize;
            _wc = new WebClient();

            if (!System.IO.Directory.Exists(_cacheLocation))
                System.IO.Directory.CreateDirectory(_cacheLocation);
        }

        public async Task Preload(Point3D center, double radius, ConsoleInformationPanel cip = null)
        {
            var topLeft = center.Move(-radius, -radius);
            var bottomRight = center.Move(radius, radius);
            var max = (long) (((bottomRight.X - topLeft.X) / TileSize) * ((bottomRight.Y - topLeft.Y) / TileSize));

            using (var pb = cip?.SetProgress("Preloading map tiles", 0, max, true))
            {
                for (var x = topLeft.X; x < bottomRight.X + TileSize; x += TileSize)
                for (var y = topLeft.Y; y < bottomRight.Y + TileSize; y += TileSize)
                {
                    var (ix, iy) = GetTileCoordinates(x, y);
                    var fn = GetFilename(ix, iy);
                    if (!HasCached(fn))
                    {
                        await DownloadTileForCoordinate(ix, iy, fn);
                        cip?.Increment("New tiles downloaded");
                    }
                    else
                    {
                        cip?.Increment("Tiles already cached");
                    }

                    pb?.Increment();
                }
            }

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
            var url = $"https://wms.geonorge.no/skwms1/wms.hoyde-dom1?bbox={bbox}&format=image/tiff&service=WMS&version=1.1.1&request=GetMap&srs=EPSG:25833&transparent=true&width={TileSize}&height={TileSize}&layers=dom1_33:None";

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

                    break;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Failed to retrieve url '" + url + "'; " + ex.Message);
                    System.IO.File.Delete(filePath);
                    lastException = ex;
                    await Task.Delay(1000);
                }
            }

            if (lastException != null) throw lastException;
            
            TilesDownloaded++;
        }

        private void CheckTiff(string filePath)
        {
            if (new System.IO.FileInfo(filePath).Length < 500)
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

        private string GetFilename(double x, double y)
        {
            return System.IO.Path.Combine(_cacheLocation, $"{x},{y}_{TileSize}x{TileSize}.tiff");
        }

        public float GetAltitude(Point3D p)
        {
            return GetAltitude(p.X, p.Y);
        }

        public float GetAltitude(double x, double y)
        {
            return GetTiff(x, y).Result.GetAltitude(x, y);
        }

        private async Task<GeoTiff> GetTiff(double x, double y, bool addToCache = true)
        {
            var (ix, iy) = GetTileCoordinates(x, y);

            if (_tiffCache.TryGetValue((ix, iy), out var tiff))
            {
                TilesRetrievedFromCache++;
                return tiff;
            }

            var fn = GetFilename(ix, iy);

            if (!HasCached(fn))
                await DownloadTileForCoordinate(ix, iy, fn);
            
            tiff = new GeoTiff(fn);

            if (addToCache)
                _tiffCache.Add((ix, iy), tiff);

            return tiff;
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

        private (int ix, int iy) GetTileCoordinates(double x, double y)
        {
            var (ix, iy) = (QuickMath.Round(x), QuickMath.Round(y));
            ix -= ix % TileSize;
            iy -= iy % TileSize;
            return (ix, iy);
        }

        public Point3D[] GetAltitudeVector(Point3D a, Point3D b, int incMeter = 1)
        {
            return GetAltitudeVector(a.X, a.Y, b.X, b.Y, incMeter);
        }

        public Point3D[] GetAltitudeVector(double aX, double aY, double bX, double bY, int incMeter = 1)
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

        public void FillAltitudeVector(Point3D[] vector, int tillIndex)
        {
            GeoTiff tiff = null;
            for (var i = 0; i <= tillIndex; i++)
            {
                if (!double.IsNaN(vector[i].Z)) continue;

                if (tiff?.Contains(vector[i].X, vector[i].Y) != true)
                    tiff = GetTiff(vector[i].X, vector[i].Y).Result;
                
                vector[i].Z = tiff.GetAltitude(vector[i]);
            }
        }

        public Point3D[] GetVector(Point3D a, Point3D b, int incMeter = 1)
        {
            return GetVector(a.X, a.Y, b.X, b.Y, incMeter);
        }

        public Point3D[] GetVector(double aX, double aY, double bX, double bY, int incMeter = 1)
        {
            var dx = bX - aX;
            var dy = bY - aY;
            var l = Math.Sqrt(dx * dx + dy * dy);
            var v = new Point3D[(int)l + 1];

            var xInc = dx / l * incMeter;
            var yInc = dy / l * incMeter;
            var m = 0;

            var (x, y) = (aX, aY);

            while (m <= l)
            {
                v[m] = new Point3D(x, y, double.MinValue);

                m += incMeter;
                x += xInc;
                y += yInc;
            }

            return v;
        }

        public int FillVector(Point4D[] vector, double aX, double aY, double bX, double bY, int incMeter = 1, bool withHeights = false)
        {
            var dx = bX - aX;
            var dy = bY - aY;
            var l = Math.Sqrt(dx * dx + dy * dy);

            var xInc = dx / l * incMeter;
            var yInc = dy / l * incMeter;
            var m = 0;

            var (x, y) = (aX, aY);
            
            GeoTiff tiff = null;

            while (m <= l)
            {
                vector[m].X = x;
                vector[m].Y = y;

                if (withHeights)
                {
                    if (tiff?.Contains(vector[m].X, vector[m].Y) != true)
                        tiff = GetTiff(vector[m].X, vector[m].Y).Result;

                    vector[m].Z = tiff.GetAltitude(vector[m]);
                }
                else
                    vector[m].Z = double.NaN;
                vector[m].M = m;

                m += incMeter;
                x += xInc;
                y += yInc;
            }

            return (int)l + 1;
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
