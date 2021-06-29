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
	public class GeoTiff : TiffReaderBase
	{
        protected int _tileW;
        protected int _tileH;
        public float[,] HeightMap { get; set; }

        public GeoTiff()
        {

        }


		public GeoTiff(string filePath, bool headerOnly = false)
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException("TIFF file '" + filePath + "' does not exist", filePath);
            using (var tiff = BitMiracle.LibTiff.Classic.Tiff.Open(filePath, "r"))
            {
                if (tiff == null)
                {
                    throw new Exception("Failed to read TIFF file at '" + filePath + "'");
                }

                ReadHeader(tiff);

                if (headerOnly)
                {
                    HeightMap = null;
                    return;
                }

                HeightMap = new float[Width, Height];

                var tileSize = tiff.TileSize();
                var buffer = new byte[tileSize];
                for (var tileX = 0; tileX < Width; tileX += _tileW)
                for (var tileY = 0; tileY < Height; tileY += _tileH)
                {
                    tiff.ReadTile(buffer, 0, tileX, tileY, 0, 0);
                    for (var y = 0; y < _tileH; y++)
                    {
                        var realY = tileY + y;
                        if (realY > Height - 1)
                            break;

                        for (var x = 0; x < _tileW; x++)
                        {
                            var realX = tileX + x;
                            if (realX > Width - 1)
                                break;

                            HeightMap[realY, realX] = BitConverter.ToSingle(buffer, (y * _tileH + x) * 4);
                        }
                    }
                }
            }
        }

        protected void ReadHeader(BitMiracle.LibTiff.Classic.Tiff tiff)
        {
            Width = tiff.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
            Height = tiff.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
            var modelPixelScaleTag = tiff.GetField(TiffTag.GEOTIFF_MODELPIXELSCALETAG);
            var modelTiePointTag = tiff.GetField(TiffTag.GEOTIFF_MODELTIEPOINTTAG);

            var modelPixelScale = modelPixelScaleTag[1].GetBytes();
            var dw = BitConverter.ToDouble(modelPixelScale, 0);
            var dh = BitConverter.ToDouble(modelPixelScale, 8) * -1;

            var modelTransformation = modelTiePointTag[1].GetBytes();
            var originLon = BitConverter.ToDouble(modelTransformation, 24);
            var originLat = BitConverter.ToDouble(modelTransformation, 32);

            StartX = (int)(originLon + dw / 2d);
            StartY = (int)(originLat + dh / 2d);
            SetEnds();

            var tileWidthTag = tiff.GetField(TiffTag.TILEWIDTH);
            var tileHeightTag = tiff.GetField(TiffTag.TILELENGTH);
            _tileW = tileWidthTag[0].ToInt();
            _tileH = tileHeightTag[0].ToInt();
        }

        protected override float GetAltitudeInternal(int x, int y)
        {
            if (HeightMap == null)
                throw new Exception("Height map has not been initialized. Did you open the file using the headerOnly flag?");

            return HeightMap[y, x];
        }


        public override void Dispose()
        {
            HeightMap = null;
        }
    }
}
