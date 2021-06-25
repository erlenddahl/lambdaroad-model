using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        private readonly byte[] _buffer;

        protected LazyGeoTiff()
        {

        }


        public LazyGeoTiff(string filePath, bool headerOnly = false)
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException("TIFF file '" + filePath + "' does not exist", filePath);
            _tiff = BitMiracle.LibTiff.Classic.Tiff.Open(filePath, "r");
            if (_tiff == null)
            {
                throw new Exception("Failed to read TIFF file at '" + filePath + "'");
            }

            ReadHeader(_tiff);
            HeightMap = new float[0, 0];

            if (headerOnly)
            {
                HeightMap = null;
                return;
            }

            _buffer = new byte[_tiff.TileSize()];
        }

        private int _previousTileX = -1;
        private int _previousTileY = -1;

        protected override float GetAltitudeInternal(int x, int y)
        {
            var xInTile = x % _tileW;
            var yInTile = y % _tileH;

            var tileX = x - xInTile;
            var tileY = y - yInTile;

            if (_previousTileX != tileX || _previousTileY != tileY)
                _tiff.ReadTile(_buffer, 0, tileX, tileY, 0, 0);

            _previousTileX = tileX;
            _previousTileY = tileY;

            return BitConverter.ToSingle(_buffer, (yInTile * _tileH + xInTile) * 4);
        }

        public void Close()
        {
            _tiff.Close();
            _tiff.Dispose();
        }
    }
}
