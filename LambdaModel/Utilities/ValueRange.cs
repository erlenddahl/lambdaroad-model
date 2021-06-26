namespace LambdaModel.Utilities
{
    public class ValueRange
    {
        public double Min { get; private set; } = double.MaxValue;
        public double Max { get; private set; } = double.MinValue;

        public void Extend(double v)
        {
            if (v < Min) Min = v;
            if (v > Max) Max = v;
        }

        public override string ToString()
        {
            return $"{Min:n5} - {Max:n5}";
        }
    }
}