using System.Text.RegularExpressions;

namespace RevitDevelop.Utils.NumberUtils
{
    public static class NumberUtil
    {
        public static double SpacingFromMaxSpacing(double length, double maxSpacing, out int numberOfSpacing)
        {
            length = length.RoundByDecimalPlace(6);
            maxSpacing = maxSpacing.RoundByDecimalPlace(6);
            numberOfSpacing = Convert.ToInt32(Math.Ceiling(length / maxSpacing));
            return length / (double)numberOfSpacing;
        }

        public static int RoundMultiple(this double d, int i)
        {
            return i * Convert.ToInt32(d / (double)i);
        }

        public static int RoundMultipleUp(this double d, int i)
        {
            if (i == 0)
            {
                i = 1;
            }

            return i * Convert.ToInt32(Math.Ceiling(d / (double)i));
        }

        public static int RoundMultipleDown(this double d, int i)
        {
            if (i == 0)
            {
                i = 1;
            }

            return i * Convert.ToInt32(Math.Floor(d / (double)i));
        }

        public static double RoundMultipleUpFeet(this double d, int i)
        {
            if (i == 0)
            {
                i = 1;
            }

            return ((double)d.FootToMm().RoundByDecimalPlace(3).RoundMultipleUp(i)).MmToFoot();
        }

        public static double RoundMultipleDownFeet(this double d, int i)
        {
            if (i == 0)
            {
                i = 1;
            }

            return ((double)d.FootToMm().RoundByDecimalPlace(3).RoundMultipleDown(i)).MmToFoot();
        }

        public static int RoundDivide(this double number, double divideBy)
        {
            return (int)(number.RoundByDecimalPlace(6) / divideBy.RoundByDecimalPlace(6)) + 1;
        }

        public static bool IsEqual(this double A, double B, double tolerance = 1E-09)
        {
            return Math.Abs(B - A) < tolerance;
        }

        public static bool IsZero(this double A)
        {
            return 0.0.IsEqual(A);
        }

        public static double NormalizeAngle(this double value)
        {
            double num = value;
            while (num.IsSmallerEqual(Math.PI * -2.0) || num.IsGreaterEqual(Math.PI * 2.0))
            {
                num = ((num.IsEqual(Math.PI * 2.0) || num.IsEqual(Math.PI * -2.0)) ? 0.0 : (num - Math.Floor(num / (Math.PI * 2.0)) * Math.PI * 2.0));
            }

            return num;
        }

        public static bool IsSmaller(this double A, double B, double tolerance = 1E-09)
        {
            return A + tolerance < B;
        }

        public static bool IsSmallerEqual(this double A, double B, double tolerance = 1E-09)
        {
            return A + tolerance >= B ? Math.Abs(B - A) < tolerance : true;
        }

        public static bool IsGreater(this double A, double B, double tolerance = 1E-09)
        {
            return A > B + tolerance;
        }

        public static bool IsGreaterEqual(this double A, double B, double tolerance = 1E-09)
        {
            return Math.Abs(B - A) >= tolerance ? A > B + tolerance : true;
        }

        public static bool IsBetweenEqual(this double a, double min, double max, double tol = 1E-09)
        {
            return a.IsGreaterEqual(min, tol) && a.IsSmallerEqual(max, tol);
        }

        public static bool IsBetween(this double a, double min, double max, double tol = 1E-09)
        {
            return a.IsGreater(min, tol) && a.IsSmaller(max, tol);
        }

        public static double Min(this double A, double B, double tolerance = 1E-09)
        {
            return !A.IsSmaller(B, tolerance) ? B : A;
        }

        public static double Max(double A, double B, double tolerance = 1E-09)
        {
            return !A.IsGreater(B, tolerance) ? B : A;
        }

        public static double MmToFoot(this double mm)
        {
            return mm / 304.79999999999995;
        }

        public static double MmToFoot(this int mm)
        {
            return (double)mm / 304.79999999999995;
        }

        public static double MeterToFoot(this double metter)
        {
            return metter / 0.30479999999999996;
        }

        public static double MeterToFoot(this int metter)
        {
            return (double)metter / 0.30479999999999996;
        }

        public static double FootToMm(this double feet)
        {
            return feet * 304.79999999999995;
        }

        public static double FootToMet(this double feet)
        {
            return feet * 304.79999999999995 / 1000.0;
        }

        public static double CubicFootToCubicMeter(this double cubicFoot)
        {
            return cubicFoot * 0.02831684659199999;
        }

        public static double SquareFootToSquareMeter(this double squareFoot)
        {
            return squareFoot * 0.092903039999999978;
        }

        public static double RadiansToDegrees(this double rads)
        {
            return rads * (180.0 / Math.PI);
        }

        public static double DegreesToRadians(this double degrees)
        {
            return degrees * (Math.PI / 180.0);
        }

        public static double RoundByDecimalPlace(this double number, int decimalPlace)
        {
            return Math.Round(number, decimalPlace);
        }

        public static double Round2Number(this double number)
        {
            return Math.Round(number, 2);
        }

        public static double RoundMilimet(this double feet, double roundMm, bool isRoundUp = true)
        {
            if (roundMm.IsEqual(0.0))
            {
                return feet;
            }

            double num = feet.FootToMm();
            num = ((!isRoundUp) ? (Math.Floor(num / roundMm) * roundMm) : (Math.Ceiling(num / roundMm) * roundMm));
            return num.MmToFoot();
        }

        public static double RoundDown(this double number, int step)
        {
            return Math.Floor(number / (double)step) * (double)step;
        }

        public static double RoundUp(this double number, int step)
        {
            return Math.Ceiling(number / (double)step) * (double)step;
        }
        public static bool IsNumber(this string text)
        {
            try
            {
                Regex regex = new Regex(@"^[-+]?[0-9]*\.?[0-9]+$");
                return regex.IsMatch(text);
            }
            catch (Exception)
            {
            }
            return false;
        }
        public static int GetIntegerFromText(this string text)
        {
            var resultString = Regex.Match(text, @"\d+").Value;
            return !string.IsNullOrEmpty(resultString) ? int.Parse(resultString) : 0;
        }
        public static int DivInterger(this double num1, double num2, out double du)
        {
            du = num1 % num2;
            var per = du * 100 / num2;
            var n = int.Parse(Math.Round((num1 - du) / num2, 0).ToString(), System.Globalization.NumberStyles.Integer);
            return per <= 20 ? n : n + 1;
        }
        public static int SolveNumber(this double num, double spacing)
        {
            var d = num % spacing;
            var per = d * 100 / spacing;
            var n = int.Parse($"{Math.Round((num - d) / spacing, 0)}");
            return per > 20 ? n + 1 : n;
        }
        public static int SolveNumber(this int num, int spacing)
        {
            var d = num % spacing;
            var per = d * 100 / spacing;
            var n = (num - d) / spacing;
            return d > 0 ? n + 1 : n;
        }
        public static int FindInterger(this string t)
        {
            var re = new Regex(@"\d+");
            var vl = re.Match(t);
            return string.IsNullOrEmpty(vl.Value) ? 0 : int.Parse(vl.Value);
        }
    }
}
