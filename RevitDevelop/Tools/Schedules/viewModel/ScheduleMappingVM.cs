using RevitDevelop.Tools.Schedules.model;

namespace RevitDevelop.Tools.Schedules.viewModel
{
    public class ScheduleMappingVM : ObservableObject
    {
        public List<MappingRecord> MappingRecordSettings { get; set; }
        public RelayCommand AddCommand { get; set; }
        public RelayCommand RemoveCommand { get; set; }
        public RelayCommand ExportCommand { get; set; }
        public RelayCommand ImportCommand { get; set; }
        public RelayCommand OkCommand { get; set; }
        public RelayCommand CancelCommand { get; set; }
    }
}
