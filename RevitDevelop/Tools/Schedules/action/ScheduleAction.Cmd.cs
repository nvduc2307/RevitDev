using Autodesk.Revit.UI;
using RevitDevelop.Tools.Schedules.model;
using RevitDevelop.Tools.Schedules.utils;
using RevitDevelop.Utils;
using System.IO;
using System.Windows.Forms;

namespace RevitDevelop.Tools.Schedules.action
{
    public partial class ScheduleAction
    {
        private void _IsSelectedAction(ProjectRevitInfomationModelUI target)
        {
            if (!target.IsSelected) return;
            foreach (var item in target.Parent.ProjectNameInRevits)
            {
                if (!item.IsSelected) continue;
                if (item.Name == target.Name) continue;
                item.IsSelected = !item.IsSelected;
            }
        }
        private void _SearchModelContainAction(ProjectInfomationModelUI uI)
        {
            if (string.IsNullOrEmpty(uI.SearchModelContain))
            {
                uI.ProjectNameInRevitsTarget = [.. uI.ProjectNameInRevits];
                return;
            }
            uI.ProjectNameInRevitsTarget = uI.ProjectNameInRevits.Where(x => x.Name.Contains(uI.SearchModelContain)).ToList();
        }
        private void _OnCancelCmd()
        {
            _view.Close();
        }
        private void _OnOkCmd()
        {
            try
            {
                SaveSetting();
                //var action = new WriteScheduleWaterAndHotWateSupplyAction();
                //var docs = new List<ScheduleDocument>()
                //{
                //    new ScheduleDocument() { Document = _document, Name = _document.Title, NameInExcel = "", Path = _document.PathName }
                //};
                //var nameScheduleInRevit = new List<string>()
                //{
                //    "フレキシブル配管集計2",
                //    "配管集計2",
                //    "配管継手集計エルボ樹脂管用2",
                //    "配管継手集計エルボ樹脂管以外2"
                //};
                //if (!File.Exists(_viewModel.ScheduleSetting.PathOutput)) return;
                //action.Execute(
                //    _viewModel.ScheduleSetting.PathOutput,
                //    "給水・給湯",
                //    docs,
                //    nameScheduleInRevit,
                //    _mappingRecords);
                //IO.ShowInfo("Complete");
                _view.Close();
            }
            catch (Exception ex)
            {
                IO.ShowWarning(ex.Message);
            }
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
            _scheduleSheetInExcelToFillModel = _scheduleExcelAction.GetSheets(_viewModel.ScheduleSetting.PathOutput);
            UpdateSheets();
            UpdateModelInSheet();
        }
        private void _OnChooseFileModelCmd()
        {
            var saveFileDialog = new System.Windows.Forms.FolderBrowserDialog
            {
                SelectedPath = @"C:\",
                ShowNewFolderButton = false
            };
            var resultDialog = saveFileDialog.ShowDialog();
            if (resultDialog != DialogResult.OK) return;
            _viewModel.ScheduleSetting.PathModels = saveFileDialog.SelectedPath;
            _projectRevitInfomationModels = GetProjectRevitInfomationModelUIDefault(_viewModel.ScheduleSetting.PathModels);
            UpdateSheets();
            UpdateModelInSheet();
        }
    }
}
