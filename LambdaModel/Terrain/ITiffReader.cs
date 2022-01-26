using System.Collections.Generic;
using LambdaModel.General;
using no.sintef.SpeedModule.Geometry.SimpleStructures;

namespace LambdaModel.Terrain
{
    public interface ITiffReader
    {
        float GetAltitude(Point3D p);
        float GetAltitude(double pX, double pY);
        Point4D<double>[] GetAltitudeVector(Point3D a, Point3D b, int incMeter = 1);
        Point4D<double>[] GetAltitudeVector(double aX, double aY, double bX, double bY, int incMeter = 1);
        int FillVector(Point4D<double>[] vector, double aX, double aY, double bX, double bY, int incMeter = 1, bool withHeights = false);
    }
}