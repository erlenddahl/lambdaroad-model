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
    public abstract class TileCacheBase : ITiffReader
    {
        protected readonly string _cacheLocation;
        public int TileSize { get; }
        protected readonly LruCache<(int x, int y), TiffReaderBase> _tiffCache = new LruCache<(int x, int y), TiffReaderBase>(1000, 5);
        protected readonly ConsoleInformationPanel _cip;

        public Func<string, TiffReaderBase> CreateTiff = fn => new QuickGeoTiff(fn);

        public int TilesRetrievedFromCache => _tiffCache.RetrievedFromCache;

        public TileCacheBase(string cacheLocation, int tileSize = 512, ConsoleInformationPanel cip = null)
        {
            _cip = cip;
            _cacheLocation = cacheLocation;
            TileSize = tileSize;

            cip?.Set("Tile size", TileSize);
            cip?.Set("Tile cache", System.IO.Path.GetFileName(_cacheLocation));
            _cip?.Set("Memcache options", _tiffCache.MaxItems + " / " + _tiffCache.RemoveItemsWhenFull);

            if (!System.IO.Directory.Exists(_cacheLocation))
                System.IO.Directory.CreateDirectory(_cacheLocation);

            _tiffCache.OnRemoved = tiff => tiff.Dispose();
        }

        protected abstract string GetFilename(int x, int y);

        public float GetAltitude(Point3D p)
        {
            return GetAltitude(p.X, p.Y);
        }

        public float GetAltitude(double x, double y)
        {
            return GetTiff(x, y).Result.GetAltitude(x, y);
        }

        private Task<TiffReaderBase> GetTiff(double x, double y)
        {
            var (ix, iy) = GetTileCoordinates(x, y);
            return GetTiffByInternalCoordinates(ix, iy);
        }

        private Task<TiffReaderBase> GetTiff(int x, int y)
        {
            var (ix, iy) = (x - x % TileSize, y - y % TileSize);
            return GetTiffByInternalCoordinates(ix, iy);
        }

        protected async Task<TiffReaderBase> GetTiffByInternalCoordinates(int ix, int iy)
        {
            if (_tiffCache.TryGetValue((ix, iy), out var tiff))
                return tiff;

            var fn = GetFilename(ix, iy);
            tiff = CreateTiff(fn);

            _tiffCache.Add((ix, iy), tiff);

            _cip?.Set("Tiles retrieved from memcache", _tiffCache.RetrievedFromCache);
            _cip?.Set("Tiles removed from memcache", _tiffCache.RemovedFromCache);
            _cip?.Set("Tiles added to memcache", _tiffCache.AddedToCache);
            _cip?.Set("Tiles in memcache", _tiffCache.CurrentlyInCache);

            return tiff;
        }

        public (int ix, int iy) GetTileCoordinates(double x, double y)
        {
            var (ix, iy) = (QuickMath.Round(x), QuickMath.Round(y));
            ix -= ix % TileSize;
            iy -= iy % TileSize;
            return (ix, iy);
        }

        public Point4D[] GetAltitudeVector(Point3D a, Point3D b, int incMeter = 1)
        {
            return GetAltitudeVector(a.X, a.Y, b.X, b.Y, incMeter);
        }

        public Point4D[] GetAltitudeVector(double aX, double aY, double bX, double bY, int incMeter = 1)
        {
            var v = GetVector(aX, aY, bX, bY, incMeter);

            TiffReaderBase tiff = null;

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
            TiffReaderBase tiff = null;
            for (var i = 0; i <= tillIndex; i++)
            {
                if (!double.IsNaN(vector[i].Z)) continue;

                if (tiff?.Contains(vector[i].X, vector[i].Y) != true)
                    tiff = GetTiff(vector[i].X, vector[i].Y).Result;
                
                vector[i].Z = tiff.GetAltitude(vector[i]);
            }
        }

        public Point4D[] GetVector(double aX, double aY, double bX, double bY, int incMeter = 1)
        {
            var dx = bX - aX;
            var dy = bY - aY;
            var l = Math.Sqrt(dx * dx + dy * dy);
            var v = new Point4D[(int)l + 1];

            var xInc = dx / l * incMeter;
            var yInc = dy / l * incMeter;
            var m = 0;

            var (x, y) = (aX, aY);

            while (m <= l)
            {
                v[m] = new Point4D(x, y, double.MinValue);

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

            TiffReaderBase tiff = null;

            while (m <= l)
            {
                var vm = vector[m];

                vm.X = x;
                vm.Y = y;

                vm.RoundedX = QuickMath.Round(x);
                vm.RoundedY = QuickMath.Round(y);

                if (withHeights)
                {
                    if (tiff?.Contains(vm.RoundedX, vm.RoundedY) != true)
                        tiff = GetTiff(vm.RoundedX, vm.RoundedY).Result;

                    vm.Z = tiff.GetAltitudeNoCheck(vm.RoundedX, vm.RoundedY);
                }
                else
                    vm.Z = double.NaN;

                vm.M = m;

                m += incMeter;
                x += xInc;
                y += yInc;
            }

            return (int)l + 1;
        }

        public virtual void Clear()
        {
            throw new Exception("Do you really want to do this?");
            System.IO.Directory.Delete(_cacheLocation, true);
            System.IO.Directory.CreateDirectory(_cacheLocation);
            _tiffCache.Clear();
        }
    }
}
