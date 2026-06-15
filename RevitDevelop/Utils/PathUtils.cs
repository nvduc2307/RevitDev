using System.Reflection;

namespace RevitDevelop.Utils
{
    public static class PathUtils
    {
        public static string FolderTemplate
        {
            get => $"{AssemblyDirectory}\\Resources\\Templates";
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
        public static string DesktopFolder
        {
            get
            {
                var folder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                return folder;
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
