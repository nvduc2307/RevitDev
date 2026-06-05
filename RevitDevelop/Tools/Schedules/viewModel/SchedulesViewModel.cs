using RevitDevelop.Tools.Schedules.model;
using System.Collections.ObjectModel;

namespace RevitDevelop.Tools.Schedules.viewModel
{
    public class SchedulesViewModel : ObservableObject
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
        private ObservableCollection<MepQuantityExportDocument> _modelProjects;
        public ObservableCollection<MepQuantityExportDocument> ModelProjects
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
        public RelayCommand OnChooseFileOutputCmd { get; set; }
        public RelayCommand OnAddModelCmd { get; set; }
        public RelayCommand OnRemoveModelCmd { get; set; }
        public RelayCommand OnSettingMappingCmd { get; set; }
        public RelayCommand OnOkCmd { get; set; }
        public RelayCommand OnCancelCmd { get; set; }
    }
}
