using ClosedXML.Excel;
using Newtonsoft.Json;
using RevitDevelop.Tools.Schedules.model;
using System.IO;

namespace RevitDevelop.Tools.Schedules.action
{
    public class DataAction
    {
        public static void ModifyData(string dir)
        {
            _solve($"{dir}\\data_schedule_1.json");
            _solve($"{dir}\\data_schedule_2.json");
            _solve($"{dir}\\data_schedule_3.json");
            _solve($"{dir}\\data_schedule_4.json");
            _solve($"{dir}\\data_schedule_5.json");
            void _solve(string path)
            {
                if (!File.Exists(path)) return;
                var datas = JsonConvert.DeserializeObject<List<ExcelScheduleField>>(File.ReadAllText(path));
                if (datas == null) return;
                if (!datas.Any()) return;
                var modelValueFrs = datas.First().ScheduleFieldValues;
                foreach (var data in datas)
                {
                    var modelValues = data.ScheduleFieldValues;
                    foreach (var modelValue in modelValues)
                    {
                        var index = modelValues.IndexOf(modelValue);
                        modelValue.IndexColQuantity = modelValueFrs[index].IndexColQuantity;
                        modelValue.IndexColTotal = modelValueFrs[index].IndexColTotal;
                    }
                }
                File.WriteAllText(path, JsonConvert.SerializeObject(datas));
            }
        }
        public static void ExecuteData(string dir, string path)
        {
            var levelSheet1 = new List<string>()
            {
                "F1S",
                "F1S",
                "F1S"
            };
            var levelSheet2 = new List<string>() { "F1S", "F1S", "F1S", "F1S" };
            var levelSheet3 = new List<string>() { "B1S", "F1S" };
            var levelSheet4 = new List<string>() {
                "F1S",
                "F1S",
                "F1S",
                "F1S" };
            var levelSheet5 = new List<string>() {
                "B1S",
                "B1S",
                "B1S",
                "B1S",
                "F1S",
                "F1S",
                "F1S",
                "F1S" };

            var modelSheet1 = new List<string>()
            {
                "給水北",
                "給水南バル",
                "給湯"
            };
            var modelSheet2 = new List<string>() { "合流北", "合流南", "分流北", "分流南" };
            var modelSheet3 = new List<string>() { "Model1", "Model2" };
            var modelSheet4 = new List<string>() {
                "1階端部住戸　LSパック",
                "1階中住戸　LSパック",
                "2階端部住戸　LSパック",
                "2階中住戸　LSパック" };
            var modelSheet5 = new List<string>() {
                "1階端部住戸　LHパック",
                "1階中住戸　LHパック",
                "2階端部住戸　LHパック",
                "2階中住戸　LHパック",
                "1階端部住戸　LHパック",
                "1階中住戸　LHパック",
                "2階端部住戸　LHパック",
                "2階中住戸　LHパック" };
            var sheet1 = GetDataSheduleFieldSheet(path, 11, 29, modelSheet1, levelSheet1, "給水・給湯");
            var sheet2 = GetDataSheduleFieldSheet(path, 14, 24, modelSheet2, levelSheet2, "排水");
            var sheet3 = GetDataSheduleFieldSheet(path, 14, 20, modelSheet3, levelSheet3, "衛生");
            var sheet4 = GetDataSheduleFieldSheet(path, 11, 42, modelSheet4, levelSheet4, "換気・LS");
            var sheet5 = GetDataSheduleFieldSheet(path, 11, 42, modelSheet5, levelSheet5, "換気・LH");

            File.WriteAllText(
                $"{dir}\\data_schedule_1.json",
                JsonConvert.SerializeObject(sheet1));
            File.WriteAllText(
                $"{dir}\\data_schedule_2.json",
                JsonConvert.SerializeObject(sheet2));
            File.WriteAllText(
                $"{dir}\\data_schedule_3.json",
                JsonConvert.SerializeObject(sheet3));
            File.WriteAllText(
                $"{dir}\\data_schedule_4.json",
                JsonConvert.SerializeObject(sheet4));
            File.WriteAllText(
                $"{dir}\\data_schedule_5.json",
                JsonConvert.SerializeObject(sheet5));
        }
        private static List<ExcelScheduleField> GetDataSheduleFieldSheet(
            string path,
            int minR, int maxR, List<string> models, List<string> levels, string sheetName)
        {
            var results = new List<ExcelScheduleField>();
            if (!File.Exists(path)) throw new Exception("OutputFilePath is not existed");
            using (var wb = new XLWorkbook(path))
            {
                var ws = wb.Worksheets.FirstOrDefault(s =>
                    string.Equals(s.Name, sheetName, StringComparison.OrdinalIgnoreCase));
                if (ws == null) return results;
                var index = 1;
                for (int i = minR; i < maxR; i++)
                {
                    var field = new ExcelScheduleField
                    {
                        Id = ws.Cell(i, "A").Value.ToString(),
                        SheetName = sheetName,
                        FamilyName = ws.Cell(i, "E").Value.ToString(),
                        TypeName = ws.Cell(i, "F").Value.ToString(),
                        IndexRow = i,
                        UnitScheduleType = 0,
                        ScheduleFieldValues = new List<ExcelScheduleFieldValue>()
                    };
                    for (int j = 0; j < models.Count; j++)
                    {
                        var scheduleFieldValue = new ExcelScheduleFieldValue
                        {
                            LevelName = levels[j],
                            ProjectName = models[j],
                            IndexRow = i,
                            IndexColQuantity = "",
                            IndexColTotal = ""
                        };
                        field.ScheduleFieldValues.Add(scheduleFieldValue);
                    }
                    results.Add(field);
                    index++;
                }
            }
            return results;
        }
    }
}
