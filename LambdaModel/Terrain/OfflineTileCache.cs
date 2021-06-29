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
    public class OfflineTileCache : TileCacheBase
    {
        public OfflineTileCache(string cacheLocation, int tileSize = 512, ConsoleInformationPanel cip = null) : base(cacheLocation, tileSize, cip)
        {
        }

        protected override string GetFilename(int ix, int iy)
        {
            return System.IO.Path.Combine(_cacheLocation, $"{ix},{iy}_{TileSize}x{TileSize}.tiff");
        }
    }
}
