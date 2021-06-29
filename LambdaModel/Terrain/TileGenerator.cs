using System;
using System.Collections.Generic;
using System.Text;
using ConsoleUtilities.ConsoleInfoPanel;
using LambdaModel.Terrain.Tiff;

namespace LambdaModel.Terrain
{
    public class TileGenerator
    {
        private readonly string _source;
        private readonly string _destination;
        private readonly int _tileSize;

        public TileGenerator(string source, string destination, int tileSize)
        {
            _source = source;
            _destination = destination;
            _tileSize = tileSize;
        }

        public void Generate(ConsoleInformationPanel cip)
        {
            var files = System.IO.Directory.GetFiles(_source, "*.tif");
            using (var pb = cip.SetProgress("Generating tiles", max: files.Length))
            {
                foreach (var file in files)
                {
                    using (var tiff = new GeoTiff(file))
                    {
                        if (tiff.Width % _tileSize != 0 || tiff.Height % _tileSize != 0) throw new Exception($"Invalid file size: {tiff.Width} x {tiff.Height}");
                        for (var x = tiff.StartX; x < tiff.EndX; x += _tileSize)
                        for (var y = tiff.StartY - _tileSize + 1; y <= tiff.StartY; y += _tileSize)
                        {
                            var fn = System.IO.Path.Combine(_destination, $"{x},{y}_{_tileSize}x{_tileSize}.bin");
                            using (var tile = tiff.GetSubset(x, y, _tileSize))
                                QuickGeoTiff.WriteQuickTiff(tile, fn);
                        }
                    }

                    pb.Increment();
                }
            }
        }
    }
}
