using Newtonsoft.Json;
using RevitDevelop.Tools.Schedules.model;
using RevitDevelop.Utils;
using System.Collections.ObjectModel;
using System.IO;

namespace RevitDevelop.Tools.Schedules.action
{
    public partial class ScheduleAction
    {
        private void SaveSetting()
        {
            var pathSetting = $"{PathUtils.AppDataDirectory}\\daito_schedule_setting.json";
            var pathSettingTemplate = $"{PathUtils.FolderTemplate}\\daito_schedule_setting.json";
            if (!File.Exists(pathSettingTemplate)) return;
            if (!File.Exists(pathSetting)) File.Copy(pathSettingTemplate, pathSetting);
            var content = new ScheduleSettingModelUI()
            {
                PathOutput = _viewModel.ScheduleSetting.PathOutput,
                PathModels = _viewModel.ScheduleSetting.PathModels,
            };
            File.WriteAllText(pathSetting, Newtonsoft.Json.JsonConvert.SerializeObject(content));
        }
        private void UpdateModelInSheet()
        {
            if (_viewModel == null) return;
            foreach (var sheet in _viewModel.ScheduleSetting.ScheduleSheets)
            {
                foreach (var model in sheet.ProjectInfomationModels)
                {
                    model.ProjectNameInRevits = GetProjectRevitInfomationModelUI(model.ProjectNameInExcel);
                    foreach (var item in model.ProjectNameInRevits)
                    {
                        item.Parent = model;
                        item.IsSelectedAction = _IsSelectedAction;
                    }
                    model.ProjectNameInRevitsTarget = [.. model.ProjectNameInRevits];
                    model.SearchModelContainAction = _SearchModelContainAction;
                }
            }
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
            var modelNameInExcels = new List<string>() { };
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
                    ProjectNameInRevits = GetProjectRevitInfomationModelUI(modelNameInExcel),
                    SearchModelContainAction = _SearchModelContainAction
                };
                projectInfomationModelUI.ProjectNameInRevitsTarget = [.. projectInfomationModelUI.ProjectNameInRevits];
                result.Add(projectInfomationModelUI);
            }
            return result;
        }
        private List<ProjectRevitInfomationModelUI> GetProjectRevitInfomationModelUIDefault(string dirModels)
        {
            var result = new List<ProjectRevitInfomationModelUI>();
            if (string.IsNullOrEmpty(dirModels)) return result;
            if (!Directory.Exists(dirModels)) return result;
            var files = Directory.GetFiles(dirModels).ToList();
            if (!files.Any()) return result;
            files = files.Where(x => x.Contains(".rvt")).ToList();
            if (!files.Any()) return result;
            result = files
                .Select(x => new ProjectRevitInfomationModelUI()
                {
                    Name = Path.GetFileName(x).Split('.').FirstOrDefault(),
                    Path = x,
                    IsSelected = false
                })
                .ToList();
            return result;
        }
        private List<ProjectRevitInfomationModelUI> GetProjectRevitInfomationModelUI(string nameModelInSheet)
        {
            var result = new List<ProjectRevitInfomationModelUI>();
            if (string.IsNullOrEmpty(nameModelInSheet)) return result;
            if (!_projectRevitInfomationModels.Any()) return result;

            foreach (var model in _projectRevitInfomationModels)
            {
                var projectRevitInfomationModelUI = new ProjectRevitInfomationModelUI()
                {
                    Name = model.Name,
                    Path = model.Path,
                    IsSelected = model.IsSelected
                };
                result.Add(projectRevitInfomationModelUI);
            }
            return result;
        }
        private ScheduleSettingModelUI GetScheduleSettingDefault()
        {
            var pathSetting = $"{PathUtils.AppDataDirectory}\\daito_schedule_setting.json";
            var result = new ScheduleSettingModelUI
            {
                PathOutput = "",
                PathModels = "",
                ScheduleSheets = new ObservableCollection<ScheduleSheetInExcelModelUI>()
            };
            if (!File.Exists(pathSetting)) return result;
            var setting = JsonConvert.DeserializeObject<ScheduleSettingModelUI>(File.ReadAllText(pathSetting));
            if (setting == null) return result;
            result.PathOutput = setting.PathOutput;
            result.PathModels = setting.PathModels;
            _projectRevitInfomationModels = GetProjectRevitInfomationModelUIDefault(result.PathModels);
            _scheduleSheetInExcelToFillModel = _scheduleExcelAction.GetSheets(result.PathOutput);
            UpdateSheets();
            UpdateModelInSheet();
            return result;
        }
        private void UpdateSheets()
        {
            if (_scheduleSheetInExcelToFillModel == null) return;
            if (!_scheduleSheetInExcelToFillModel.Any()) return;
            if (_viewModel == null) return;
            foreach (var sheetFill in _scheduleSheetInExcelToFillModel)
            {
                if (_viewModel.ScheduleSetting.ScheduleSheets.Any(x => x.SheetName == sheetFill.SheetName)) continue;
                var item = new ScheduleSheetInExcelModelUI
                {
                    SheetName = sheetFill.SheetName,
                    ScheduleNameInRevit = "フレキシブル配管集計2, 配管集計2, 配管継手集計エルボ樹脂管用2, 配管継手集計エルボ樹脂管以外2",
                    ProjectInfomationModels = new ObservableCollection<ProjectInfomationModelUI>()
                };
                foreach (var scheduleFieldValue in sheetFill.ExcelScheduleFields.FirstOrDefault().ScheduleFieldValues)
                {
                    var projectInfomationModelUI = new ProjectInfomationModelUI();
                    projectInfomationModelUI.ProjectNameInExcel = scheduleFieldValue.ProjectName;
                    item.ProjectInfomationModels.Add(projectInfomationModelUI);
                }
                _viewModel.ScheduleSetting.ScheduleSheets.Add(item);
            }
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
        private List<ScheduleSheetInExcelModelUI> GetScheduleSheets(string pathExcelOutput)
        {
            var result = new List<ScheduleSheetInExcelModelUI>();
            if(!File.Exists(pathExcelOutput)) return result;
            if(!File.Exists(pathExcelOutput)) return result;
            return result;
        }
    }
}
