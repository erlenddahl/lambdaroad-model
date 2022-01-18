using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using ConsoleUtilities.ConsoleInfoPanel;
using LambdaModel.Terrain.Cache;
using LambdaModel.Terrain.Tiff;
using LambdaModel.Utilities;

namespace LambdaModel.Terrain
{
    public class TileGenerator : TileCacheBase<string>, IDisposable
    {
        private readonly string _source;
        private readonly string _destination;
        private readonly int _tileSize;
        private readonly ConsoleInformationPanel _cip;
        private readonly (string Path, GeoTiff Tiff)[] _files;

        public TileGenerator(string source, string destination, int tileSize, ConsoleInformationPanel cip) : base(source, tileSize, cip, 10, 3)
        {
            BitMiracle.LibTiff.Classic.Tiff.SetErrorHandler(new LambdaTiffErrorHandler(cip));
            _source = source;
            _destination = destination;
            _tileSize = tileSize;
            _cip = cip;

            if (!Directory.Exists(_destination))
                Directory.CreateDirectory(_destination);

            _files = Directory
                .GetFiles(_source, "*.tif", SearchOption.AllDirectories)
                .Select(p => (Path: p, Tiff: new GeoTiff(p, true)))
                .ToArray();

            CreateTiff = fn => new LazyGeoTiff(fn, false, 100000, 1000);
        }

        public void Generate()
        {
            foreach (var file in _cip.Run("Generating tiles (" + _tileSize + ")", _files))
            {
                var tiff = file.Tiff;
                for (var x = tiff.StartX - tiff.StartX % _tileSize; x < tiff.EndX; x += _tileSize)
                {
                    for (var y = tiff.StartY - tiff.StartY % _tileSize; y < tiff.EndY; y += _tileSize)
                    {
                        try
                        {
                            var fn = Path.Combine(_destination, $"{x},{y}_{_tileSize}x{_tileSize}.bin");
                            if (File.Exists(fn))
                            {
                                _cip.Increment("Skipped existing tiles");
                            }
                            else
                            {
                                using (var tile = GetSubset(x, y, _tileSize))
                                    QuickGeoTiff.WriteQuickTiff(tile, fn);
                                _cip.Increment("Generated tiles");
                            }
                        }
                        catch (OutsideOfAreaException ex)
                        {
                            _cip.Increment("Tiles outside of area");
                        }
                    }
                }
            }
        }

        protected override string GetFilename(string key)
        {
            _cip.Set("Last loaded", Path.GetFileName(key));
            return key;
        }

        public override string GetTileKey(double x, double y)
        {
            return GetTileKey(QuickMath.Round(x), QuickMath.Round(y));
        }

        public override string GetTileKey(int x, int y)
        {
            foreach (var f in _files)
            {
                if (f.Tiff.Contains(x, y))
                    return f.Path;
            }

            throw new OutsideOfAreaException();
        }

        public void Dispose()
        {
            foreach(var f in _files)
                f.Tiff.Dispose();
        }
    }

    public class OutsideOfAreaException : Exception
    {
    }
}
