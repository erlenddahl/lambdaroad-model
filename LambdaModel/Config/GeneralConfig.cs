using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ConsoleUtilities.ConsoleInfoPanel;
using LambdaModel.Stations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LambdaModel.Config
{
    public abstract class GeneralConfig
    {
        public CalculationMethod CalculationMethod { get; set; }

        public int TileSize { get; set; } = 512;
        public double MinimumAllowableSignalValue { get; set; } = -150;

        public BaseStation[] BaseStations { get; set; }
        public string OutputLocation { get; set; }
        public TerrainConfig Terrain { get; set; }

        public int? CalculationThreads { get; set; }

        public ConsoleInformationPanel Cip { get; set; }
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
            if (string.IsNullOrWhiteSpace(OutputLocation)) throw new ConfigException("Invalid output location: '" + OutputLocation + "'");

            OutputLocation = GetFullPath(configLocation, OutputLocation);

            Terrain.Config = this;

            return this;
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
