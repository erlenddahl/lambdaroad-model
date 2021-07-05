using System;
using System.Collections.Generic;
using System.Text;
using LambdaModel.General;

namespace LambdaModel.Stations
{
    public class RoadLinkBaseStation : BaseStation
    {
        public List<ShapeLink> Links { get; set; } = new List<ShapeLink>();

        public RoadLinkBaseStation(double x, double y, int heightAboveTerrain) : base(x, y, heightAboveTerrain)
        {
        }
    }
}
