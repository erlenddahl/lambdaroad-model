using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public TileGenerator(string source, string destination, int tileSize, ConsoleInformationPanel cip) : base(source, tileSize, cip, 50, 10)
        {
            _source = source;
            _destination = destination;
            _tileSize = tileSize;
            _cip = cip;

            if (!System.IO.Directory.Exists(_destination))
                System.IO.Directory.CreateDirectory(_destination);

            _files = System.IO.Directory
                .GetFiles(_source, "*.tif")
                .Select(p => (Path: p, Tiff: new GeoTiff(p, true)))
                .ToArray();

            CreateTiff = fn => new LazyGeoTiff(fn, false, 100000, 1000);
        }

        public void Generate()
        {
            using (var pb = _cip.SetProgress("Generating tiles (" + _tileSize + ")", max: _files.Length))
            {
                for (var i = 0; i < _files.Length; i++)
                {
                    var tiff = _files[i].Tiff;
                    for (var x = tiff.StartX - tiff.StartX % _tileSize; x < tiff.EndX; x += _tileSize)
                    {
                        for (var y = tiff.StartY - tiff.StartY % _tileSize; y < tiff.EndY; y += _tileSize)
                        {
                            try
                            {
                                var fn = System.IO.Path.Combine(_destination, $"{x},{y}_{_tileSize}x{_tileSize}.bin");
                                if (System.IO.File.Exists(fn))
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

                    pb.Increment();
                }
            }
        }

        protected override string GetFilename(string key)
        {
            _cip.Set("Last loaded", System.IO.Path.GetFileName(key));
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
