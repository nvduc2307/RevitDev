using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using RevitDevelop.testHtml.views;
using System.Windows;
using System.Windows.Markup;

namespace RevitDevelop.testHtml
{
    [Transaction(TransactionMode.Manual)]
    public class testHtmlCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiapp = commandData.Application;
            AppEntry.UiApp = uiapp;
            // nếu đã mở thì focus
            if (HtmlWindow.Current != null) { HtmlWindow.Current.Activate(); return Result.Succeeded; }

            var w = new HtmlWindow(uiapp) { WindowStartupLocation = WindowStartupLocation.CenterOwner };
            w.Show(); // modeless
            return Result.Succeeded;
        }
    }
}
