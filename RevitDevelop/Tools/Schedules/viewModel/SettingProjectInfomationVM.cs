using RevitDevelop.Tools.Schedules.model;

namespace RevitDevelop.Tools.Schedules.viewModel
{
    public class SettingProjectInfomationVM
    {
        public SettingProjectInfomationModelUI SettingProjectInfomation { get; set; }
        public RelayCommand SelectFolderModelsCmd { get; set; }
        public RelayCommand OnOkCmd { get; set; }
        public RelayCommand OnCancelCmd { get; set; }
        public RelayCommand OnAddModelsCmd { get; set; }
        public RelayCommand OnRemoveModelsCmd { get; set; }
        public RelayCommand OnResetCmd { get; set; }
    }
}
