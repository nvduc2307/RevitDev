using Newtonsoft.Json;
using RevitDevelop.Tools.Schedules.model;
using RevitDevelop.Utils;
using System.Collections.ObjectModel;
using System.IO;

namespace RevitDevelop.Tools.Schedules.action
{
    public partial class ScheduleAction
    {
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
            result.ScheduleSheets = GetScheduleSheets(data.ScheduleSheets);
            if(data.ScheduleSheets == null)
                return GetScheduleSettingDefault();
            return result;
        }
        private ObservableCollection<ScheduleSheetInExcelModelUI> GetScheduleSheets(List<ScheduleSheetInExcelModel> dataSheets)
        {
            var result = new ObservableCollection<ScheduleSheetInExcelModelUI>();
            if(dataSheets == null) return result;
            if(!dataSheets.Any()) return result;
            foreach (var data in dataSheets)
            {
                var schduleSheet = new ScheduleSheetInExcelModelUI
                {
                    SheetName = data.SheetName,
                    ScheduleNameInRevit = data.ScheduleNameInRevit,
                    ProjectInfomationModels = new ObservableCollection<ProjectInfomationModelUI>(),
                    OnAddModelCmd = new RelayCommand<ScheduleSheetInExcelModelUI>(_OnAddModelSheetCmd),
                    OnRemoveModelCmd = new RelayCommand<ScheduleSheetInExcelModelUI>(_OnRemoveModelSheetCmd)
                };
                //_projectInfomationModels
                foreach (var item in data.ProjectInfomationModels)
                {
                    var item1 = new ProjectInfomationModelUI
                    {
                        ProjectNameInExcel = item.ProjectNameInExcel,
                        ProjectNameInRevits = item.ProjectNameInRevits
                        .Select(x=>new ProjectRevitInfomationModelUI() { Name = x.Name, Path=x.Path, IsSelected = x.IsSelected})
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
            var modelNames = new List<string>();
            if(sheetName == ScheduleSheetNameDefault.SHEET_WATER_AND_HOT_WATER)
                modelNames = ScheduleSheetModelNameDefault.SHEET_MODEL_NAME_WATER_AND_HOT_WATER;
            if (sheetName == ScheduleSheetNameDefault.SHEET_DRAIN)
                modelNames = ScheduleSheetModelNameDefault.SHEET_MODEL_NAME_DRAIN;
            if (sheetName == ScheduleSheetNameDefault.SHEET_HEALTH)
                modelNames = ScheduleSheetModelNameDefault.SHEET_MODEL_NAME_SHEET_HEALTH;
            if (sheetName == ScheduleSheetNameDefault.SHEET_VENTILATION_LS)
                modelNames = ScheduleSheetModelNameDefault.SHEET_MODEL_NAME_SHEET_VENTILATION_LS;
            if (sheetName == ScheduleSheetNameDefault.SHEET_VENTILATION_LH)
                modelNames = ScheduleSheetModelNameDefault.SHEET_MODEL_NAME_SHEET_VENTILATION_LH;
            foreach (var modelName in modelNames)
            {
                var projectInfomationModelUI = new ProjectInfomationModelUI();
                projectInfomationModelUI.ProjectNameInExcel = modelName;
                projectInfomationModelUI.ProjectNameInRevits = new List<ProjectRevitInfomationModelUI>() 
                { 
                    new ProjectRevitInfomationModelUI() { Name = "model1", Path = "abc", IsSelected= true } ,
                    new ProjectRevitInfomationModelUI() { Name = "model2", Path = "abc", IsSelected= true } ,
                    new ProjectRevitInfomationModelUI() { Name = "model3", Path = "abc", IsSelected= true } ,
                };
                result.Add(projectInfomationModelUI);
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
    }
}
