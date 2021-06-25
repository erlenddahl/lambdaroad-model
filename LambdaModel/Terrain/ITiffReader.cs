using System.Collections.Generic;
using LambdaModel.General;
using no.sintef.SpeedModule.Geometry.SimpleStructures;

namespace LambdaModel.Terrain
{
    public interface ITiffReader
    {
        float GetAltitude(Point3D p);
        float GetAltitude(double pX, double pY);
        Point3D[] GetAltitudeVector(Point3D a, Point3D b, int incMeter = 1);
        Point3D[] GetAltitudeVector(double aX, double aY, double bX, double bY, int incMeter = 1);
    }
}