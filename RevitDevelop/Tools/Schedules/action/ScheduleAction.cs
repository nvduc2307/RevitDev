using Autodesk.Revit.UI;
using Newtonsoft.Json;
using RevitDevelop.Tools.Schedules.model;
using RevitDevelop.Tools.Schedules.utils;
using RevitDevelop.Tools.Schedules.view;
using RevitDevelop.Tools.Schedules.viewModel;
using RevitDevelop.Utils;
using System.Collections.ObjectModel;
using System.IO;

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
        private List<ProjectInfomationModelUI> _projectInfomationModels;
        public ScheduleAction(UIApplication uiApp)
        {
            _uiApp = uiApp;
            _uidocument = _uiApp.ActiveUIDocument;
            _document = _uidocument == null ? null : _uidocument.Document;
            _projectInfomationModels = GetProjectInfomationModels();
            _viewModel = new SchedulesVM()
            {
                ScheduleSheets = GetScheduleSheets(),
                SheetNameWaterAndHotWateSupply = "給水・給湯",
                ScheduleNameWaterAndHotWateSupply = "フレキシブル配管集計2,配管集計2,配管継手集計エルボ樹脂管用2,配管継手集計エルボ樹脂管以外2",
                ModelProjects = ScheduleDocumentUtils.GetDefault(_document),
                OnChooseFileOutputCmd = new RelayCommand(_OnChooseFileOutputCmd),
                OnSettingMappingCmd = new RelayCommand(_OnSettingMappingCmd),
                OnOkCmd = new RelayCommand(_OnOkCmd),
                OnSettingModelsCmd = new RelayCommand(_OnSettingModelsCmd),
                OnCancelCmd = new RelayCommand(_OnCancelCmd),
                OnNewSheetCmd = new RelayCommand(_OnNewSheetCmd),
                OnRemoveSheetCmd = new RelayCommand(_OnRemoveSheetCmd),
                OnSaveSheetsCmd = new RelayCommand(_OnSaveSheetsCmd),
            };
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
