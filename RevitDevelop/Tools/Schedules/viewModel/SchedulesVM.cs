using RevitDevelop.Tools.Schedules.model;
using System.Collections.ObjectModel;

namespace RevitDevelop.Tools.Schedules.viewModel
{
    public class SchedulesVM : ObservableObject
    {
        private string _pathFileOutput;
        public string PathFileOutput
        {
            get => _pathFileOutput;
            set
            {
                _pathFileOutput = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<ScheduleDocument> _modelProjects;
        public ObservableCollection<ScheduleDocument> ModelProjects
        {
            get => _modelProjects;
            set
            {
                _modelProjects = value;
                OnPropertyChanged();
            }
        }
        private string _sheetNameWaterAndHotWateSupply;
        public string SheetNameWaterAndHotWateSupply
        {
            get => _sheetNameWaterAndHotWateSupply;
            set
            {
                _sheetNameWaterAndHotWateSupply = value;
                OnPropertyChanged();
            }
        }
        private string _scheduleNameWaterAndHotWateSupply;
        public string ScheduleNameWaterAndHotWateSupply
        {
            get => _scheduleNameWaterAndHotWateSupply;
            set
            {
                _scheduleNameWaterAndHotWateSupply = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<ScheduleSheetInExcelModelUI> _scheduleSheets;
        public ObservableCollection<ScheduleSheetInExcelModelUI> ScheduleSheets { 
            get => _scheduleSheets;
            set
            {
                _scheduleSheets = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand OnChooseFileOutputCmd { get; set; }
        public RelayCommand OnSettingMappingCmd { get; set; }
        public RelayCommand OnOkCmd { get; set; }
        public RelayCommand OnCancelCmd { get; set; }
        public RelayCommand OnSettingModelsCmd { get; set; }
        public RelayCommand OnNewSheetCmd { get; set; }
        public RelayCommand OnRemoveSheetCmd { get; set; }
        public RelayCommand OnSaveSheetsCmd { get; set; }
    }
}
