using System.Reflection;

namespace RevitDevelop.Utils.Paths
{
    public static class PathUtils
    {
        public static string PathDatas
        {
            get => $"{AssemblyDirectory}\\Resources\\Datas";
        }
        public static string FamiliesBase
        {
            get => $"{AssemblyDirectory}\\Resources\\FamiliesBases";
        }
        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return System.IO.Path.GetDirectoryName(path);
            }
        }
        public static string AppDataDirectory
        {
            get
            {
                return $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\";
            }
        }
    }
}
