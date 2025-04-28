using System.Text;
using System.Text.RegularExpressions;

namespace RevitDevelop.Utils.StringUtils
{
    public static class StringUtil
    {
        public static int GetInterger(this string textValue, string baseValue)
        {
            try
            {
                var num = 0;
                string pattern = $"{Regex.Escape(baseValue)} \\((\\d+)\\)";
                var regex = new Regex(pattern);
                var match = regex.Match(textValue);
                if (match.Success && match.Groups.Count > 1)
                    if (int.TryParse(match.Groups[1].Value, out int number))
                        num = number;
                return num;
            }
            catch (Exception)
            {
            }
            return 0;
        }
        public static bool CompareString(this string st1, string st2)
        {
            bool isEqual = string.Equals(
            st1.Normalize(NormalizationForm.FormKC).Replace(" ", ""),
            st2.Normalize(NormalizationForm.FormKC).Replace(" ", ""),
            StringComparison.OrdinalIgnoreCase);
            return isEqual;
        }
    }
}
