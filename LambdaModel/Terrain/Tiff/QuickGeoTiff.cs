using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using BitMiracle.LibTiff.Classic;
using Extensions.StringExtensions;
using LambdaModel.General;
using LambdaModel.Utilities;
using no.sintef.SpeedModule.Geometry.SimpleStructures;

namespace LambdaModel.Terrain.Tiff
{
	public class QuickGeoTiff : TiffReaderBase
    {
        private float[,] _heightMap; // Have tested -- a flattened 1d array is not measurably faster than this

        protected QuickGeoTiff()
        {

        }


		public QuickGeoTiff(string filePath)
        {
            var quickFile = filePath.ChangeExtension(".bin");

            if (!File.Exists(quickFile))
                GenerateQuickFile(filePath, quickFile);

            using (var reader = new BinaryReader(File.OpenRead(quickFile)))
            {
                StartX = reader.ReadInt32();
                StartY = reader.ReadInt32();
                Width = reader.ReadInt32();
                Height = reader.ReadInt32();
                SetEnds();

                var bufferSize = Math.Min(4 * 5000, Width * Height);
                var bytes = new byte[bufferSize];
                _heightMap = new float[Height, Width];

                var ix = 0;
                reader.Read(bytes, 0, bufferSize);
                for (var y = 0; y < Width; y++)
                for (var x = 0; x < Height; x++)
                {
                    if (ix >= bytes.Length)
                    {
                        ix = 0;
                        reader.Read(bytes, 0, bufferSize);
                    }
                    _heightMap[y, x] = BitConverter.ToSingle(bytes, ix);
                    ix += 4;
                }
            }
        }

        private void GenerateQuickFile(string filePath, string quickFile)
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException("TIFF file '" + filePath + "' does not exist", filePath);

            using (var writer = new BinaryWriter(File.Create(quickFile)))
            using (var tiff = new GeoTiff(filePath))
            {
                writer.Write(tiff.StartX);
                writer.Write(tiff.StartY);
                writer.Write(tiff.Width);
                writer.Write(tiff.Height);

                for (var y = 0; y < tiff.Width; y++)
                for (var x = 0; x < tiff.Height; x++)
                    writer.Write(tiff.HeightMap[y, x]);
            }
        }
        
        protected override float GetAltitudeInternal(int x, int y)
        {
            return _heightMap[y, x];
        }

        public override void Dispose()
        {
            _heightMap = null;
        }
    }
}
