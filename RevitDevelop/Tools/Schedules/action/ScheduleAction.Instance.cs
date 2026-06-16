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
                    var item1 = new ProjectInfomationModelUI
                    {
                        ProjectNameInExcel = item.ProjectNameInExcel,
                        ProjectNameInRevits = item.ProjectNameInRevits
                        .Select(x=>new ProjectRevitInfomationModelUI() { Name = x.Name, Path=x.Path, IsSelected = x.IsSelected})
                        .ToList(),
                        ProjectNameInExcelAction = _ProjectNameInExcelAction
                    };
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
            return schduleSheet;
        }
    }
}
