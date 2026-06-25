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
        private WriteScheduleWaterAndHotWateSupplyAction _scheduleWaterAndHotWateSupplyAction;
        private List<ProjectRevitInfomationModelUI> _projectRevitInfomationModels;
        public ScheduleAction(UIApplication uiApp)
        {
            _uiApp = uiApp;
            _uidocument = _uiApp.ActiveUIDocument;
            _document = _uidocument == null ? null : _uidocument.Document;
            _projectRevitInfomationModels = new List<ProjectRevitInfomationModelUI>();
            _viewModel = new SchedulesVM()
            {
                ScheduleSetting = GetScheduleSettingDefault(),
                OnChooseFileOutputCmd = new RelayCommand(_OnChooseFileOutputCmd),
                OnChooseFileModelCmd = new RelayCommand(_OnChooseFileModelCmd),
                OnSettingMappingCmd = new RelayCommand(_OnSettingMappingCmd),
                OnOkCmd = new RelayCommand(_OnOkCmd),
                OnCancelCmd = new RelayCommand(_OnCancelCmd),
                OnNewSheetCmd = new RelayCommand(_OnNewSheetCmd),
                OnRemoveSheetCmd = new RelayCommand(_OnRemoveSheetCmd),
                OnSaveSheetsCmd = new RelayCommand(_OnSaveSheetsCmd),
            };
            UpdateModelInSheet();
            _mappingRecords = ScheduleMappingUtils.GetMappingRecords();
            foreach (var sheet in _viewModel.ScheduleSetting.ScheduleSheets)
            {
                var index = _viewModel.ScheduleSetting.ScheduleSheets.IndexOf(sheet);
                if (index != 0) continue;
                sheet.SheetName = "給水・給湯";
                sheet.ScheduleNameInRevit = "フレキシブル配管集計2, 配管集計2, 配管継手集計エルボ樹脂管用2, 配管継手集計エルボ樹脂管以外2";
            }
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
