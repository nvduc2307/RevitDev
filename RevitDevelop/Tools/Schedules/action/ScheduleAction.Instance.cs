using Newtonsoft.Json;
using RevitDevelop.Tools.Schedules.model;
using System.Collections.ObjectModel;
using System.IO;

namespace RevitDevelop.Tools.Schedules.action
{
    public partial class ScheduleAction
    {
        private ObservableCollection<ScheduleSheetInExcelModelUI> GetScheduleSheets()
        {
            var path = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\daito_schedule_sheets.json";
            if (!File.Exists(path)) return GetScheduleSheetsDefault();
            var datas = JsonConvert.DeserializeObject<List<ScheduleSheetInExcelModel>>(File.ReadAllText(path));
            if (datas == null) return GetScheduleSheetsDefault();
            if (!datas.Any()) return GetScheduleSheetsDefault();
            var result = new ObservableCollection<ScheduleSheetInExcelModelUI>();
            foreach (var data in datas)
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
                    var itemTarget = _projectInfomationModels
                        .FirstOrDefault(x=>x.ProjectNameInExcel == item.ProjectNameInExcel 
                        && x.ProjectNameInRevit.Name == item.ProjectNameInRevit.Name
                        && x.ProjectNameInRevit.Path == item.ProjectNameInRevit.Path);
                    if (itemTarget == null) continue;
                    var item1 = new ProjectInfomationModelUI
                    {
                        ProjectNameInExcels = [.. itemTarget.ProjectNameInExcels ?? new List<string>()],
                        ProjectNameInRevits = [.. itemTarget.ProjectNameInRevits ?? new List<ProjectRevitInfomationModel>()],
                        ProjectNameInExcelAction = _ProjectNameInExcelAction
                    };
                    item1.ProjectNameInExcel = item1.ProjectNameInExcels.FirstOrDefault(x => x == itemTarget.ProjectNameInExcel);
                    item1.ProjectNameInRevit = item1.ProjectNameInRevits.FirstOrDefault(x => x.Name == itemTarget.ProjectNameInRevit.Name);
                    schduleSheet.ProjectInfomationModels.Add(item1);
                }
                result.Add(schduleSheet);
            }
            return result;
        }
        private ObservableCollection<ScheduleSheetInExcelModelUI> GetScheduleSheetsDefault()
        {
            var result = new ObservableCollection<ScheduleSheetInExcelModelUI>();
            result.Add(CreateNewSheet("給水・給湯"));
            result.Add(CreateNewSheet("排水"));
            result.Add(CreateNewSheet("衛生"));
            result.Add(CreateNewSheet("換気・LS"));
            result.Add(CreateNewSheet("換気・LH"));
            return result;
        }
        private ScheduleSheetInExcelModelUI CreateNewSheet(string name)
        {
            var schduleSheet = new ScheduleSheetInExcelModelUI
            {
                SheetName = name,
                ScheduleNameInRevit = "ScheduleName",
                ProjectInfomationModels = new ObservableCollection<ProjectInfomationModelUI>(),
                OnAddModelCmd = new RelayCommand<ScheduleSheetInExcelModelUI>(_OnAddModelSheetCmd),
                OnRemoveModelCmd = new RelayCommand<ScheduleSheetInExcelModelUI>(_OnRemoveModelSheetCmd)
            };
            foreach (var item in _projectInfomationModels)
            {
                var item1 = new ProjectInfomationModelUI
                {
                    ProjectNameInExcels = [.. item.ProjectNameInExcels ?? new List<string>()],
                    ProjectNameInRevits = [.. item.ProjectNameInRevits ?? new List<ProjectRevitInfomationModel>()],
                    ProjectNameInExcelAction = _ProjectNameInExcelAction
                };
                item1.ProjectNameInExcel = item1.ProjectNameInExcels.FirstOrDefault(x => x == item.ProjectNameInExcel);
                item1.ProjectNameInRevit = item1.ProjectNameInRevits.FirstOrDefault(x => x.Name == item.ProjectNameInRevit.Name);
                schduleSheet.ProjectInfomationModels.Add(item1);
            }
            return schduleSheet;
        }
        private List<ProjectInfomationModelUI> GetProjectInfomationModels()
        {
            var path = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\daito_schedule_project_infomation.json";
            var result = new List<ProjectInfomationModelUI>();
            if (!File.Exists(path)) return result;
            var data = JsonConvert.DeserializeObject<SettingProjectInfomationModel>(File.ReadAllText(path));
            if (data == null) return result;
            foreach (var item in data.ProjectInfomations)
            {
                var projectInfomationModelUI = new ProjectInfomationModelUI
                {
                    ProjectNameInRevits = [.. item.ProjectNameInRevits],
                    ProjectNameInExcels = [.. item.ProjectNameInExcels ?? new List<string>()]
                };
                projectInfomationModelUI.ProjectNameInRevit = projectInfomationModelUI.ProjectNameInRevits
                    .FirstOrDefault(x => x.Name == item.ProjectNameInRevit.Name);
                projectInfomationModelUI.ProjectNameInExcel = projectInfomationModelUI.ProjectNameInExcels
                    .FirstOrDefault(x => x == item.ProjectNameInExcel);
                projectInfomationModelUI.ProjectNameInExcelAction = _ProjectNameInExcelAction;
                result.Add(projectInfomationModelUI);
            }
            return result;
        }
    }
}
