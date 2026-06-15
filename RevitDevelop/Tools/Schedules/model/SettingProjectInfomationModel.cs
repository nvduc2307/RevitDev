using System.Collections.ObjectModel;

namespace RevitDevelop.Tools.Schedules.model
{
    public class SettingProjectInfomationModelUI : ObservableObject
    {
        private string _pathFolderModel;
        public string PathFolderModel
        {
            get => _pathFolderModel;
            set
            {
                _pathFolderModel = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<ProjectInfomationModelUI> _projectInfomations { get; set; }
        public ObservableCollection<ProjectInfomationModelUI> ProjectInfomations
        {
            get => _projectInfomations;
            set
            {
                _projectInfomations = value;
                OnPropertyChanged();
            }
        }
    }
    public class SettingProjectInfomationModel
    {
        public string PathFolderModel { get; set;  }
        public List<ProjectInfomationModel> ProjectInfomations { get; set; }
    }
}
