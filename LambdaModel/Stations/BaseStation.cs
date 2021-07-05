﻿using System;
using ConsoleUtilities.ConsoleInfoPanel;
using LambdaModel.General;
using LambdaModel.PathLoss;
using no.sintef.SpeedModule.Geometry.SimpleStructures;

namespace LambdaModel.Stations
{
    public class BaseStation
    {
        protected readonly ConsoleInformationPanel _cip;
        protected readonly PathLossCalculator _calc;
        protected readonly Point4D[] _vector;
        public Point3D Center { get; set; }
        public int HeightAboveTerrain { get; set; }
        public int MaxRadius { get; }

        public double TotalTransmissionLevel { get; set; } = 46 + 18 - 2;

        public BaseStation(double x, double y, int heightAboveTerrain, int maxRadius, ConsoleInformationPanel cip = null)
        {
            _cip = cip;
            HeightAboveTerrain = heightAboveTerrain;
            Center = new Point3D(x, y);
            MaxRadius = maxRadius;

            _cip = cip;

            // Initialize a PointUtm array that is to be (re)used as the vector of points from
            // the center to each of the points that should be calculated.
            _vector = new Point4D[(int)Math.Sqrt((long)maxRadius * maxRadius * 2L) + 1];
            for (var i = 0; i < _vector.Length; i++)
                _vector[i] = new Point4D(0, 0);

            _calc = new PathLossCalculator();
        }

    }
}