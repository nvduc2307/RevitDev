using RevitDevelop.DemoST.Command1.actions;
using RevitDevelop.DemoST.Command1.models;
using RevitDevelop.DemoST.Command1.views;
using System.Collections.ObjectModel;

namespace RevitDevelop.DemoST.Command1.viewModels
{
    public partial class Command1VM
    {
        private Command1Cmd _cmd;
        private Command1Action _action;
        public ObservableCollection<ProjectModel> Projects { get; set; }
        public Command1V MainView { get; set; }
        public Command1VM(Command1Cmd cmd, Command1Action action)
        {
            _cmd = cmd;
            _action = action;
            _action.Validate();
            Projects = _action.GetProjectModels();
            MainView = new Command1V() { DataContext = this};
        }
        [RelayCommand]
        private void LoadProject()
        {
            var docsLoad = _action.LoadProject();
            foreach (var doc in docsLoad)
            {
                Projects.Add(doc);
            }
        }
        [RelayCommand]
        private void Ok()
        {
            MainView.Close();
            _action.LoadFamilyIntoProject(Projects);
        }
        [RelayCommand]
        private void Cancel()
        {
            MainView.Close();
        }
    }
}
