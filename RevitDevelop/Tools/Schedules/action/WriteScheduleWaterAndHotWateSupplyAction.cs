using ClosedXML.Excel;
using Newtonsoft.Json;
using RevitDevelop.Tools.Schedules.model;
using RevitDevelop.Tools.Schedules.utils;
using RevitDevelop.Utils;
using System.IO;

namespace RevitDevelop.Tools.Schedules.action
{
    public class WriteScheduleWaterAndHotWateSupplyAction
    {
        private List<MappingRecord> _mappingRecords;
        private List<ExcelScheduleField> _excelScheduleWaterAndHotWateSupplies;
        public WriteScheduleWaterAndHotWateSupplyAction(
            List<MappingRecord> mappingRecords)
        {
            _mappingRecords = mappingRecords;
            _excelScheduleWaterAndHotWateSupplies = GetExcelScheduleFields();
        }
        public void Execute(string pathExcelOut, string sheetName, List<ScheduleDocument> documents, List<string> scheduleNames)
        {
            try
            {
                if (string.IsNullOrEmpty(pathExcelOut)) throw new Exception($"File:pathExcelOut is not existed");
                if (!File.Exists(pathExcelOut)) throw new Exception($"File:{pathExcelOut} is not existed");
                if (_mappingRecords == null) throw new Exception($"MappingRecords is null");
                if (!_mappingRecords.Any()) throw new Exception($"MappingRecords is empty");
                if (!_excelScheduleWaterAndHotWateSupplies.Any()) throw new Exception($"ScheduleWaterAndHotWateSupply is empty");
                if (!documents.Any()) throw new Exception($"documents is empty");
                if (!scheduleNames.Any()) throw new Exception($"scheduleNames is empty");
                using (var wb = new XLWorkbook(pathExcelOut))
                {
                    var ws = wb.Worksheets.FirstOrDefault(s =>
                        string.Equals(s.Name, sheetName, StringComparison.OrdinalIgnoreCase));
                    var wSheets = wb.Worksheets;
                    if (ws == null) throw new Exception("sheetName is null");
                    foreach (var scheduleDoc in documents)
                    {
                        var revitScheduleFieldValues = RevitScheduleFieldUtils.GetDataSchedule(scheduleDoc.Document, scheduleNames);
                        if (!revitScheduleFieldValues.Any()) continue;
                        foreach (var excelScheduleWaterAndHotWateSupply in _excelScheduleWaterAndHotWateSupplies)
                        {
                            var mappings = _mappingRecords
                                .Where(x => excelScheduleWaterAndHotWateSupply.FamilyName.Contains(x.WorkItem))
                                .ToList();
                            if (!mappings.Any()) continue;
                            ActionInvokeDeviceLength(ws, excelScheduleWaterAndHotWateSupply, mappings, revitScheduleFieldValues);
                            ActionInvokeTeeQuantity(ws, excelScheduleWaterAndHotWateSupply, mappings, revitScheduleFieldValues);
                            ActionInvokeElbowQuantity(ws, excelScheduleWaterAndHotWateSupply, mappings, revitScheduleFieldValues);
                            ActionInvokeAdapterQuantity(ws, excelScheduleWaterAndHotWateSupply, mappings, revitScheduleFieldValues);
                            ActionInvokePipingHeaderQuantity(ws, excelScheduleWaterAndHotWateSupply, mappings, revitScheduleFieldValues);
                        }
                    }
                    wb.Save();
                }
            }
            catch (Exception ex)
            {
                IO.ShowWarning(ex.Message);
            }
        }
        private void ActionInvokeDeviceLength(
            IXLWorksheet ws,
            ExcelScheduleField excelScheduleField,
            List<MappingRecord> mappings,
            List<RevitScheduleFieldValue> revitScheduleFieldValues)
        {
            var familyNameContain = "DKﾕﾆｯﾄ配管 保温付樹脂管";
            if (!excelScheduleField.FamilyName.Contains(familyNameContain)) return;
            var revitScheduleFieldValuesTarget = revitScheduleFieldValues
                .Where(x => mappings.Any(y => y.FamilyName == x.FamilyName && y.TypeName == x.TypeName))
                .ToList();
            if (!revitScheduleFieldValuesTarget.Any()) return;
            var size = excelScheduleField.FamilyName.GetIntegerFromText();
            double lengthMm = 0;
            foreach (var dataTarget in revitScheduleFieldValuesTarget)
            {
                var sizes = dataTarget.Size
                    .Split('-')
                    .ToList();
                var sizeTarget = sizes
                    .Where(x => x.Contains($"{size}"))
                    .ToList();
                lengthMm += dataTarget.Length * sizeTarget.Count / sizes.Count;
            }
            var lengthM = Math.Round(lengthMm / 1000, 1);
            foreach (var value in excelScheduleField.ScheduleFieldValues)
            {
                var cellIndex = ws.Cell(value.IndexRow, value.IndexColQuantity);
                cellIndex.SetValue(lengthM);
            }
        }
        private void ActionInvokeTeeQuantity(
            IXLWorksheet ws,
            ExcelScheduleField excelScheduleField,
            List<MappingRecord> mappings,
            List<RevitScheduleFieldValue> revitScheduleFieldValues)
        {
            var familyNameContain = "DKﾕﾆｯﾄ配管 ｴﾙﾎﾞ";
            if (!excelScheduleField.FamilyName.Contains(familyNameContain)) return;
            var revitScheduleFieldValuesTarget = revitScheduleFieldValues
                .Where(x => mappings.Any(y => y.FamilyName == x.FamilyName && y.TypeName == x.TypeName))
                .ToList();
            if (!revitScheduleFieldValuesTarget.Any()) return;


        }
        private void ActionInvokeElbowQuantity(
            IXLWorksheet ws,
            ExcelScheduleField excelScheduleField,
            List<MappingRecord> mappings,
            List<RevitScheduleFieldValue> revitScheduleFieldValues)
        {
            var familyNameContain = "DKﾕﾆｯﾄ配管 ﾁｰｽﾞ";
            if (!excelScheduleField.FamilyName.Contains(familyNameContain)) return;
            var revitScheduleFieldValuesTarget = revitScheduleFieldValues
                .Where(x => mappings.Any(y => y.FamilyName == x.FamilyName && y.TypeName == x.TypeName))
                .ToList();
            if (!revitScheduleFieldValuesTarget.Any()) return;
        }
        private void ActionInvokeAdapterQuantity(
            IXLWorksheet ws,
            ExcelScheduleField excelScheduleField,
            List<MappingRecord> mappings,
            List<RevitScheduleFieldValue> revitScheduleFieldValues)
        {
            var familyNameContain1 = "DKﾕﾆｯﾄ配管 ｵｽｱﾀﾞﾌﾟﾀｰ";
            var familyNameContain2 = "DKﾕﾆｯﾄ配管 床上用ｱﾀﾞﾌﾟﾀｰ";
            if (!(excelScheduleField.FamilyName.Contains(familyNameContain1)
                || excelScheduleField.FamilyName.Contains(familyNameContain2))) return;
            var revitScheduleFieldValuesTarget = revitScheduleFieldValues
                .Where(x => mappings.Any(y => y.FamilyName == x.FamilyName && y.TypeName == x.TypeName))
                .ToList();
            if (!revitScheduleFieldValuesTarget.Any()) return;
        }
        private void ActionInvokePipingHeaderQuantity(
            IXLWorksheet ws,
            ExcelScheduleField excelScheduleField,
            List<MappingRecord> mappings,
            List<RevitScheduleFieldValue> revitScheduleFieldValues)
        {
            var familyNameContain = "DKﾕﾆｯﾄ配管 ﾍｯﾀﾞｰ";
            if (!excelScheduleField.FamilyName.Contains(familyNameContain)) return;
            var revitScheduleFieldValuesTarget = revitScheduleFieldValues
                .Where(x => mappings.Any(y => y.FamilyName == x.FamilyName && y.TypeName == x.TypeName))
                .ToList();
            if (!revitScheduleFieldValuesTarget.Any()) return;
        }
        private List<ExcelScheduleField> GetExcelScheduleFields()
        {
            var path = @"D:\proj\me\RevitDev\RevitDevelop\Resources\Datas\data_schedule_1.json";
            var result = new List<ExcelScheduleField>();
            if (!File.Exists(path)) return result;
            var content = File.ReadAllText(path);
            result = JsonConvert.DeserializeObject<List<ExcelScheduleField>>(content) ?? new List<ExcelScheduleField>();
            return result;
        }
    }
}
