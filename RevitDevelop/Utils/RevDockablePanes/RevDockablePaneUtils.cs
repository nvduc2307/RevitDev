using Autodesk.Revit.UI;

namespace RevitDevelop.Utils.RevDockablePanes
{
    public static class RevDockablePaneUtils
    {
        public static DockablePaneId GetDockablePaneId(this DockablePaneIdType dockablePaneIdType)
        {
            var propertiesId = Autodesk.Revit.UI.DockablePanes.BuiltInDockablePanes.PropertiesPalette;
            switch (dockablePaneIdType)
            {
                case DockablePaneIdType.Properties:
                    propertiesId = Autodesk.Revit.UI.DockablePanes.BuiltInDockablePanes.PropertiesPalette;
                    break;
                case DockablePaneIdType.ProjectBrowser:
                    propertiesId = Autodesk.Revit.UI.DockablePanes.BuiltInDockablePanes.ProjectBrowser;
                    break;
            }
            return propertiesId;
        }
    }
    public enum DockablePaneIdType
    {
        Properties = 0,
        ProjectBrowser=1
    }
}
