using System;
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
        private string _originalOutputDirectory;
        public CalculationMethod CalculationMethod { get; set; }

        public MobileNetworkRegressionType? MobileRegression { get; set; } = MobileNetworkRegressionType.All;
        public double MinimumAllowableRsrp { get; set; } = -150;
        public double ReceiverHeightAboveTerrain { get; set; }

        public BaseStation[] BaseStations { get; set; }
        public string OutputDirectory { get; set; }
        public string ShapeFileName { get; set; } = "results.shp";
        public string CsvFileName { get; set; } = "results.csv";
        public string ApiResultInnerFolder { get; set; } = "links";
        public string CsvSeparator { get; set; } = ";";
        public string LogFileName { get; set; } = "log.json";
        public bool WriteShape { get; set; } = true;
        public bool WriteCsv { get; set; } = true;
        public bool WriteLog { get; set; } = true;
        public bool WriteApiResults { get; set; } = true;
        public TerrainConfig Terrain { get; set; }

        public int? CalculationThreads { get; set; }
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

            if (WriteShape && ShapeFileName.Contains("\\")) throw new ConfigException("ShapeFileName must be a file name only, not a path.");
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

            if (json["CalculationMethod"] == null)
                throw new ConfigException("The given configuration does not contain the mandatory CalculationMethod property.");

            if (!Enum.TryParse<CalculationMethod>(json.Value<string>("CalculationMethod"), out var method))
                throw new ConfigException("The given configuration does not contain a valid value for the mandatory CalculationMethod property.");

            if (method == CalculationMethod.RoadNetwork)
                return json.ToObject<RoadNetworkConfig>()?.Validate(configLocation);

            if (method == CalculationMethod.Grid)
                return json.ToObject<GridConfig>()?.Validate(configLocation);

            if (method == CalculationMethod.GenerateTiles)
                return json.ToObject<GenerateTilesConfig>()?.Validate(configLocation);

            throw new ConfigException("The given configuration does not contain a valid value for the mandatory CalculationMethod property.");
        }
    }
}
