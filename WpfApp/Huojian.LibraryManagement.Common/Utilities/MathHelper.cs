using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShadowBot.Common.Utilities
{
    public static class MathHelper
    {
        public const double Epsilon = 1E-5;

        public static bool AreDoubleClose(double d1, double d2)
        {
            return AreDoubleClose(d1, d2, Epsilon);
        }

        public static bool AreDoubleClose(double d1, double d2, double precesion)
        {
            return Math.Abs(d1 - d2) < precesion;
        }

        public static bool LessThan(double value1, double value2)
        {
            if (value1 < value2)
            {
                return !AreDoubleClose(value1, value2);
            }

            return false;
        }

        public static bool GreaterThan(double value1, double value2)
        {
            if (value1 > value2)
            {
                return !AreDoubleClose(value1, value2);
            }

            return false;
        }
    }
}
