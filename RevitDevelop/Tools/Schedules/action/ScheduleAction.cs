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
        private SchedulesViewModel _viewModel;
        private List<MappingRecord> _mappingRecords;
        private WriteScheduleWaterAndHotWateSupplyAction _scheduleWaterAndHotWateSupplyAction;
        public ScheduleAction(UIApplication uiApp)
        {
            _uiApp = uiApp;
            _uidocument = _uiApp.ActiveUIDocument;
            _document = _uidocument == null ? null : _uidocument.Document;
            _viewModel = new SchedulesViewModel()
            {
                SheetNameWaterAndHotWateSupply = "給水・給湯",
                ScheduleNameWaterAndHotWateSupply = "フレキシブル配管集計2,配管集計2,配管継手集計エルボ樹脂管用2,配管継手集計エルボ樹脂管以外2",
                ModelProjects = ScheduleDocumentUtils.GetDefault(_document),
                OnChooseFileOutputCmd = new RelayCommand(_OnChooseFileOutputCmd),
                OnAddModelCmd = new RelayCommand(_OnAddModelCmd),
                OnRemoveModelCmd = new RelayCommand(_OnRemoveModelCmd),
                OnSettingMappingCmd = new RelayCommand(_OnSettingMappingCmd),
                OnOkCmd = new RelayCommand(_OnOkCmd),
                OnCancelCmd = new RelayCommand(_OnCancelCmd),
            };
            _mappingRecords = ScheduleMappingUtils.GetMappingRecords();
            _scheduleWaterAndHotWateSupplyAction =
                new WriteScheduleWaterAndHotWateSupplyAction(_mappingRecords);
            _view = new ScheduleView() { DataContext = _viewModel };
        }
        public void Execute()
        {
            _view.ShowDialog();
        }
    }
}
