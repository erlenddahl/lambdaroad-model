using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using BitMiracle.LibTiff.Classic;
using ConsoleUtilities;
using ConsoleUtilities.ConsoleInfoPanel;
using DotSpatial.Data;
using DotSpatial.Topology;
using LambdaModel.Calculations;
using LambdaModel.Config;
using LambdaModel.General;
using LambdaModel.Stations;
using LambdaModel.Terrain;
using LambdaModel.Terrain.Cache;
using LambdaModel.Terrain.Tiff;
using LambdaModel.Utilities;
using no.sintef.SpeedModule.Geometry.SimpleStructures;

namespace LambdaModelRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            new ConsoleConfigHelper(args)
                .AutoResize()
                .Run("Processing config files - Lambda Model Runner", GeneralConfig.ParseConfigFile)
                .PrintSummary();
        }
    }
}
