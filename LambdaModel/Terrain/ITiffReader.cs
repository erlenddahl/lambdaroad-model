using System.Collections.Generic;
using LambdaModel.General;

namespace LambdaModel.Terrain
{
    public interface ITiffReader
    {
        float GetAltitude(PointUtm p);
        float GetAltitude(double pX, double pY);
        List<PointUtm> GetAltitudeVector(PointUtm a, PointUtm b, double incMeter = 1);
        List<PointUtm> GetAltitudeVector(double aX, double aY, double bX, double bY, double incMeter = 1);
    }
}