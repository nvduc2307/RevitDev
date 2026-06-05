using Autodesk.Revit.UI;
using Newtonsoft.Json;
using RevitDevelop.Tools.Schedules.utils;
using RevitDevelop.Tools.Schedules.view;
using RevitDevelop.Tools.Schedules.viewModel;
using RevitDevelop.Utils;
using System.IO;
using System.Windows.Forms;

namespace RevitDevelop.Tools.Schedules.action
{
    public class ScheduleMappingAction
    {

        private UIDocument _uidocument;
        private Document _document;
        private ScheduleMappingVM _viewModel;
        private ScheduleMappingView _view;
        public ScheduleMappingAction(UIDocument uidocument)
        {
            _uidocument = uidocument;
            _document = _uidocument.Document;
            _viewModel = new ScheduleMappingVM()
            {
                MappingRecordSettings = ScheduleMappingUtils.GetMappingRecords(),
                ExportCommand = new RelayCommand(_ExportCommand),
                ImportCommand = new RelayCommand(_ImportCommand),
                OkCommand = new RelayCommand(_OkCommand),
                CancelCommand = new RelayCommand(_CancelCommand),

            };
            _view = new ScheduleMappingView() { DataContext = _viewModel };
        }

        private void _CancelCommand()
        {
            _view.Close();
        }

        private void _OkCommand()
        {
            if (!_viewModel.MappingRecordSettings.Any()) return;
            var path = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\{ScheduleMappingUtils.fileMappingName}.json";
            var pathTemplate = $"{PathUtils.FolderTemplate}\\daito_schedule_mapping_record.json";
            if (!File.Exists(path)) File.Copy(pathTemplate, path, true);
            if (File.Exists(path))
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(_viewModel.MappingRecordSettings));
                _view.Close();
            }
        }

        private void _ImportCommand()
        {
            try
            {
                var fileOpen = new OpenFileDialog()
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
                };
                if (fileOpen.ShowDialog() == DialogResult.OK)
                {
                    var path = fileOpen.FileName;
                    _viewModel.MappingRecordSettings = ScheduleMappingUtils.GetMappingRecords(path);
                    IO.ShowInfo("", "Complete");
                }
            }
            catch (Exception)
            {
            }
        }

        private void _ExportCommand()
        {
            var fileSave = new SaveFileDialog()
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
                FileName = "（参考）ファミリ名称⇔設備内訳書資材名称_対比表（FB）"
            };
            if (fileSave.ShowDialog() == DialogResult.OK)
            {
                var path = fileSave.FileName;
                if (File.Exists(path))
                {
                    IO.ShowWarning("", "File is existed");
                    return;
                }
                var pathTemplate = $"{PathUtils.FolderTemplate}\\daito_schedule_mapping_record.xlsx";
                File.Copy(pathTemplate, path, true);
                ScheduleMappingUtils.ExportFileMappingRecord(path, _viewModel.MappingRecordSettings);
                IO.ShowInfo("", "Complete");
            }
        }

        public void Execute()
        {
            _view.ShowDialog();
        }
    }
}
