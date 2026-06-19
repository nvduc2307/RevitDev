using Newtonsoft.Json;
using RevitDevelop.Tools.Schedules.model;
using RevitDevelop.Utils;
using System.Collections.ObjectModel;
using System.IO;

namespace RevitDevelop.Tools.Schedules.action
{
    public partial class ScheduleAction
    {
        private void UpdateModelInSheet()
        {
            foreach (var sheet in _viewModel.ScheduleSetting.ScheduleSheets)
            {
                foreach (var model in sheet.ProjectInfomationModels)
                {
                    model.ProjectNameInRevits = GetProjectRevitInfomationModelUI(model.ProjectNameInExcel);
                }
            }
        }
        private ScheduleSettingModelUI GetScheduleSetting()
        {
            var result = new ScheduleSettingModelUI();
            var path = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\daito_schedule_setting.json";
            var pathTemplate = $"{PathUtils.FolderTemplate}\\daito_schedule_setting.json";
            if (!File.Exists(pathTemplate)) return GetScheduleSettingDefault();
            if (!File.Exists(path)) File.Copy(pathTemplate, path);
            var data = JsonConvert.DeserializeObject<ScheduleSettingModel>(File.ReadAllText(path));
            if (data == null) return GetScheduleSettingDefault();
            result.PathOutput = data.PathOutput;
            result.PathModels = data.PathModels;
            _projectRevitInfomationModels = GetProjectRevitInfomationModelUIDefault(result.PathModels);
            result.ScheduleSheets = GetScheduleSheets(data.ScheduleSheets);
            if (data.ScheduleSheets == null)
                return GetScheduleSettingDefault();
            return result;
        }
        private ObservableCollection<ScheduleSheetInExcelModelUI> GetScheduleSheets(List<ScheduleSheetInExcelModel> dataSheets)
        {
            var result = new ObservableCollection<ScheduleSheetInExcelModelUI>();
            if (dataSheets == null) return result;
            if (!dataSheets.Any()) return result;
            foreach (var data in dataSheets)
            {
                var index = dataSheets.IndexOf(data);
                var schduleSheet = new ScheduleSheetInExcelModelUI
                {
                    SheetName = data.SheetName,
                    ScheduleNameInRevit = data.ScheduleNameInRevit,
                    ProjectInfomationModels = new ObservableCollection<ProjectInfomationModelUI>(),
                    OnAddModelCmd = new RelayCommand<ScheduleSheetInExcelModelUI>(_OnAddModelSheetCmd),
                    OnRemoveModelCmd = new RelayCommand<ScheduleSheetInExcelModelUI>(_OnRemoveModelSheetCmd)
                };
                foreach (var item in data.ProjectInfomationModels)
                {
                    var item1 = new ProjectInfomationModelUI
                    {
                        ProjectNameInExcel = item.ProjectNameInExcel,
                        ProjectNameInRevits = item.ProjectNameInRevits
                        .Select(x => new ProjectRevitInfomationModelUI() { Name = x.Name, Path = x.Path, IsSelected = x.IsSelected })
                        .ToList(),
                    };
                    schduleSheet.ProjectInfomationModels.Add(item1);
                }
                result.Add(schduleSheet);
            }
            return result;
        }
        private ScheduleSheetInExcelModelUI CreateNewSheet(string name)
        {
            var schduleSheet = new ScheduleSheetInExcelModelUI
            {
                SheetName = name,
                ScheduleNameInRevit = "ScheduleName",
                ProjectInfomationModels = GetProjectInfomationModelsDefault(name),
                OnAddModelCmd = new RelayCommand<ScheduleSheetInExcelModelUI>(_OnAddModelSheetCmd),
                OnRemoveModelCmd = new RelayCommand<ScheduleSheetInExcelModelUI>(_OnRemoveModelSheetCmd)
            };
            return schduleSheet;
        }
        private ObservableCollection<ProjectInfomationModelUI> GetProjectInfomationModelsDefault(string sheetName)
        {
            var result = new ObservableCollection<ProjectInfomationModelUI>();
            var modelNameInExcels = new List<string>();
            if (sheetName == ScheduleSheetNameDefault.SHEET_WATER_AND_HOT_WATER)
                modelNameInExcels = ScheduleSheetModelNameDefault.SHEET_MODEL_NAME_WATER_AND_HOT_WATER;
            if (sheetName == ScheduleSheetNameDefault.SHEET_DRAIN)
                modelNameInExcels = ScheduleSheetModelNameDefault.SHEET_MODEL_NAME_DRAIN;
            if (sheetName == ScheduleSheetNameDefault.SHEET_HEALTH)
                modelNameInExcels = ScheduleSheetModelNameDefault.SHEET_MODEL_NAME_SHEET_HEALTH;
            if (sheetName == ScheduleSheetNameDefault.SHEET_VENTILATION_LS)
                modelNameInExcels = ScheduleSheetModelNameDefault.SHEET_MODEL_NAME_SHEET_VENTILATION_LS;
            if (sheetName == ScheduleSheetNameDefault.SHEET_VENTILATION_LH)
                modelNameInExcels = ScheduleSheetModelNameDefault.SHEET_MODEL_NAME_SHEET_VENTILATION_LH;
            foreach (var modelNameInExcel in modelNameInExcels)
            {
                var projectInfomationModelUI = new ProjectInfomationModelUI
                {
                    ProjectNameInExcel = modelNameInExcel,
                    ProjectNameInRevits = GetProjectRevitInfomationModelUI(modelNameInExcel)
                };
                result.Add(projectInfomationModelUI);
            }
            return result;
        }
        private List<ProjectRevitInfomationModel> GetProjectRevitInfomationModelUIDefault(string dirModels)
        {
            var result = new List<ProjectRevitInfomationModel>();
            if (!Directory.Exists(dirModels)) return result;
            var files = Directory.GetFiles(dirModels).ToList();
            if (!files.Any()) return result;
            files = files.Where(x => x.Contains(".rvt")).ToList();
            if (!files.Any()) return result;
            result = files
                .Select(x => new ProjectRevitInfomationModel() { Name = Path.GetFileName(x).Split('.').FirstOrDefault(), Path = x, IsSelected = false })
                .ToList();
            return result;
        }
        private List<ProjectRevitInfomationModelUI> GetProjectRevitInfomationModelUI(string nameModelInSheet)
        {
            var result = new List<ProjectRevitInfomationModelUI>();
            if (string.IsNullOrEmpty(nameModelInSheet)) return result;
            if (!_projectRevitInfomationModels.Any()) return result;
            var scheduleModelContains = GetScheduleSheetModelNameContain();
            if (!scheduleModelContains.Any()) return result;
            var scheduleModelContainsTarget = scheduleModelContains
            .Where(x => x.ProjectNameInExcel == nameModelInSheet)
            .ToList();
            if (!scheduleModelContainsTarget.Any()) return result;
            var nameContains = scheduleModelContainsTarget
               .Select(x => x.ProjectNameContain)
               .ToList();
            foreach (var model in _projectRevitInfomationModels)
            {
                foreach (var nameContain in nameContains)
                {
                    var contents = nameContain.Split(',').Select(x=>x.Trim()).ToList();
                    var isContain = true;
                    foreach (var content in contents)
                    {
                        if (!model.Name.ToUpper().Contains(content.ToUpper()))
                        {
                            isContain = false;
                            break;
                        }
                    }
                    if (isContain)
                        result.Add(new ProjectRevitInfomationModelUI() { Name = model.Name, Path = model.Path, IsSelected = model.IsSelected });
                }
            }
            return result;
        }
        private ScheduleSettingModelUI GetScheduleSettingDefault()
        {
            var result = new ScheduleSettingModelUI
            {
                PathOutput = "",
                PathModels = "",
                ScheduleSheets = GetScheduleSheetsDefault()
            };
            return result;
        }
        private ObservableCollection<ScheduleSheetInExcelModelUI> GetScheduleSheetsDefault()
        {
            var result = new ObservableCollection<ScheduleSheetInExcelModelUI>();
            result.Add(CreateNewSheet(ScheduleSheetNameDefault.SHEET_WATER_AND_HOT_WATER));
            result.Add(CreateNewSheet(ScheduleSheetNameDefault.SHEET_DRAIN));
            result.Add(CreateNewSheet(ScheduleSheetNameDefault.SHEET_HEALTH));
            result.Add(CreateNewSheet(ScheduleSheetNameDefault.SHEET_VENTILATION_LS));
            result.Add(CreateNewSheet(ScheduleSheetNameDefault.SHEET_VENTILATION_LH));
            return result;
        }
        private List<ScheduleSheetModelNameContain> GetScheduleSheetModelNameContain()
        {
            var pathTemplate = $"{PathUtils.FolderTemplate}\\daito_schedule_model_contain.json";
            var result = new List<ScheduleSheetModelNameContain>();
            if (!File.Exists(pathTemplate)) return result;
            var data = JsonConvert.DeserializeObject<List<ScheduleSheetModelNameContain>>(File.ReadAllText(pathTemplate));
            if (data == null) return result;
            if (!data.Any()) return result;
            result = data;
            return result;
        }
    }
}
