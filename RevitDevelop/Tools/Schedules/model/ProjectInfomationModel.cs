namespace RevitDevelop.Tools.Schedules.model
{
    public class ProjectInfomationModelUI : ObservableObject
    {
        private string _projectNameInExcel;
        private List<ProjectRevitInfomationModelUI> _projectNameInRevits;
        private List<ProjectRevitInfomationModelUI> _projectNameInRevitsTarget;
        public string ProjectNameInExcel
        {
            get => _projectNameInExcel;
            set
            {
                _projectNameInExcel = value;
                OnPropertyChanged();
            }
        }
        private string _searchModelContain; 
        public string SearchModelContain
        {
            get => _searchModelContain;
            set
            {
                _searchModelContain = value;
                OnPropertyChanged();
                SearchModelContainAction?.Invoke(this);
            }
        }
        public Action<ProjectInfomationModelUI> SearchModelContainAction { get; set; }
        public List<ProjectRevitInfomationModelUI> ProjectNameInRevitsTarget
        {
            get => _projectNameInRevitsTarget;
            set
            {
                _projectNameInRevitsTarget = value;
                OnPropertyChanged();
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
                IsSelectedAction?.Invoke(this);
            }
        }
        public Action<ProjectRevitInfomationModelUI> IsSelectedAction { get; set; }
        public ProjectInfomationModelUI Parent { get; set; }
    }
}
