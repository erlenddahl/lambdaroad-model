﻿using System;
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
	public class GeoTiff : ITiffReader
	{
		public float[,] HeightMap { get; set; }
		public int Width { get; protected set; }
        public int Height { get; protected set; }
        public int StartX { get; protected set; }
        public int StartY { get; protected set; }
        public int EndX { get; protected set; }
        public int EndY { get; protected set; }
		public double DW { get;  }
        public double DH { get;  }

        protected GeoTiff()
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

                Width = tiff.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
                Height = tiff.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
                HeightMap = new float[Width, Height];
                var modelPixelScaleTag = tiff.GetField(TiffTag.GEOTIFF_MODELPIXELSCALETAG);
                var modelTiePointTag = tiff.GetField(TiffTag.GEOTIFF_MODELTIEPOINTTAG);

                var modelPixelScale = modelPixelScaleTag[1].GetBytes();
                DW = BitConverter.ToDouble(modelPixelScale, 0);
                DH = BitConverter.ToDouble(modelPixelScale, 8) * -1;

                var modelTransformation = modelTiePointTag[1].GetBytes();
                var originLon = BitConverter.ToDouble(modelTransformation, 24);
                var originLat = BitConverter.ToDouble(modelTransformation, 32);

                StartX = (int) (originLon + DW / 2d);
                StartY = (int) (originLat + DH / 2d);
                SetEnds();

                //var tileByteCountsTag = tiff.GetField(TiffTag.TILEBYTECOUNTS);
                //var tileByteCounts = tileByteCountsTag[0].TolongArray();

                //var bitsPerSampleTag = tiff.GetField(TiffTag.BITSPERSAMPLE);
                //var bytesPerSample = bitsPerSampleTag[0].ToInt() / 8;


                var tileWidthTag = tiff.GetField(TiffTag.TILEWIDTH);
                var tileHeightTag = tiff.GetField(TiffTag.TILELENGTH);
                var tileW = tileWidthTag[0].ToInt();
                var tileH = tileHeightTag[0].ToInt();

                /*var tileWidthCount = Width / tilew;
                var remainingWidth = Width - tileWidthCount * tilew;
                if (remainingWidth > 0)
                {
                    tileWidthCount++;
                }

                var tileHeightCount = Height / tileh;
                var remainingHeight = Height - tileHeightCount * tileh;
                if (remainingHeight > 0)
                {
                    tileHeightCount++;
                }*/

                if (headerOnly)
                {
                    HeightMap = null;
                    return;
                }

                var tileSize = tiff.TileSize();
                var buffer = new byte[tileSize];
                for (var x = 0; x < Width; x += tileW)
                for (var y = 0; y < Height; y += tileH)
                {
                    tiff.ReadTile(buffer, 0, x, y, 0, 0);
                    for (var tileX = 0; tileX < tileW; tileX++)
                    {
                        var iwhm = y + tileX;
                        if (iwhm > Width - 1)
                            break;

                        for (var tileY = 0; tileY < tileH; tileY++)
                        {
                            var iyhm = x + tileY;
                            if (iyhm > Height - 1)
                                break;

                            HeightMap[iwhm, iyhm] = BitConverter.ToSingle(buffer, (tileX * tileH + tileY) * 4);
                        }
                    }
                }
            }
        }

        protected void SetEnds()
        {
            EndX = StartX + Width;
            EndY = StartY - Height;
        }

        public float GetAltitude(Point3D p)
        {
            return GetAltitude(p.X, p.Y);
        }

        public float GetAltitude(double pX, double pY)
        {
            if (HeightMap == null)
                throw new Exception("Height map has not been initialized. Did you open the file using the headerOnly flag?");

            var (x, y) = ToLocal(pX, pY);
            if (!Contains(pX, pY))
                throw new Exception("Requested point is not inside this TIFF file.");

            return HeightMap[y, x];
        }

        private (int x, int y) ToLocal(double pX, double pY)
        {
            return (QuickMath.Round(pX - StartX), QuickMath.Round(StartY - pY));
        }

        public Point3D[] GetAltitudeVector(Point3D a, Point3D b, int incMeter = 1)
        {
            return GetAltitudeVector(a.X,a.Y, b.X, b.Y, incMeter);
        }

        public Point3D[] GetAltitudeVector(double aX, double aY, double bX, double bY, int incMeter = 1)
        {
            (aX, aY) = ToLocal(aX, aY);
            (bX, bY) = ToLocal(bX, bY);
            
            var dx = bX - aX;
            var dy = bY - aY;
			var l = Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));
            var v = new Point3D[(int)l + 1];

            var xInc = dx / l * incMeter;
            var yInc = dy / l * incMeter;
            var m = 0;

            var (x, y) = (aX, aY);

            while (m <= l)
            {
                v[m] = new Point3D(x, y, HeightMap[QuickMath.Round(y), QuickMath.Round(x)]);

                m += incMeter;
                x += xInc;
                y += yInc;
            }

            return v;
        }

        public bool Contains(double pX, double pY)
        {
            var x = QuickMath.Round(pX);
            var y = QuickMath.Round(pY);
            return x >= StartX && x < EndX && y > EndY && y <= StartY;
        }
    }
}
