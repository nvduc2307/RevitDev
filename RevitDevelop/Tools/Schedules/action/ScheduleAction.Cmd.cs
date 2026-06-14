using ClosedXML.Excel;
using RevitDevelop.Tools.Schedules.model;
using RevitDevelop.Tools.Schedules.utils;
using System.IO;
using System.Windows.Forms;

namespace RevitDevelop.Tools.Schedules.action
{
    public partial class ScheduleAction
    {
        private void _OnSettingModelsCmd()
        {
            _view.Hide();
            var action = new SettingProjectInfomationAction();
            action.Execute();
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

        private void _OnRemoveModelCmd()
        {
            if (!_viewModel.ModelProjects.Any()) return;
            var item = _view.dataModelProjects;
            if (item == null) return;
            if (item.SelectedIndex < 0) return;
            if (!(item.SelectedItem is ScheduleDocument doc)) return;
            var elementIndex = _viewModel.ModelProjects.FirstOrDefault(x => x.Path == doc.Path && x.Name == doc.Name);
            if (elementIndex == null) return;
            _viewModel.ModelProjects.Remove(elementIndex);
        }

        private void _OnAddModelCmd()
        {
            var documents = _uiApp.Application.Documents;
            var openFile = new OpenFileDialog()
            {
                Filter = "Excel Files (*.rvt)|*.rvt|All Files (*.*)|*.*",
                Multiselect = true
            };
            if (openFile.ShowDialog() != DialogResult.OK) return;
            var pathFiles = openFile.FileNames;
            foreach (var pathFile in pathFiles)
            {
                if (!File.Exists(pathFile)) return;
                Document existingDoc = null;
                if (documents != null)
                {
                    foreach (Document document in documents)
                    {
                        if (!document.IsFamilyDocument &&
                                string.Equals(document.PathName, pathFile, StringComparison.OrdinalIgnoreCase))
                        {
                            existingDoc = document;
                            break;
                        }
                    }
                }
                if (_viewModel.ModelProjects.Any(x => x.Path == pathFile)) return;
                var doc = existingDoc ?? _uiApp.Application.OpenDocumentFile(pathFile);
                _viewModel.ModelProjects.Add(new ScheduleDocument() { Document = doc, Name = doc.Title, Path = doc.PathName });
            }
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
