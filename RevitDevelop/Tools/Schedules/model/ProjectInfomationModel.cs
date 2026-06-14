namespace RevitDevelop.Tools.Schedules.model
{
    public class ProjectInfomationModelUI : ObservableObject
    {
        private string _projectNameInExcel;
        private ProjectRevitInfomationModel _projectNameInRevit;
        public string ProjectNameInExcel
        {
            get => _projectNameInExcel;
            set
            {
                _projectNameInExcel = value;
                OnPropertyChanged();
            }
        }
        public ProjectRevitInfomationModel ProjectNameInRevit
        {
            get => _projectNameInRevit;
            set
            {
                _projectNameInRevit = value;
                OnPropertyChanged();
            }
        }
        private List<ProjectRevitInfomationModel> _projectNameInRevits;
        public List<ProjectRevitInfomationModel> ProjectNameInRevits
        {
            get => _projectNameInRevits;
            set
            {
                _projectNameInRevits = value;
                OnPropertyChanged();
            }
        }
    }
    public class ProjectInfomationModel
    {
        public string ProjectNameInExcel { get; set; }
        public ProjectRevitInfomationModel ProjectNameInRevit { get; set; }
        public List<ProjectRevitInfomationModel> ProjectNameInRevits { get; set; }
    }
    public class ProjectRevitInfomationModel
    {
        public string Name { get; set; }
        public string Path { get; set; }
    }
}
