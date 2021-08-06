using ConsoleUtilities.ConsoleInfoPanel;
using LambdaModel.Terrain;

namespace LambdaModel.Config
{
    public class GenerateTilesConfig : GeneralConfig
    {
        public string RawDataLocation { get; set; }

        public override GeneralConfig Validate(string configLocation = null)
        {
            RawDataLocation = GetFullPath(configLocation, RawDataLocation);
            return base.Validate(configLocation);
        }

        public override void Run()
        {
            using (var cip = new ConsoleInformationPanel("Generating terrain tiles"))
                new TileGenerator(RawDataLocation, OutputLocation, TileSize, cip).Generate();
        }
    }
}