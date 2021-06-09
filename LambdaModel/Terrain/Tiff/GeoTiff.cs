using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using BitMiracle.LibTiff.Classic;
using LambdaModel.General;

namespace LambdaModel.Terrain.Tiff
{
	public class GeoTiff : ITiffReader
	{
		public float[,] HeightMap { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public double StartX { get; set; }
		public double StartY { get; set; }
        public double DW { get; set; }
        public double DH { get; set; }

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
                    throw new Exception("Failed to read TIFF");
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

				StartX = (int)(originLon + DW / 2d);
				StartY = (int)(originLat + DH / 2d);

				//var tileByteCountsTag = tiff.GetField(TiffTag.TILEBYTECOUNTS);
				//var tileByteCounts = tileByteCountsTag[0].TolongArray();

				//var bitsPerSampleTag = tiff.GetField(TiffTag.BITSPERSAMPLE);
				//var bytesPerSample = bitsPerSampleTag[0].ToInt() / 8;


				var tilewtag = tiff.GetField(TiffTag.TILEWIDTH);
				var tilehtag = tiff.GetField(TiffTag.TILELENGTH);
				var tilew = tilewtag[0].ToInt();
				var tileh = tilehtag[0].ToInt();

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
				for (var x = 0; x < Width; x += tilew)
				{
					for (var y = 0; y < Height; y += tileh)
					{
						var buffer = new byte[tileSize];
						tiff.ReadTile(buffer, 0, x, y, 0, 0);
						for (var tileX = 0; tileX < tilew; tileX++)
						{
							var iwhm = y + tileX;
							if (iwhm > Width - 1)
								break;

							for (var tileY = 0; tileY < tileh; tileY++)
							{
								var iyhm = x + tileY;

								if (iyhm > Height - 1)
									break;

								HeightMap[iwhm, iyhm] = BitConverter.ToSingle(buffer, (tileX * tileh + tileY) * 4);
							}
						}
					}
				}
			}
		}

		public float GetAltitude(PointUtm p)
        {
            return GetAltitude(p.X, p.Y);
        }

        public float GetAltitude(double pX, double pY)
        {
            if (HeightMap == null)
                throw new Exception("Height map has not been initialized. Did you open the file using the headerOnly flag?");

            var (x, y) = ToLocal(pX, pY);
            if (x < 0 || y < 0 || x > Width || y > Height)
                throw new Exception("Requested point is not inside this TIFF file.");

            return HeightMap[y, x];
        }

        private (int x, int y) ToLocal(double pX, double pY)
        {
            return ((int) Math.Round(pX - StartX, 0), (int) Math.Round(StartY - pY, 0));
        }

        public List<PointUtm> GetAltitudeVector(PointUtm a, PointUtm b, double incMeter = 1)
        {
            return GetAltitudeVector(a.X,a.Y, b.X, b.Y, incMeter);
        }

        public List<PointUtm> GetAltitudeVector(double aX, double aY, double bX, double bY, double incMeter = 1)
        {
            var v = new List<PointUtm>();

            (aX, aY) = ToLocal(aX, aY);
            (bX, bY) = ToLocal(bX, bY);
            
            var dx = bX - aX;
            var dy = bY - aY;
			var l = Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));

            var xInc = dx / l * incMeter;
            var yInc = dy / l * incMeter;
            var m = 0d;

            var (x, y) = (aX, aY);

            while (m <= l)
            {
                v.Add(new PointUtm(x, y, HeightMap[(int) Math.Round(y), (int) Math.Round(x)], m));

                m += incMeter;
                x += xInc;
                y += yInc;
            }

            return v;
        }
    }
}
