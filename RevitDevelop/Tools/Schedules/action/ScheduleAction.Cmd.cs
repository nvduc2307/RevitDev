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
            if (!_viewModel.ScheduleSheets.Any()) return;
            var data = new List<ScheduleSheetInExcelModel>();
            try
            {
                var path = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\daito_schedule_sheets.json";
                var pathTemplate = $"{PathUtils.FolderTemplate}\\daito_schedule_sheets.json";
                if (!File.Exists(path)) File.Copy(pathTemplate, path, true);
                if (!File.Exists(path)) throw new Exception($"File: {path} is not found");
                foreach (var sheet in _viewModel.ScheduleSheets)
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
                        item1.ProjectNameInRevit = projectInfomationModel.ProjectNameInRevit;
                        item1.ProjectNameInExcels = [.. projectInfomationModel.ProjectNameInExcels];
                        item1.ProjectNameInRevits = [.. projectInfomationModel.ProjectNameInRevits];
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
            if (!_viewModel.ScheduleSheets.Any()) return;
            var index = _view.TabScheduleSheets.SelectedIndex == -1
                ? _viewModel.ScheduleSheets.Count - 1
                : _view.TabScheduleSheets.SelectedIndex;
            _viewModel.ScheduleSheets.RemoveAt(index);
        }
        private void _OnNewSheetCmd()
        {
            if (_viewModel.ScheduleSheets == null) return;
            var qty = _viewModel.ScheduleSheets.Count;
            var newSheet = CreateNewSheet($"sheet{qty + 1}");
            _viewModel.ScheduleSheets.Add(newSheet);
            _view.TabScheduleSheets.SelectedIndex = qty;
        }
        private void _ProjectNameInExcelAction(ProjectInfomationModelUI uI)
        {
            var path = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\daito_schedule_project_infomation.json";
            if (!File.Exists(path)) return;
            var data = JsonConvert.DeserializeObject<SettingProjectInfomationModel>(File.ReadAllText(path));
            if (data == null) return;
            var dataTarget = data.ProjectInfomations.FirstOrDefault(x => x.ProjectNameInExcel == uI.ProjectNameInExcel);
            if (dataTarget == null) return;
            uI.ProjectNameInExcelAction = null;
            uI.ProjectNameInExcels = [.. dataTarget.ProjectNameInExcels];
            uI.ProjectNameInRevits = [.. dataTarget.ProjectNameInRevits];
            uI.ProjectNameInExcel = uI.ProjectNameInExcels.FirstOrDefault(x => x == dataTarget.ProjectNameInExcel);
            uI.ProjectNameInRevit = uI.ProjectNameInRevits.FirstOrDefault(x => x.Name == dataTarget.ProjectNameInRevit.Name);
            uI.ProjectNameInExcelAction = _ProjectNameInExcelAction;
        }
        private void _OnSettingModelsCmd()
        {
            _view.Hide();
            var action = new SettingProjectInfomationAction();
            action.Execute();
            _projectInfomationModels = GetProjectInfomationModels();
            _viewModel.ScheduleSheets = GetScheduleSheets();
            _view.TabScheduleSheets.SelectedIndex = 0;
            _view.ShowDialog();
        }
        private void _OnCancelCmd()
        {
            _view.Close();
        }
        private void _OnOkCmd()
        {
            _scheduleWaterAndHotWateSupplyAction
                .Execute(
                _viewModel.PathFileOutput,
                _viewModel.SheetNameWaterAndHotWateSupply,
                _viewModel.ModelProjects.ToList(),
                _viewModel.ScheduleNameWaterAndHotWateSupply.Split(',').ToList(),
                _mappingRecords);
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
            var item1 = new ProjectInfomationModelUI
            {
                ProjectNameInExcels = [.. _projectInfomationModels.FirstOrDefault()?.ProjectNameInExcels ?? new List<string>()],
                ProjectNameInRevits = [.. _projectInfomationModels.FirstOrDefault()?.ProjectNameInRevits ?? new List<ProjectRevitInfomationModel>()],
                ProjectNameInExcelAction = _ProjectNameInExcelAction
            };
            item1.ProjectNameInExcel = item1.ProjectNameInExcels.FirstOrDefault();
            uI.ProjectInfomationModels.Add(item1);
        }
        private void _OnChooseFileOutputCmd()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
            };
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;
            _viewModel.PathFileOutput = openFileDialog.FileName;
        }
    }
}
