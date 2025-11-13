using RevitDevelop.Tools.TestEdittorStatus.Actions;
using RevitDevelop.Tools.TestEdittorStatus.Models;
using RevitDevelop.Tools.TestEdittorStatus.Views;

namespace RevitDevelop.Tools.TestEdittorStatus.ViewModels
{
    public class TestEdittorStatuViewModel
    {
        private TestEdittorStatusCommand _cmd;
        private TestEdittorStatusAction _action;
        public List<ObjectStatus> Objects { get; set; }
        public TestEdittorStatusView MainView { get;private set; }
        public TestEdittorStatuViewModel(
            TestEdittorStatusCommand cmd,
            TestEdittorStatusAction action)
        {
            _cmd = cmd;
            _action = action;
            Objects = _action?.GetObjects();
            MainView = new TestEdittorStatusView() { DataContext = this};
            MainView.MouseMove += MainView_MouseMove;
        }

        private void MainView_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _action?.UpdateObjectStatus(Objects);
        }
    }
}
