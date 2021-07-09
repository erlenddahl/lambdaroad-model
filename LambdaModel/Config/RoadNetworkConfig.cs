using LambdaModel.Stations;

namespace LambdaModel.Config
{
    public class RoadNetworkConfig : GeneralConfig
    {
        public string RoadShapeLocation { get; set; }
        public new RoadLinkBaseStation[] BaseStations { get; set; }
    }
}