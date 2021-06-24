using System;
using System.Collections.Generic;
using System.Text;

namespace LambdaModel.Utilities
{
    public static class SpiralGridEnumerator
    {
        /// <summary>
        /// Returns coordinates in a grid going in a clockwise spiral starting in the upper left corner, ending at the center.
        /// For example, given a radius of 2, it returns the coordinates in this sequence:
        ///
        ///     1  2  3  4  5
        ///     16 17 18 19 6
        ///     15 24 25 20 7
        ///     14 23 22 21 8
        ///     13 12 11 10 9
        /// 
        /// </summary>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static IEnumerable<(int x, int y)> Enumerate(int radius)
        {
            var minY = -radius;
            var maxY = radius;
            var minX = -radius;
            var maxX = radius;
            while (maxY >= minY || maxX >= minX)
            {
                for (var x = minX; x <= maxX; x++)
                    yield return (x, minY);
                minY++;

                for (var y = minY; y <= maxY; y++)
                    yield return (maxX, y);
                maxX--;

                for (var x = maxX; x >= minX; x--)
                    yield return (x, maxY);
                maxY--;

                for (var y = maxY; y >= minY; y--)
                    yield return (minX, y);
                minX++;
            }
        }
    }
}
