using RevitDevelop.Tools.Schedules.model;
using System.Collections.ObjectModel;

namespace RevitDevelop.Tools.Schedules.viewModel
{
    public class SchedulesVM
    {
        public ScheduleSettingModelUI ScheduleSetting { get; set; }
        public RelayCommand OnChooseFileModelCmd { get; set; }
        public RelayCommand OnChooseFileOutputCmd { get; set; }
        public RelayCommand OnSettingMappingCmd { get; set; }
        public RelayCommand OnOkCmd { get; set; }
        public RelayCommand OnCancelCmd { get; set; }
        public RelayCommand OnSettingModelsCmd { get; set; }
    }
}
