using Autodesk.Revit.Attributes;
using RevitDevelop.TestDamper.actions;
using RevitDevelop.TestDamper.views;

namespace RevitDevelop.TestDamper.viewModels
{
    public partial class TestDamperVM
    {
        private TestDamperCmd _cmd;
        private TestDamperAction _action;
        public TestDamperView MainView { get; set; }
        public TestDamperVM(TestDamperCmd cmd, TestDamperAction action)
        {
            _cmd = cmd;
            _action = action;
            MainView = new TestDamperView() { DataContext = this };
            MainView.Loaded += MainView_Loaded;
            MainView.Closed += MainView_Closed;
        }

        private void MainView_Closed(object sender, EventArgs e)
        {
            //_action.RemoveTabCustomSettingProperties();
            //_action.IsEnalblePropertiesPanel(true);
        }

        private void MainView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //_action.CreateTabCustomSettingProperties();
            _action.GetItemOfProjectBrowser();
        }

        [RelayCommand]
        private void Ok()
        {
            MainView.Close();
        }
    }
}
