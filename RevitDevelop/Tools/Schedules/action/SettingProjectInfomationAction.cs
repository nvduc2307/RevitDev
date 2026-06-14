using Newtonsoft.Json;
using RevitDevelop.Tools.Schedules.model;
using RevitDevelop.Tools.Schedules.view;
using RevitDevelop.Tools.Schedules.viewModel;
using RevitDevelop.Utils;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Forms;

namespace RevitDevelop.Tools.Schedules.action
{
    public class SettingProjectInfomationAction
    {
        private ScheduleSettingModelView _view;
        private SettingProjectInfomationVM _viewModel;
        private List<ProjectRevitInfomationModel> _projects;
        public SettingProjectInfomationAction()
        {
            _projects = new List<ProjectRevitInfomationModel>();
            _viewModel = new SettingProjectInfomationVM()
            {
                SettingProjectInfomation = GetSettingProjectInfomation(),
                SelectFolderModelsCmd = new RelayCommand(_SelectFolderModelsCmd),
                OnOkCmd = new RelayCommand(_OnOkCmd),
                OnCancelCmd = new RelayCommand(_OnCancelCmd),
                OnAddModelsCmd = new RelayCommand(_OnAddModelsCmd),
                OnRemoveModelsCmd = new RelayCommand(_OnRemoveModelsCmd),
                OnResetCmd = new RelayCommand(_OnResetCmd),
            };
            _view = new ScheduleSettingModelView() { DataContext = _viewModel };
        }
        public void Execute()
        {
            _view.ShowDialog();
        }
        private void _OnResetCmd()
        {
            var pathTemplate = $"{PathUtils.FolderTemplate}\\daito_schedule_project_infomation.json";
            try
            {
                if (!File.Exists(pathTemplate)) throw new Exception($"File: {pathTemplate} is not found");
                var data = JsonConvert.DeserializeObject<SettingProjectInfomationModel>(File.ReadAllText(pathTemplate));
                _viewModel.SettingProjectInfomation.PathFolderModel = data.PathFolderModel;
                _viewModel.SettingProjectInfomation.ProjectInfomations = new ObservableCollection<ProjectInfomationModelUI>();
                _projects = new List<ProjectRevitInfomationModel>();
                foreach (var item in data.ProjectInfomations)
                {
                    var projectInfomationModelUI = new ProjectInfomationModelUI
                    {
                        ProjectNameInExcel = item.ProjectNameInExcel,
                        ProjectNameInRevits = item.ProjectNameInRevits
                    };
                    projectInfomationModelUI.ProjectNameInRevit =
                        projectInfomationModelUI.ProjectNameInRevits
                        .FirstOrDefault(x => x.Name == item.ProjectNameInRevit.Name);
                    _viewModel.SettingProjectInfomation.ProjectInfomations.Add(projectInfomationModelUI);
                }
            }
            catch (Exception ex)
            {
                IO.ShowWarning(ex.Message);
            }
        }
        private void _OnRemoveModelsCmd()
        {
            try
            {
                var dataModelProject = _view.dataModelProjects;
                if (!_viewModel.SettingProjectInfomation.ProjectInfomations.Any()) return;
                var qty = _viewModel.SettingProjectInfomation.ProjectInfomations.Count;
                var index = dataModelProject.SelectedIndex == -1 ? qty - 1 : dataModelProject.SelectedIndex;
                _viewModel.SettingProjectInfomation.ProjectInfomations.RemoveAt(index);
            }
            catch (Exception)
            {
            }
        }
        private void _OnAddModelsCmd()
        {
            var projectInfomationModelUI = new ProjectInfomationModelUI()
            {
                ProjectNameInExcel = "project name in excel",
                ProjectNameInRevits = [.. _projects]
            };
            projectInfomationModelUI.ProjectNameInRevit = projectInfomationModelUI.ProjectNameInRevits.FirstOrDefault();
            _viewModel.SettingProjectInfomation
                .ProjectInfomations.Add(projectInfomationModelUI);
        }
        private void _OnCancelCmd()
        {
            _view.Close();
        }
        private void _OnOkCmd()
        {
            try
            {
                var path = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\daito_schedule_project_infomation.json";
                var pathTemplate = $"{PathUtils.FolderTemplate}\\daito_schedule_project_infomation.json";
                if (!File.Exists(path)) File.Copy(pathTemplate, path, true);
                if (!File.Exists(path)) throw new Exception($"File: {path} is not found");
                if (string.IsNullOrEmpty(_viewModel.SettingProjectInfomation.PathFolderModel))
                    throw new Exception($"PathFolderModel is Empty");
                if (!Directory.Exists(_viewModel.SettingProjectInfomation.PathFolderModel))
                    throw new Exception($"Folder: {_viewModel.SettingProjectInfomation.PathFolderModel} is not found");
                if (!_viewModel.SettingProjectInfomation.ProjectInfomations.Any())
                    throw new Exception($"Projects Revit is not found");
                if (_viewModel.SettingProjectInfomation.ProjectInfomations.Any(x => string.IsNullOrEmpty(x.ProjectNameInExcel)))
                {
                    var itemTarget = _viewModel.SettingProjectInfomation.ProjectInfomations
                        .FirstOrDefault(x => string.IsNullOrEmpty(x.ProjectNameInExcel));
                    throw new Exception($"Setting At ${_viewModel.SettingProjectInfomation.ProjectInfomations.IndexOf(itemTarget)} is empty");
                }
                var data = new SettingProjectInfomationModel();
                data.PathFolderModel = _viewModel.SettingProjectInfomation.PathFolderModel;
                data.ProjectInfomations = new List<ProjectInfomationModel>();
                foreach (var item in _viewModel.SettingProjectInfomation.ProjectInfomations)
                {
                    var projectInfomationModel = new ProjectInfomationModel
                    {
                        ProjectNameInExcel = item.ProjectNameInExcel,
                        ProjectNameInRevits = item.ProjectNameInRevits
                    };
                    projectInfomationModel.ProjectNameInRevit =
                        projectInfomationModel.ProjectNameInRevits
                        .FirstOrDefault(x => x.Name == item.ProjectNameInRevit.Name);
                    data.ProjectInfomations.Add(projectInfomationModel);
                }
                File.WriteAllText(path, JsonConvert.SerializeObject(data));
                _view.Close();
            }
            catch (Exception ex)
            {
                IO.ShowWarning(ex.Message);
            }
        }
        private void _SelectFolderModelsCmd()
        {
            var dir = GetFolderOutputDefault();
            var folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog
            {
                SelectedPath = @"C:\",
                ShowNewFolderButton = false
            };
            var resultDialog = folderBrowserDialog.ShowDialog();
            if (resultDialog == DialogResult.OK)
                dir = folderBrowserDialog.SelectedPath;
            _viewModel.SettingProjectInfomation.PathFolderModel = dir;
            _projects = GetProjectRevitInfomationModels(dir);
            if (_viewModel.SettingProjectInfomation == null) return;
            foreach (var item in _viewModel.SettingProjectInfomation.ProjectInfomations)
            {
                item.ProjectNameInRevits = [.. _projects];
                item.ProjectNameInRevit = item.ProjectNameInRevit == null
                    ? item.ProjectNameInRevits.FirstOrDefault()
                    : item.ProjectNameInRevits
                    .FirstOrDefault(x => x.Name == item.ProjectNameInRevit.Name)
                    ?? item.ProjectNameInRevits.FirstOrDefault();
            }
        }
        public string GetFolderOutputDefault()
        {
            var pathFolderDefault = $"{PathUtils.DesktopFolder}\\Models";
            if (!System.IO.Directory.Exists(pathFolderDefault))
                System.IO.Directory.CreateDirectory(pathFolderDefault);
            return pathFolderDefault;
        }
        private SettingProjectInfomationModelUI GetSettingProjectInfomation()
        {
            var path = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\daito_schedule_project_infomation.json";
            SettingProjectInfomationModelUI result = null;
            result = new SettingProjectInfomationModelUI
            {
                PathFolderModel = string.Empty,
                ProjectInfomations = new ObservableCollection<ProjectInfomationModelUI>()
            };
            if (File.Exists(path))
            {
                try
                {
                    var data = JsonConvert.DeserializeObject<SettingProjectInfomationModel>(File.ReadAllText(path));
                    result.PathFolderModel = data.PathFolderModel;
                    if (data.ProjectInfomations.Any())
                        _projects = [.. data.ProjectInfomations.FirstOrDefault().ProjectNameInRevits];
                    foreach (var item in data.ProjectInfomations)
                    {
                        var projectInfomationModelUI = new ProjectInfomationModelUI
                        {
                            ProjectNameInExcel = item.ProjectNameInExcel,
                            ProjectNameInRevits = item.ProjectNameInRevits
                        };
                        projectInfomationModelUI.ProjectNameInRevit =
                            projectInfomationModelUI.ProjectNameInRevits
                            .FirstOrDefault(x => x.Name == item.ProjectNameInRevit.Name);
                        result.ProjectInfomations.Add(projectInfomationModelUI);
                    }
                }
                catch (Exception)
                {
                }
            }
            return result;
        }
        private List<ProjectRevitInfomationModel> GetProjectRevitInfomationModels(string dir)
        {
            var result = new List<ProjectRevitInfomationModel>();
            if (!Directory.Exists(dir)) return result;
            var files = Directory.GetFiles(dir);
            if (!files.Any()) return result;
            var fileRevits = files
                .Where(x => x.Contains(".rvt"))
                .ToList();
            if (!fileRevits.Any()) return result;
            fileRevits = fileRevits.OrderBy(x => x).ToList();
            foreach (var item in fileRevits)
            {
                var projectRevitInfomationModel = new ProjectRevitInfomationModel
                {
                    Name = Path.GetFileName(item),
                    Path = item
                };
                result.Add(projectRevitInfomationModel);
            }
            return result;
        }
    }
}
