using System;
using System.Diagnostics;
using System.Threading;
using ConsoleUtilities.ConsoleInfoPanel;
using LambdaModel.General;
using LambdaModel.Terrain.Tiff;
using LambdaModel.Utilities;
using no.sintef.SpeedModule.Geometry.SimpleStructures;

namespace LambdaModel.Terrain.Cache
{
    public abstract class TileCacheBase<T> : ITiffReader, IDisposable
    {
        protected readonly string _cacheLocation;
        public int TileSize { get; }
        protected LruCache<T, TiffReaderBase> TiffCache { get; set; }
        protected readonly ConsoleInformationPanel _cip;

        public Func<string, TiffReaderBase> CreateTiff = fn => new QuickGeoTiff(fn);

        private static int _cacheCounter = -1;
        private int _cacheIx;
        private int _lastRetrieved;

        public int TilesRetrievedFromCache => TiffCache.RetrievedFromCache;
        public double SecondsLostToRemovals => TiffCache.SecondsLostToRemovals;

        public TileCacheBase(string cacheLocation, int tileSize = 512, ConsoleInformationPanel cip = null, int maxCacheItems = 1000, int removeCacheItemsWhenFull = 5)
        {
            _cip = cip;
            _cacheLocation = cacheLocation;
            TileSize = tileSize;
            TiffCache = new LruCache<T, TiffReaderBase>(maxCacheItems, removeCacheItemsWhenFull);

            cip?.Set("Tile size", TileSize);
            cip?.Set("Tile cache", System.IO.Path.GetFileName(_cacheLocation));
            cip?.Set("Memcache options", TiffCache.MaxItems + " / " + TiffCache.RemoveItemsWhenFull);
            _cacheIx = Interlocked.Increment(ref _cacheCounter);

            if (!System.IO.Directory.Exists(_cacheLocation))
                System.IO.Directory.CreateDirectory(_cacheLocation);

            TiffCache.OnRemoved = tiff => tiff.Dispose();
        }

        public void SetCache(LruCache<T, TiffReaderBase> cache)
        {
            TiffCache = cache;
        }

        protected abstract string GetFilename(T key);

        public float GetAltitude(Point3D p)
        {
            return GetAltitude(p.X, p.Y);
        }

        public float GetAltitude(double x, double y)
        {
            return GetTiff(x, y).GetAltitude(x, y);
        }

        private TiffReaderBase GetTiff(double x, double y)
        {
            return GetTiffByInternalCoordinates(GetTileKey(x, y));
        }

        private TiffReaderBase GetTiff(int x, int y)
        {
            return GetTiffByInternalCoordinates(GetTileKey(x, y));
        }

        protected TiffReaderBase GetTiffByInternalCoordinates(T key)
        {
            if (TiffCache.TryGetValue(key, out var tiff))
                return tiff;

            var fn = GetFilename(key);
            tiff = CreateTiff(fn);

            TiffCache.Add(key, tiff);

            var retrieved = TiffCache.RetrievedFromCache - _lastRetrieved;
            _lastRetrieved = TiffCache.RetrievedFromCache;
            _cip?.Increment("Tiles retrieved from memcache", retrieved);

            _cip?.Set("Memcache added/current [" + _cacheIx + "]", TiffCache.AddedToCache + " / " + TiffCache.CurrentlyInCache);
            _cip?.Set("Memcache rem/rem.ops [" + _cacheIx + "]", TiffCache.RemovedFromCache + " / " + TiffCache.CacheRemovals);

            return tiff;
        }

        public virtual T GetTileKey(double x, double y)
        {
            return GetTileKey(QuickMath.Round(x), QuickMath.Round(y));
        }

        public abstract T GetTileKey(int x, int y);

        public Point4D<double>[] GetAltitudeVector(Point3D a, Point3D b, int incMeter = 1)
        {
            return GetAltitudeVector(a.X, a.Y, b.X, b.Y, incMeter);
        }

        public Point4D<double>[] GetAltitudeVector(double aX, double aY, double bX, double bY, int incMeter = 1)
        {
            var v = GetVector(aX, aY, bX, bY, incMeter);

            TiffReaderBase tiff = null;

            foreach(var p in v)
            {
                if (tiff?.Contains(p.X, p.Y) != true)
                    tiff = GetTiff(p.X, p.Y);

                p.Z = tiff.GetAltitude(p.X, p.Y);
            }

            return v;
        }

