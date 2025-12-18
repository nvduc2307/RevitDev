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
                var layoutAnchorablePaneControl = MainWindow.getMainWnd().FindDescendants<LayoutAnchorablePaneControl>();
                var layerModifier = MainWindow.getMainWnd().FindDescendants<LayerModifier>();
                var layoutAnchorable = MainWindow.getMainWnd().FindDescendants<LayoutAnchorable>();
                var layoutAnchorableControl = MainWindow.getMainWnd().FindDescendants<LayoutAnchorableControl>();
                var layoutAnchorableFloatingWindow = MainWindow.getMainWnd().FindDescendants<LayoutAnchorableFloatingWindow>();
                var layoutAnchorableFloatingWindowControl = MainWindow.getMainWnd().FindDescendants<LayoutAnchorableFloatingWindowControl>();
                var layoutAnchorableItem = MainWindow.getMainWnd().FindDescendants<LayoutAnchorableItem>();
                var layoutAnchorablePane = MainWindow.getMainWnd().FindDescendants<LayoutAnchorablePane>();
                var layoutAnchorablePaneGroup = MainWindow.getMainWnd().FindDescendants<LayoutAnchorablePaneGroup>();
                var layoutAnchorablePaneGroupControl = MainWindow.getMainWnd().FindDescendants<LayoutAnchorablePaneGroupControl>();
                var layoutAnchorableTabItem = MainWindow.getMainWnd().FindDescendants<LayoutAnchorableTabItem>();
                var layoutAnchorControl = MainWindow.getMainWnd().FindDescendants<LayoutAnchorControl>();
                var layoutAnchorGroup = MainWindow.getMainWnd().FindDescendants<LayoutAnchorGroup>();
                var layoutAnchorGroupControl = MainWindow.getMainWnd().FindDescendants<LayoutAnchorGroupControl>();
                var layoutAnchorSide = MainWindow.getMainWnd().FindDescendants<LayoutAnchorSide>();
                var layoutAnchorSideControl = MainWindow.getMainWnd().FindDescendants<LayoutAnchorSideControl>();
                var layoutAutoHideWindowControl = MainWindow.getMainWnd().FindDescendants<LayoutAutoHideWindowControl>();
                var layoutContent = MainWindow.getMainWnd().FindDescendants<LayoutContent>();
                var layoutDocument = MainWindow.getMainWnd().FindDescendants<LayoutDocument>();
                var layoutDocumentControl = MainWindow.getMainWnd().FindDescendants<LayoutDocumentControl>();
                var layoutDocumentFloatingWindow = MainWindow.getMainWnd().FindDescendants<LayoutDocumentFloatingWindow>();
                var layoutDocumentFloatingWindowControl = MainWindow.getMainWnd().FindDescendants<LayoutDocumentFloatingWindowControl>();
                var layoutDocumentItem = MainWindow.getMainWnd().FindDescendants<LayoutDocumentItem>();
                var layoutDocumentPane = MainWindow.getMainWnd().FindDescendants<LayoutDocumentPane>();
                var layoutDocumentPaneControl = MainWindow.getMainWnd().FindDescendants<LayoutDocumentPaneControl>();
                var layoutDocumentPaneGroup = MainWindow.getMainWnd().FindDescendants<LayoutDocumentPaneGroup>();
                var layoutDocumentPaneGroupControl = MainWindow.getMainWnd().FindDescendants<LayoutDocumentPaneGroupControl>();
                var layoutDocumentTabItem = MainWindow.getMainWnd().FindDescendants<LayoutDocumentTabItem>();
                var layoutElement = MainWindow.getMainWnd().FindDescendants<LayoutElement>();
                var layoutElementEventArgs = MainWindow.getMainWnd().FindDescendants<LayoutElementEventArgs>();
                var layoutFloatingWindow = MainWindow.getMainWnd().FindDescendants<LayoutFloatingWindow>();
                var layoutFloatingWindowControl = MainWindow.getMainWnd().FindDescendants<LayoutFloatingWindowControl>();
                var layoutGridResizerControl = MainWindow.getMainWnd().FindDescendants<LayoutGridResizerControl>();
                var layoutGroupBase = MainWindow.getMainWnd().FindDescendants<LayoutGroupBase>();
                var layoutItem = MainWindow.getMainWnd().FindDescendants<LayoutItem>();
                var layoutPanel = MainWindow.getMainWnd().FindDescendants<LayoutPanel>();
                var layoutPanelControl = MainWindow.getMainWnd().FindDescendants<LayoutPanelControl>();
                var layoutRoot = MainWindow.getMainWnd().FindDescendants<LayoutRoot>();
                var layoutRule = MainWindow.getMainWnd().FindDescendants<LayoutRule>();
                var layoutRuleClearSpacing = MainWindow.getMainWnd().FindDescendants<LayoutRuleClearSpacing>();
                var layoutRuleFixedDistance = MainWindow.getMainWnd().FindDescendants<LayoutRuleFixedDistance>();
                var layoutRuleFixedNumber = MainWindow.getMainWnd().FindDescendants<LayoutRuleFixedNumber>();
                var layoutRuleMaximumSpacing = MainWindow.getMainWnd().FindDescendants<LayoutRuleMaximumSpacing>();

                //foreach (var docPane in docPanes)
                //{
                //    var itemSource = docPane.ItemsSource;
                //    foreach (LayoutAnchorable item in itemSource)
                //    {
                //        if (item is not { }) continue;
                //        if (item.Title.Contains("プロジェクト ブラウザ"))
                //        {
                //            var content = item.Content;
                //        }
                //    }
                //}
            }
        }
    }
}
