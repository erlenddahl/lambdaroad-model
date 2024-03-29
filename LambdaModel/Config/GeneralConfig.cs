﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using ConsoleUtilities;
using ConsoleUtilities.ConsoleInfoPanel;
using LambdaModel.PathLoss;
using LambdaModel.Stations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LambdaModel.Config
{
    public abstract class GeneralConfig : IRunnable
    {
        public string ApiKey { get; set; }

        private string _originalOutputDirectory;
        public OperationType Operation { get; set; }

        public MobileNetworkRegressionType? MobileRegression { get; set; } = MobileNetworkRegressionType.All;
        public double MinimumAllowableRsrp { get; set; } = -150;
        public double ReceiverHeightAboveTerrain { get; set; }

        public BaseStation[] BaseStations { get; set; }
        public string OutputDirectory { get; set; }
        public string LogFileName { get; set; } = "log.json";
        public bool WriteLog { get; set; } = true;
        public TerrainConfig Terrain { get; set; }

        public int? CalculationThreads { get; set; }
        
        [JsonIgnore]
        public CancellationTokenSource Cancellor { get; set; }

        [JsonIgnore]
        public ConsoleInformationPanel Cip { get; set; }

        [JsonIgnore]
        public ConsoleInformationPanelSnapshot FinalSnapshot { get; set; }

        public static GeneralConfig ParseConfigFile(string file)
        {
            if (!File.Exists(file))
                throw new FileNotFoundException("The configuration file at '" + file + "' does not exist.");

            return ParseConfigString(File.ReadAllText(file), Path.GetDirectoryName(file));
        }

        public abstract void Run();

        public virtual GeneralConfig Validate(string configLocation = null)
        {
            if (string.IsNullOrWhiteSpace(OutputDirectory)) throw new ConfigException("Output directory cannot be empty.");

            OutputDirectory = GetFullPath(configLocation, OutputDirectory);
            PrepareOutputDirectory();

            if (WriteLog && LogFileName.Contains("\\")) throw new ConfigException("LogFileName must be a file name only, not a path.");

            Terrain.Config = this;

            return this;
        }

        public void PrepareOutputDirectory()
        {
            if (_originalOutputDirectory == null) _originalOutputDirectory = OutputDirectory;
            OutputDirectory = _originalOutputDirectory.Replace("{time}", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff"))
                .Replace("{cache_size}", Terrain.MaxCacheItems.ToString())
                .Replace("{cache_remove}", Terrain.RemoveCacheItemsWhenFull.ToString());

            if (File.Exists(OutputDirectory)) throw new ConfigException("OutputDirectory is a file -- must be a directory.");
            if (!Directory.Exists(OutputDirectory))
                Directory.CreateDirectory(OutputDirectory);
        }

        protected string GetFullPath(string containingFolder, string path)
        {
            if (string.IsNullOrWhiteSpace(containingFolder)) return path;
            if (path.Contains(":\\")) return path;
            return Path.Combine(containingFolder, path);
        }

        public static GeneralConfig ParseConfigString(string text, string configLocation = null)
        {
            JObject json;
            try
            {
                json = JObject.Parse(text);
            }
            catch (Exception ex)
            {
                throw new JsonException("The given configuration data is not valid JSON.", ex);
            }

            if (json[nameof(Operation)] == null)
                throw new ConfigException("The given configuration does not contain the mandatory " + nameof(Operation) + " property.");

            if (!Enum.TryParse<OperationType>(json.Value<string>(nameof(Operation)), out var method))
                throw new ConfigException("The given configuration does not contain a valid value for the mandatory " + nameof(Operation) + " property.");

            if (method == OperationType.RoadNetwork)
                return json.ToObject<RoadNetworkConfig>()?.Validate(configLocation);

            if (method == OperationType.Grid)
                return json.ToObject<GridConfig>()?.Validate(configLocation);

            if (method == OperationType.GenerateTiles)
                return json.ToObject<GenerateTilesConfig>()?.Validate(configLocation);

            throw new ConfigException("The given configuration does not contain a valid value for the mandatory " + nameof(Operation) + " property.");
        }
    }
}
