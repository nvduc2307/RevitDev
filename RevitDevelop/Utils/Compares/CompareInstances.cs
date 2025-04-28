using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitDevelop.Utils.Compares
{
    public static class CompareInstances
    {
        public static T Cast<T>(this object obj)
        {
            return (T)obj;
        }
        public static double Round(this double source)
        {
            return Math.Round(source, 9);
        }
        public static double Round(this double source, int digits)
        {
            return Math.Round(source, digits);
        }
        public static bool IsAlmostEqual(this double source, double value)
        {
            return Math.Abs(source - value) < 1e-9;
        }
        public static bool IsAlmostEqual(this double source, double value, double tolerance)
        {
            return Math.Abs(source - value) < tolerance;
        }
        public static bool IsNullOrEmpty(this string source)
        {
            return string.IsNullOrEmpty(source);
        }
        public static bool IsNullOrWhiteSpace(this string source)
        {
            return string.IsNullOrWhiteSpace(source);
        }
        public static bool Contains(this string source, string value, StringComparison comparison)
        {
            return source.IndexOf(value, comparison) >= 0;
        }
    }
}
