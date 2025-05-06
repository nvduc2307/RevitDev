using System.IO;
using System.Text;

namespace RevitDevelop.Utils.Directionaries
{
    public static class DirectionaryUtils
    {
        public static void CreateDirectory(this string pathFile)
        {
            var pathItems = pathFile.Split('\\').ToList();
            var qpathItems = pathItems.Count;
            var fileName = pathItems.LastOrDefault();
            var dir = pathItems
                .Where((x, index) => index != qpathItems - 1)
                .Aggregate((a, b) => $"{a}\\{b}");
            var isFileExisted = File.Exists(pathFile);
            var isExisted = Directory.Exists(dir);
            if (!isExisted) Directory.CreateDirectory(dir);

            if (!isFileExisted)
            {
                using (FileStream fs = File.Create(pathFile))
                {
                    char[] value = "[]".ToCharArray();
                    fs.Write(Encoding.UTF8.GetBytes(value), 0, value.Length);
                }
            }
        }
    }
}
