using Newtonsoft.Json;
using RevitDevelop.Tools.Schedules.model;
using RevitDevelop.Tools.Schedules.utils;
using RevitDevelop.Utils;
using System.IO;
using System.Windows.Forms;

namespace RevitDevelop.Tools.Schedules.action
{
    public partial class ScheduleAction
    {
        private void _OnSaveSheetsCmd()
        {
            if (!_viewModel.ScheduleSetting.ScheduleSheets.Any()) return;
            var data = new List<ScheduleSheetInExcelModel>();
            try
            {
                var path = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\daito_schedule_sheets.json";
                var pathTemplate = $"{PathUtils.FolderTemplate}\\daito_schedule_sheets.json";
                if (!File.Exists(path)) File.Copy(pathTemplate, path, true);
                if (!File.Exists(path)) throw new Exception($"File: {path} is not found");
                foreach (var sheet in _viewModel.ScheduleSetting.ScheduleSheets)
                {
                    var item = new ScheduleSheetInExcelModel
                    {
                        SheetName = sheet.SheetName,
                        ScheduleNameInRevit = sheet.ScheduleNameInRevit,
                        //ScheduleNameInRevits = [.. sheet.ScheduleNameInRevits],
                        ProjectInfomationModels = new List<ProjectInfomationModel>()
                    };
                    foreach (var projectInfomationModel in sheet.ProjectInfomationModels)
                    {
                        var item1 = new ProjectInfomationModel();
                        item1.ProjectNameInExcel = projectInfomationModel.ProjectNameInExcel;
                        item1.ProjectNameInRevits = projectInfomationModel.ProjectNameInRevits
                            .Select(x=>new ProjectRevitInfomationModel() { Name = x.Name, Path = x.Path, IsSelected = x.IsSelected})
                            .ToList();
                        item.ProjectInfomationModels.Add(item1);
                    }
                    data.Add(item);
                }
                var content = JsonConvert.SerializeObject(data);
                File.WriteAllText(path, content);
            }
            catch (Exception ex)
            {
                IO.ShowWarning(ex.Message);
            }
        }
        private void _OnRemoveSheetCmd()
        {
            if (!_viewModel.ScheduleSetting.ScheduleSheets.Any()) return;
            var index = _view.TabScheduleSheets.SelectedIndex == -1
                ? _viewModel.ScheduleSetting.ScheduleSheets.Count - 1
                : _view.TabScheduleSheets.SelectedIndex;
            _viewModel.ScheduleSetting.ScheduleSheets.RemoveAt(index);
        }
        private void _OnNewSheetCmd()
        {
            if (_viewModel.ScheduleSetting.ScheduleSheets == null) return;
            var qty = _viewModel.ScheduleSetting.ScheduleSheets.Count;
            var newSheet = CreateNewSheet($"sheet{qty + 1}");
            _viewModel.ScheduleSetting.ScheduleSheets.Add(newSheet);
            _view.TabScheduleSheets.SelectedIndex = qty;
        }
        private void _ProjectNameInExcelAction(ProjectInfomationModelUI uI)
        {
            
        }
        private void _OnSettingModelsCmd()
        {
            
        }
        private void _OnCancelCmd()
        {
            _view.Close();
        }
        private void _OnOkCmd()
        {
            
        }
        private void _OnSettingMappingCmd()
        {
            _view.Hide();
            try
            {
                var mappoingAction = new ScheduleMappingAction(_uidocument);
                mappoingAction.Execute();
            }
            catch (Exception)
            {
            }
            finally
            {
                _mappingRecords = ScheduleMappingUtils.GetMappingRecords();
                _view.ShowDialog();
            }
        }
        private void _OnRemoveModelSheetCmd(ScheduleSheetInExcelModelUI uI)
        {
            if (uI.ProjectInfomationModels.Any())
            {
                var qty = uI.ProjectInfomationModels.Count;
                uI.ProjectInfomationModels.RemoveAt(qty - 1);
            }
        }
        private void _OnAddModelSheetCmd(ScheduleSheetInExcelModelUI uI)
        {

        }
        private void _OnChooseFileOutputCmd()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
            };
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;
            _viewModel.ScheduleSetting.PathOutput = openFileDialog.FileName;
        }
        private void _OnChooseFileModelCmd()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "Excel Files (*.rvt)|*.rvt|All Files (*.*)|*.*",
            };
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;
            _viewModel.ScheduleSetting.PathModels = openFileDialog.FileName;
        }
    }
}
