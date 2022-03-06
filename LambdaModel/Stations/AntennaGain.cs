using System;
using System.Linq;
using LambdaModel.Utilities;

namespace LambdaModel.Stations
{
    public class AntennaGain
    {
        private double[] _values = new double[360];

        /// <summary>
        /// Creates an antenna gain from a string definition. The string must consist of sections delimited by '|'. Each section
        /// must consist of three numbers delimited by ';', where the first is the inclusive from angle, the second is the exclusive
        /// to angle, and the third is the gain value between these angles.
        /// </summary>
        /// <param name="definition"></param>
        /// <returns></returns>
        public static AntennaGain FromDefinition(string definition)
        {
            var g = new AntennaGain();

            var sections = definition
                .Split('|')
                .Select(p => p.Split(';').Select(double.Parse).ToArray())
                .Select(p => new {From = p[0], To = p[1], Value = p[2]})
                .ToArray();

            foreach (var s in sections)
            {
                for (var i = (int) Math.Round(s.From); i < (int) Math.Round(s.To); i++)
                    g._values[RestrictAngle(i)] = s.Value;
            }

            return g;
        }

        /// <summary>
        /// Creates a constant antenna gain with the given value at all angles.
        /// </summary>
        /// <param name="constantValue"></param>
        /// <returns></returns>
        public static AntennaGain FromConstant(double constantValue)
        {
            var g = new AntennaGain();
            for (var i = 0; i < g._values.Length; i++)
                g._values[i] = constantValue;
            return g;
        }

        /// <summary>
        /// Returns the antenna gain for the given angle (degrees).
        /// </summary>
        /// <param name="angle">The requested angle in degrees. Zero degrees is East, 90 degrees is North, 180 degrees is West, 270 degrees is South.</param>
        /// <returns></returns>
        public double GetGainAtAngle(double angle)
        {
            var a = RestrictAngle(angle);
            return _values[a];
        }

        /// <summary>
        /// Restricts an angle to the space between 0 (inclusive) and 360 (exclusive) degrees and rounds it to the nearest integer value.
        /// Note: does not handle negative angles!
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static int RestrictAngle(double angle)
        {
            var a = QuickMath.Round(angle);
            if (a >= 360) a = a % 360;
            if (a < 0) a += 360 * (int)Math.Ceiling(Math.Abs(a) / 360d);
            return a;
        }
    }
}