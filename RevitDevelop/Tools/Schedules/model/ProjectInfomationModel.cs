namespace RevitDevelop.Tools.Schedules.model
{
    public class ProjectInfomationModelUI : ObservableObject
    {
        private string _projectNameInExcel;
        private List<ProjectRevitInfomationModelUI> _projectNameInRevits;
        public string ProjectNameInExcel
        {
            get => _projectNameInExcel;
            set
            {
                _projectNameInExcel = value;
                OnPropertyChanged();
                ProjectNameInExcelAction?.Invoke(this);
            }
        }
        public List<ProjectRevitInfomationModelUI> ProjectNameInRevits
        {
            get => _projectNameInRevits;
            set
            {
                _projectNameInRevits = value;
                OnPropertyChanged();
            }
        }
        public Action<ProjectInfomationModelUI> ProjectNameInExcelAction { get; set;  }
    }
    public class ProjectInfomationModel
    {
        public string ProjectNameInExcel { get; set; }
        public List<ProjectRevitInfomationModel> ProjectNameInRevits { get; set; }
    }
    public class ProjectRevitInfomationModelUI : ObservableObject
    {
        public string Name { get; set; }
        public string Path { get; set; }
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }
    }
    public class ProjectRevitInfomationModel
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public bool IsSelected { get; set; }
    }
}
