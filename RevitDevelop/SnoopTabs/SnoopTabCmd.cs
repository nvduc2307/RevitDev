using Autodesk.Revit.Attributes;
using Autodesk.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.PlatformUI;
using Nice3point.Revit.Toolkit.External;
using RevitDevelop.Utils.Messages;
using RevitDevelop.Utils.RevDockablePanes;
using UIFramework;
using Xceed.Wpf.AvalonDock.Controls;
using Xceed.Wpf.AvalonDock.Layout;

namespace RevitDevelop.SnoopTabs
{
    [Transaction(TransactionMode.Manual)]
    public class SnoopTabCmd : ExternalCommand
    {
        public override void Execute()
        {

            using (var tsg = new TransactionGroup(Document, "new Command"))
            {
                tsg.Start();
                try
                {
                    var service = new ServiceCollection();
                    service.AddSingleton<SnoopTabCmd>();
                    var provider = service.BuildServiceProvider();
                    GetItemOfProjectBrowser();
                    tsg.Assimilate();
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException) { }
                catch (Exception ex)
                {
                    IO.ShowWarning(ex.Message);
                    tsg.RollBack();
                }
            }

        }
        private void CreateTabCustomSettingProperties()
        {
            RibbonControl ribbon = ComponentManager.Ribbon;
            var modifyTabs = ribbon.Tabs;
            if (!modifyTabs.Any())
                return;
            foreach (var tab in modifyTabs)
            {
                if (tab.Title != "Add-Ins")
                    continue;
                tab.IsActive = true;
                foreach (var panel in tab.Panels)
                {
                    if (panel == null) continue;
                }
            }
        }
        public void GetItemOfProjectBrowser()
        {
            var propertiesId = DockablePaneIdType.ProjectBrowser.GetDockablePaneId();
            var propertiesPanel = UiApplication.GetDockablePane(propertiesId);
            var isExistedPanel = Autodesk.Revit.UI.DockablePane.PaneExists(propertiesId);
            if (isExistedPanel)
            {
                var docPanes = MainWindow.getMainWnd().FindDescendants<LayoutAnchorablePaneControl>();
                foreach (var docPane in docPanes)
                {
                    var itemSource = docPane.ItemsSource;
                    foreach (LayoutAnchorable item in itemSource)
                    {
                        if (item is not { }) continue;
                        if (item.Title.Contains("Project Browser"))
                        {
                            var content = item.Content;
                        }
                    }
                }
            }
        }
    }
}
