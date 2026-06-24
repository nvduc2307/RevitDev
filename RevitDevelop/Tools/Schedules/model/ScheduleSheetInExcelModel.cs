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
        public RelayCommand<ScheduleSheetInExcelModelUI> OnAddModelCmd { get; set; }
        public RelayCommand<ScheduleSheetInExcelModelUI> OnRemoveModelCmd { get; set; }
    }
}
