using ClosedXML.Excel;
using Newtonsoft.Json;
using RevitDevelop.Tools.Schedules.model;
using System.IO;

namespace RevitDevelop.Tools.Schedules.utils
{
    public class ScheduleMappingUtils
    {
        public const string fileMappingName = "daito_schedule_mapping_record";
        private const string SheetName = "比較表";
        private const string ColFamilyName = "G";  // G - ファビオネスト ファミリ名
        private const string ColTypeName = "H";     // H - ファビオネスト タイプ名
        private const string ColWorkItem = "I";     // I - 工事項目
        private const int HeaderRow = 6;       // Row 6 is the column header row
        public static List<MappingRecord> GetMappingRecords(string pathMapping)
        {
            var result = new List<MappingRecord>();
            if (string.IsNullOrEmpty(pathMapping)) return result;
            if (!File.Exists(pathMapping)) return result;
            using (var wb = new XLWorkbook(pathMapping))
            {
                var ws = wb.Worksheets.FirstOrDefault(s =>
                    string.Equals(s.Name, SheetName, StringComparison.OrdinalIgnoreCase));
                if (ws == null) return result;
                var lastRow = ws.LastRowUsed()?.RowNumber() ?? 0;
                for (int row = HeaderRow + 1; row <= lastRow; row++)
                {
                    var workItem = ws.Cell(row, ColWorkItem).GetString()?.Trim();
                    if (string.IsNullOrWhiteSpace(workItem))
                        continue;
                    var familyName = ws.Cell(row, ColFamilyName).GetString()?.Trim() ?? string.Empty;
                    var typeName = ws.Cell(row, ColTypeName).GetString()?.Trim() ?? string.Empty;

                    if (string.IsNullOrWhiteSpace(familyName) && string.IsNullOrWhiteSpace(typeName))
                        continue;
                    result.Add(new MappingRecord
                    {
                        FamilyName = familyName,
                        TypeName = typeName,
                        WorkItem = workItem,
                        RowIndex = row
                    });
                }
            }
            return result;
        }

        public static List<MappingRecord> GetMappingRecords()
        {
            var path = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\{fileMappingName}.json";
            if (!File.Exists(path)) return new List<MappingRecord>();
            var datas = JsonConvert.DeserializeObject<List<MappingRecord>>(File.ReadAllText(path));
            return datas;
        }
        public static void ExportFileMappingRecord(string pathMapping, List<MappingRecord> datas)
        {
            try
            {
                if (!datas.Any()) return;
                if (!File.Exists(pathMapping)) return;
                using (var wb = new XLWorkbook(pathMapping))
                {
                    var ws = wb.Worksheets.FirstOrDefault(s =>
                        string.Equals(s.Name, SheetName, StringComparison.OrdinalIgnoreCase));
                    if (ws == null) return;
                    foreach (var data in datas)
                    {
                        var row = datas.IndexOf(data) + 1 + HeaderRow;
                        ws.Cell(row, ColFamilyName).SetValue(data.FamilyName);
                        ws.Cell(row, ColTypeName).SetValue(data.TypeName);
                        ws.Cell(row, ColWorkItem).SetValue(data.WorkItem);
                    }
                    wb.Save();
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
