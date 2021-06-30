using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using BitMiracle.LibTiff.Classic;
using LambdaModel.General;
using LambdaModel.Utilities;
using no.sintef.SpeedModule.Geometry.SimpleStructures;

namespace LambdaModel.Terrain.Tiff
{
	public class LazyGeoTiff : GeoTiff
	{
        private readonly BitMiracle.LibTiff.Classic.Tiff _tiff;

        protected LazyGeoTiff()
        {

        }


        public LazyGeoTiff(string filePath, bool headerOnly = false, int maxCacheItems = 20, int removeCacheItemsWhenFull = 5)
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException("TIFF file '" + filePath + "' does not exist", filePath);
            _tiff = BitMiracle.LibTiff.Classic.Tiff.Open(filePath, "r");
            if (_tiff == null)
            {
                throw new Exception("Failed to read TIFF file at '" + filePath + "'");
            }

            ReadHeader(_tiff);
            if (headerOnly)
                return;

            _buffer = new byte[_tiff.TileSize()];

            _readBuffers = new LruCache<(int x, int y), byte[]>(maxCacheItems, removeCacheItemsWhenFull);
        }

        private byte[] _buffer;
        private readonly LruCache<(int x, int y), byte[]> _readBuffers;

        protected override float GetAltitudeInternal(int x, int y)
        {
            y = Height - y - 1;

            var xInTile = x % _tileW;
            var yInTile = y % _tileH;

            var tileX = x - xInTile;
            var tileY = y - yInTile;

            if (!_readBuffers.TryGetValue((tileX, tileY), out var buffer))
            {
                _tiff.ReadTile(_buffer, 0, tileX, tileY, 0, 0);
                buffer = _buffer.ToArray();
                _readBuffers.Add((tileX, tileY), buffer);
            }

            return BitConverter.ToSingle(buffer, (yInTile * _tileH + xInTile) * 4);
        }

        public override void Dispose()
        {
            _tiff.Close();
            _tiff.Dispose();
            _readBuffers.Clear();
            _buffer = null;
            base.Dispose();
        }
    }
}
