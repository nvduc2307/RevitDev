using Autodesk.Revit.UI;
using RevitDevelop.Tools.Schedules.model;
using RevitDevelop.Tools.Schedules.utils;
using RevitDevelop.Tools.Schedules.view;
using RevitDevelop.Tools.Schedules.viewModel;

namespace RevitDevelop.Tools.Schedules.action
{
    public partial class ScheduleAction
    {
        private UIApplication _uiApp;
        private UIDocument _uidocument;
        private Document _document;
        private ScheduleView _view;
        private SchedulesVM _viewModel;
        private List<MappingRecord> _mappingRecords;
        private List<ProjectRevitInfomationModelUI> _projectRevitInfomationModels;
        private List<ScheduleSheetInExcelToFillModel> _scheduleSheetInExcelToFillModel;

        private ScheduleExcelAction _scheduleExcelAction;
        private WriteScheduleWaterAndHotWateSupplyAction _scheduleWaterAndHotWateSupplyAction;
        public ScheduleAction(UIApplication uiApp)
        {
            _uiApp = uiApp;
            _uidocument = _uiApp.ActiveUIDocument;
            _document = _uidocument == null ? null : _uidocument.Document;
            _scheduleExcelAction = new ScheduleExcelAction();
            _projectRevitInfomationModels = new List<ProjectRevitInfomationModelUI>();
            _viewModel = new SchedulesVM()
            {
                ScheduleSetting = GetScheduleSettingDefault(),
                OnChooseFileOutputCmd = new RelayCommand(_OnChooseFileOutputCmd),
                OnChooseFileModelCmd = new RelayCommand(_OnChooseFileModelCmd),
                OnSettingMappingCmd = new RelayCommand(_OnSettingMappingCmd),
                OnOkCmd = new RelayCommand(_OnOkCmd),
                OnCancelCmd = new RelayCommand(_OnCancelCmd),
            };
            UpdateSheets();
            UpdateModelInSheet();
            _mappingRecords = ScheduleMappingUtils.GetMappingRecords();
            _scheduleWaterAndHotWateSupplyAction =
                new WriteScheduleWaterAndHotWateSupplyAction();
            _view = new ScheduleView() { DataContext = _viewModel };
        }
        public void Execute()
        {
            _view.ShowDialog();
        }
    }
}
