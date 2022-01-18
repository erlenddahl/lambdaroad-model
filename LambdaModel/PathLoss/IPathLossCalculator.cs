using LambdaModel.General;

namespace LambdaModel.PathLoss
{
    public interface IPathLossCalculator
    {
        double CalculateLoss(Point4D[] path, double txHeightAboveTerrain, double rxHeightAboveTerrain, int rxIndex = -1);
        double CalculateMinPossibleLoss(double horizontalDistance, double txHeightAboveTerrain);
    }
}