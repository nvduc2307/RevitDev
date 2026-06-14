using System.Collections.ObjectModel;

namespace RevitDevelop.Tools.Schedules.model
{
    public class ScheduleSheetInExcelModelUI : ObservableObject
    {
        private string _sheetName;
        private string _scheduleNameInRevit;
        private ObservableCollection<ProjectInfomationModelUI> _projectInfomationModels;
        public string SheetName
        {
            get => _sheetName;
            set
            {
                _sheetName = value;
                OnPropertyChanged();
            }
        }
        public string ScheduleNameInRevit
        {
            get => _scheduleNameInRevit;
            set
            {
                _scheduleNameInRevit = value;
                OnPropertyChanged();
                ScheduleNameInRevitAction?.Invoke(this);
            }
        }
        public ObservableCollection<ProjectInfomationModelUI> ProjectInfomationModels
        {
            get => _projectInfomationModels;
            set
            {
                _projectInfomationModels = value;
                OnPropertyChanged();
            }
        }
        public List<string> ScheduleNameInRevits { get; set; }
        public Action<ScheduleSheetInExcelModelUI> ScheduleNameInRevitAction {  get; set; }
        public RelayCommand<ScheduleSheetInExcelModelUI> OnAddModelCmd { get; set; }
        public RelayCommand<ScheduleSheetInExcelModelUI> OnRemoveModelCmd { get; set; }
    }
    public class ScheduleSheetInExcelModel
    {
        public string SheetName { get; set; }
        public string ScheduleNameInRevit { get; set; }
        public List<string> ScheduleNameInRevits { get; set; }
        public List<ProjectInfomationModel> ProjectInfomationModels { get; set; }
    }
}
