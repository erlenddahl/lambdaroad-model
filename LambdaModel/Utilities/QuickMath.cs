using System;
using System.Collections.Generic;
using System.Text;

namespace LambdaModel.Utilities
{
    public static class QuickMath
    {
        public static int Round(double num)
        {
            return (int)(num + 0.5); // Since coordinates are always positive. If negative, need to use -0.5 in negative cases.
        }
    }
}
