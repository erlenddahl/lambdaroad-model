using no.sintef.SpeedModule.Geometry.SimpleStructures;

namespace LambdaModel.Stations
{
    public class BaseStation
    {
        public Point3D Center { get; set; }
        public int HeightAboveTerrain { get; set; }
        public int MaxRadius { get; set; } = 50_000;

        public double TotalTransmissionLevel { get; set; } = 46 + 18 - 2;

        public BaseStation(double x, double y, int heightAboveTerrain)
        {
            HeightAboveTerrain = heightAboveTerrain;
            Center = new Point3D(x, y);
        }

    }
}