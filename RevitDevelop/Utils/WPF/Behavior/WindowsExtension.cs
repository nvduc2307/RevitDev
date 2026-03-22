using RevitDevelop.Utils.Compares;
using System.Windows;

namespace RevitDevelop.Utils.WPF.Behavior
{
    public static class WindowsExtension
    {
        public static void Escape(this Window window)
        {
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            new System.Windows.Interop.WindowInteropHelper(window) { Owner = Autodesk.Windows.ComponentManager.ApplicationWindow };
            window.PreviewKeyDown += (s, e) => { if (e.Key == System.Windows.Input.Key.Escape) window.Close(); };
        }
    }
}
