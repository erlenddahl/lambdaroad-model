using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LambdaModel.Stations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LambdaModel.Config
{
    public class GeneralConfig
    {
        public CalculationMethod CalculationMethod { get; set; }

        public int TileSize { get; set; } = 512;
        public double MinimumAllowableSignalValue { get; set; } = -150;

        public BaseStation[] BaseStations { get; set; }
        public string OutputLocation { get; set; }
        public TerrainConfig Terrain { get; set; }

        public static GeneralConfig ParseConfigFile(string file)
        {
            if (!File.Exists(file))
                throw new FileNotFoundException("The configuration file at '" + file + "' does not exist.");

            return ParseConfigString(File.ReadAllText(file));
        }

        private GeneralConfig Validate()
        {
            if (string.IsNullOrWhiteSpace(OutputLocation)) throw new ConfigException("Invalid output location: '" + OutputLocation + "'");
            if (BaseStations?.Any() != true) throw new ConfigException("No BaseStations defined.");
            if (Terrain == null) throw new ConfigException("Missing Terrain config.");

            Terrain.Config = this;

            return this;
        }

        public static GeneralConfig ParseConfigString(string text)
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

            CalculationMethod method;
            try
            {
                method = json.Value<CalculationMethod>("CalculationMethod");
            }
            catch (ConfigException cex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ConfigException("The given configuration does not contain a valid value for the mandatory CalculationMethod property.", ex);
            }

            if (method == CalculationMethod.RoadNetwork)
                return json.ToObject<RoadNetworkConfig>()?.Validate();

            if (method == CalculationMethod.Grid)
                return json.ToObject<GridConfig>()?.Validate();

            throw new ConfigException("The given configuration does not contain a valid value for the mandatory CalculationMethod property.");
        }
    }
}
