using System;
using System.Collections.Generic;
using ConsoleUtilities.ConsoleInfoPanel;
using Extensions.Utilities;
using LambdaModel.General;
using LambdaModel.PathLoss;
using Newtonsoft.Json;
using no.sintef.SpeedModule.Geometry.SimpleStructures;

namespace LambdaModel.Stations
{
    public class BaseStation
    {
        [JsonIgnore]
        public ConsoleInformationPanel Cip { get; set; }

        [JsonIgnore]
        public IPathLossCalculator Calculator { get; private set; }

        [JsonIgnore]
        protected Point4D<double>[] _vector;
        public string Id { get; set; }
        public string Name { get; set; }
        public Point3D Center { get; set; }
        public int HeightAboveTerrain { get; set; }
        public int MaxRadius { get; set; } = 100_000;
        public AntennaType? AntennaType { get; set; }

        public string GainDefinition { get; set; } = "0";

        [JsonIgnore]
        public AntennaGain Gain { get; set; }
        public double Power { get; set; } = double.MinValue;
        public double CableLoss { get; set; } = 2;
        
        public double? ResourceBlockConstant { get; private set; }

        public BaseStation()
        {

        }

        public BaseStation(double x, double y, int heightAboveTerrain, int maxRadius, ConsoleInformationPanel cip = null)
        {
            Cip = cip;
            HeightAboveTerrain = heightAboveTerrain;
            Center = new Point3D(x, y);
            MaxRadius = maxRadius;

            Cip = cip;

            Initialize();
        }

        public void Initialize()
        {
            if (_vector != null) return;

            // Initialize a PointUtm array that is to be (re)used as the vector of points from
            // the center to each of the points that should be calculated.
            _vector = new Point4D<double>[(int)Math.Sqrt((long)MaxRadius * MaxRadius * 2L) + 1];
            for (var i = 0; i < _vector.Length; i++)
                _vector[i] = new Point4D<double>(0, 0);

            if (AntennaType == Stations.AntennaType.ItsG5)
                Calculator = new ItsG5PathLossCalculator();
            else if (AntennaType == Stations.AntennaType.MobileNetwork)
                Calculator = new MobileNetworkPathLossCalculator();
            else if (AntennaType == Stations.AntennaType.Los)
                Calculator = new LosCalculator();
            else
                throw new Exception("No calculator for AntennaType=" + AntennaType);

            if (!ResourceBlockConstant.HasValue)
                ResourceBlockConstant = AntennaType == Stations.AntennaType.MobileNetwork ? 10 * Math.Log10(12 * 50) : 0;

            Gain = AntennaGain.FromDefinition(GainDefinition);
        }

        public void Validate()
        {
            if (!AntennaType.HasValue)
                throw new Exception("Base stations must have the AntennaType property, which may be one of these values: " + string.Join(", ", EnumHelper.GetValues<AntennaType>()));
            if (AntennaType != Stations.AntennaType.Los && Power < 0) throw new Exception("Power must be defined as a positive number.");
        }

        /// <summary>
        /// Calculates the angle in degrees between the base station's coordinates and the given coordinate.
        /// Zero degrees is East, 90 degrees is North, 180 degrees is West, 270 degrees is South.
        /// </summary>
        /// <param name="targetCoordinates"></param>
        /// <returns></returns>
        public double AngleTo(Point3D targetCoordinates)
        {
            return Center.AngleFromHorizon(targetCoordinates);
        }

        /// <summary>
        /// Returns the RSRP for the given angle (power + gain_angle - cable loss - path loss - resource block constant).
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="loss"></param>
        /// <returns></returns>
        public double CalculateRsrpAtAngle(double angle, double loss)
        {
            return Power + Gain.GetGainAtAngle(angle) - CableLoss - loss - (ResourceBlockConstant ?? 0);
        }
    }
}