        public void FillAltitudeVector(Point3D[] vector, int tillIndex)
        {
            TiffReaderBase tiff = null;
            for (var i = 0; i <= tillIndex; i++)
            {
                if (!double.IsNaN(vector[i].Z)) continue;

                if (tiff?.Contains(vector[i].X, vector[i].Y) != true)
                    tiff = GetTiff(vector[i].X, vector[i].Y);
                
                vector[i].Z = tiff.GetAltitude(vector[i]);
            }
        }

        public Point4D<double>[] GetVector(double aX, double aY, double bX, double bY, int incMeter = 1)
        {
            var dx = bX - aX;
            var dy = bY - aY;
            var l = Math.Sqrt(dx * dx + dy * dy);
            var v = new Point4D<double>[(int)l + 1];

            var xInc = dx / l * incMeter;
            var yInc = dy / l * incMeter;
            var m = 0;

            var (x, y) = (aX, aY);

            while (m <= l)
            {
                v[m] = new Point4D<double>(x, y, double.MinValue);

                m += incMeter;
                x += xInc;
                y += yInc;
            }

            return v;
        }

        public int FillVector(Point4D<double>[] vector, double aX, double aY, double bX, double bY, int incMeter = 1, bool withHeights = false)
        {
            var dx = bX - aX;
            var dy = bY - aY;
            var l = Math.Sqrt(dx * dx + dy * dy);

            var xInc = dx / l * incMeter;
            var yInc = dy / l * incMeter;
            var m = 0;

            var (x, y) = (aX, aY);
            var (rx, ry) = (QuickMath.Round(x), QuickMath.Round(y));

            if (withHeights)
            {
                while (m <= l)
                {
                    var tiff = GetTiff(x, y);

                    var (startX, startY, endX, endY) = (tiff.StartX, tiff.StartY, tiff.EndX, tiff.EndY);

                    while (m < l && rx >= startX && rx < endX && ry >= startY && ry < endY)
                    {
                        var vm = vector[m];

                        vm.X = x;
                        vm.Y = y;
                        vm.RoundedX = rx;
                        vm.RoundedY = ry;
                        vm.Z = tiff.GetAltitudeNoCheck(rx, ry);
                        vm.M = m;

                        m += incMeter;
                        x += xInc;
                        y += yInc;
                        rx = QuickMath.Round(x);
                        ry = QuickMath.Round(y);
                    }
                }
            }
            else
            {
                while (m <= l)
                {
                    var vm = vector[m];

                    vm.X = x;
                    vm.Y = y;
                    vm.Z = double.NaN;
                    vm.M = m;

                    vm.RoundedX = QuickMath.Round(x);
                    vm.RoundedY = QuickMath.Round(y);

                    m += incMeter;
                    x += xInc;
                    y += yInc;
                }
            }

            return (int)l + 1;
        }

        public virtual void Clear()
        {
            throw new Exception("Do you really want to do this?");
            System.IO.Directory.Delete(_cacheLocation, true);
            System.IO.Directory.CreateDirectory(_cacheLocation);
            TiffCache.Clear();
        }

        public GeoTiff GetSubset(int bottomLeftX, int bottomLeftY, int size)
        {
            var res = new GeoTiff
            {
                HeightMap = new float[size, size],
                StartX = bottomLeftX,
                StartY = bottomLeftY,
                Width = size,
                Height = size
            };
            res.SetEnds(); 
            TiffReaderBase tiff = null;
            for (var y = bottomLeftY; y < bottomLeftY + size; y++)
            for (var x = bottomLeftX; x < bottomLeftX + size; x++)
            {
                if (tiff?.Contains(x, y) != true)
                    tiff = GetTiff(x, y);

                res.HeightMap[size - y + bottomLeftY - 1, x - bottomLeftX] = tiff.GetAltitudeNoCheck(x, y);
            }

            return res;
        }

        public void Dispose()
        {
            TiffCache.Clear();
        }
    }
}
