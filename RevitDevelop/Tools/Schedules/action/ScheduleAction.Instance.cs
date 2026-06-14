using Newtonsoft.Json;
using RevitDevelop.Tools.Schedules.model;
using System.Collections.ObjectModel;
using System.IO;

namespace RevitDevelop.Tools.Schedules.action
{
    public partial class ScheduleAction
    {
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
        private ObservableCollection<ScheduleSheetInExcelModelUI> GetScheduleSheets()
        {
            var result = new ObservableCollection<ScheduleSheetInExcelModelUI>();
            result.Add(CreateNewSheet("sheet1"));
            result.Add(CreateNewSheet("sheet2"));
            return result;
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
