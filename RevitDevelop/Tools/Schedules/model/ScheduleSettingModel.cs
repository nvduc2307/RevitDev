using System.Collections.ObjectModel;

namespace RevitDevelop.Tools.Schedules.model
{
    public class ScheduleSettingModelUI : ObservableObject
    {
        private string _pathOutput;
        private string _pathModels;
        private ObservableCollection<ScheduleSheetInExcelModelUI> _scheduleSheets;
        public string PathOutput
        {
            get => _pathOutput;
            set
            {
                _pathOutput = value;
                OnPropertyChanged();
            }
        }
        public string PathModels
        {
            get => _pathModels;
            set
            {
                _pathModels = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<ScheduleSheetInExcelModelUI> ScheduleSheets
        {
            get => _scheduleSheets;
            set
            {
                _scheduleSheets = value;
                OnPropertyChanged();
            }
        }
    }
}
