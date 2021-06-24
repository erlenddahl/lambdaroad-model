using System.Collections.Generic;
using LambdaModel.General;

namespace LambdaModel.Terrain
{
    public interface ITiffReader
    {
        float GetAltitude(PointUtm p);
        float GetAltitude(double pX, double pY);
        PointUtm[] GetAltitudeVector(PointUtm a, PointUtm b, int incMeter = 1);
        PointUtm[] GetAltitudeVector(double aX, double aY, double bX, double bY, int incMeter = 1);
    }
}