using Autodesk.Windows;
using Microsoft.VisualStudio.PlatformUI;
using RevitDevelop.Resources;
using RevitDevelop.TestDamper.commands;
using RevitDevelop.Utils.RevDockablePanes;
using RevitDevelop.Utils.RevRibbons;
using UIFramework;
using Xceed.Wpf.AvalonDock.Controls;
using Xceed.Wpf.AvalonDock.Layout;

namespace RevitDevelop.TestDamper.actions
{
    public class TestDamperAction
    {
        private const string _panelName = "Mode";
        private const string _panelPreviusId = "create_shr";
        private const string _tabParentId = "Modify";
        private TestDamperCmd _cmd;
        public TestDamperAction(TestDamperCmd cmd)
        {
            _cmd = cmd;
        }
        public void RemoveTabCustomSettingProperties()
        {
            RibbonControl ribbon = ComponentManager.Ribbon;
            var modifyTabs = ribbon.Tabs
                .Where(t =>
                t.Id.ToString() == _tabParentId);
            if (!modifyTabs.Any())
                return;
            var cancelCommand = new ExternalCommandCustom();
            cancelCommand.CanExecuteChanged += CancelCommand_CanExecuteChanged;
            var finishCommand = new ExternalCommandCustom();
            finishCommand.CanExecuteChanged += FinishCommand_CanExecuteChanged;
            foreach (var tab in modifyTabs)
            {
                var tabTarget = tab.Panels.FirstOrDefault(x => x.Source.Title == _panelName);
                if (tabTarget != null)
                {
                    tab.Panels.Remove(tabTarget);
                    return;
                }
            }
        }
        public void CreateTabCustomSettingProperties()
        {
            RibbonControl ribbon = ComponentManager.Ribbon;
            var modifyTabs = ribbon.Tabs
                .Where(t =>
                t.Id.ToString() == _tabParentId);
            if (!modifyTabs.Any())
                return;
            var cancelCommand = new ExternalCommandCustom();
            cancelCommand.CanExecuteChanged += CancelCommand_CanExecuteChanged;
            var finishCommand = new ExternalCommandCustom();
            finishCommand.CanExecuteChanged += FinishCommand_CanExecuteChanged;
            foreach (var tab in modifyTabs)
            {
                tab.IsActive = true;
                foreach (var panel in tab.Panels)
                {
                    if (panel == null) continue;
                    if (panel.Source.Id == _panelPreviusId)
                    {
                        var index = tab.Panels.IndexOf(panel);
                        var newPanel = new RibbonPanel();
                        newPanel.Source = new RibbonPanelSource();
                        newPanel.Source.Title = _panelName;
                        var ribbonRowPanel = new RibbonRowPanel();
                        var btnCancel = RevRibbonUtils.CreateRibbonButton(
                            StringDefines.IMG_COMMAND_CANCEL_16,
                            StringDefines.IMG_COMMAND_CANCEL_32,
                            cancelCommand);
                        var btnFinish = RevRibbonUtils.CreateRibbonButton(
                            StringDefines.IMG_COMMAND_FINISH_16,
                            StringDefines.IMG_COMMAND_FINISH_32,
                            finishCommand);
                        ribbonRowPanel.Items.Add(btnCancel);
                        ribbonRowPanel.Items.Add(new RibbonRowBreak());
                        ribbonRowPanel.Items.Add(btnFinish);
                        newPanel.Source.Items.Add(ribbonRowPanel);
                        tab.Panels.Insert(index + 1, newPanel);
                        return;
                    }
                }
            }
        }
        private void FinishCommand_CanExecuteChanged(object sender, EventArgs e)
        {
            IsEnalblePropertiesPanel(false);
        }
        private void CancelCommand_CanExecuteChanged(object sender, EventArgs e)
        {
            IsEnalblePropertiesPanel(true);
        }
        public void IsEnalblePropertiesPanel(bool status)
        {
            var propertiesId = DockablePaneIdType.Properties.GetDockablePaneId();
            var propertiesPanel = _cmd.UiApplication.GetDockablePane(propertiesId);
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
                        if (item.Title == propertiesPanel.GetTitle())
                        {
                            if (docPane.IsEnabled != status)
                                docPane.IsEnabled = status;
                            return;
                        }
                    }
                }
            }
        }
        public void GetItemOfProjectBrowser()
        {
            var propertiesId = DockablePaneIdType.ProjectBrowser.GetDockablePaneId();
            var propertiesPanel = _cmd.UiApplication.GetDockablePane(propertiesId);
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
                        if (item.Title.Contains("プロジェクト ブラウザ"))
                        {
                            var content = item.Content;
                        }
                    }
                }
            }
        }
    }
}
