using System.Text.RegularExpressions;

namespace RevitDevelop.Utils.Numbers
{
    public static class NumberHelper
    {
        public static double GetDoubleFromText(this string text)
        {
            Regex regex = new Regex(@"^[-+]?[0-9]*\.?[0-9]+$");
            var resultString = regex.Match(text).Value;
            return !string.IsNullOrEmpty(resultString) ? double.Parse(resultString) : 0.0;
        }
        public static int GetIntegerFromText(this string text)
        {
            var resultString = Regex.Match(text, @"\d+").Value;
            return !string.IsNullOrEmpty(resultString) ? int.Parse(resultString) : 0;
        }
    }
}
