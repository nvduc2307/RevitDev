using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.VisualStudio.Telemetry.Metrics;
using Newtonsoft.Json;
using RevitDevelop.Tools.Schedules.model;
using RevitDevelop.Tools.Schedules.utils;
using RevitDevelop.Utils;
using System.IO;

namespace RevitDevelop.Tools.Schedules.action
{
    public class WriteScheduleWaterAndHotWateSupplyAction
    {
        private List<ExcelScheduleField> _excelScheduleWaterAndHotWateSupplies;
        public WriteScheduleWaterAndHotWateSupplyAction()
        {
            _excelScheduleWaterAndHotWateSupplies = GetExcelScheduleFields();
        }
        public void Execute(
            string pathExcelOut, 
            string sheetName, 
            List<ScheduleDocument> documents, 
            List<string> scheduleNames,
            List<MappingRecord> mappingRecords)
        {
            try
            {
                if (string.IsNullOrEmpty(pathExcelOut)) throw new Exception($"File:pathExcelOut is not existed");
                if (!File.Exists(pathExcelOut)) throw new Exception($"File:{pathExcelOut} is not existed");
                if (mappingRecords == null) throw new Exception($"MappingRecords is null");
                if (!mappingRecords.Any()) throw new Exception($"MappingRecords is empty");
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
                            var mappings = mappingRecords
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
            var diameter = excelScheduleField.FamilyName.GetIntegerFromText();
            if (diameter == 0) return;
            double lengthMm = 0;
            foreach (var dataTarget in revitScheduleFieldValuesTarget)
            {
                var sizes = dataTarget.Size
                    .Split('-')
                    .ToList();
                var sizeTarget = sizes
                    .Where(x => x.Contains($"{diameter}"))
                    .ToList();
                lengthMm += dataTarget.Length * sizeTarget.Count / sizes.Count;
            }
            var lengthM = Math.Round(lengthMm / 1000, 1);
            foreach (var value in excelScheduleField.ScheduleFieldValues)
            {
                var cellIndex = ws.Cell(value.IndexRow, value.IndexColQuantity);
                cellIndex.SetValue(lengthM);
                break;
            }
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
            var diameter = excelScheduleField.FamilyName.GetIntegerFromText();
            if (diameter == 0) return;
            int quantity = 0;
            foreach (var dataTarget in revitScheduleFieldValuesTarget)
            {
                var sizes = dataTarget.Size
                    .Split('-')
                    .ToList();
                var sizeTarget = sizes
                    .Where(x => x.Contains($"{diameter}"))
                    .ToList();
                quantity += dataTarget.Quantity;
            }
            foreach (var value in excelScheduleField.ScheduleFieldValues)
            {
                var cellIndex = ws.Cell(value.IndexRow, value.IndexColQuantity);
                cellIndex.SetValue(quantity);
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
            var diameters = excelScheduleField.FamilyName
                    .Split('x')
                    .Select(x => x.GetIntegerFromText())
                    .ToList();
            if (!diameters.Any(x => x != 0)) return;
            var diameterMapping = diameters.Count == 1
                    ? $"{diameters.First()}"
                    : diameters.Select(x => x.ToString()).Aggregate((a, b) => $"{a}x{b}");
            int quantity = 0;
            foreach (var item in revitScheduleFieldValuesTarget)
            {
                var sizes = item.Size
                        .Split('-')
                        .ToList();
                var diameterCheck = sizes.Count == 1
                    ? $"{sizes.First()}"
                    : sizes.Select(x => x.ToString()).Aggregate((a, b) => $"{a}x{b}");
                if (diameterCheck != diameterMapping) continue;
                quantity += item.Quantity;
            }
            foreach (var value in excelScheduleField.ScheduleFieldValues)
            {
                var cellIndex = ws.Cell(value.IndexRow, value.IndexColQuantity);
                cellIndex.SetValue(quantity);
            }
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
            var diameter = excelScheduleField.FamilyName.GetIntegerFromText();
            if (diameter == 0) return;
            int quantity = 0;
            foreach (var dataTarget in revitScheduleFieldValuesTarget)
            {
                var sizes = dataTarget.Size
                    .Split('-')
                    .ToList();
                var sizeTarget = sizes
                    .Where(x => x.Contains($"{diameter}"))
                    .ToList();
                quantity += dataTarget.Quantity;
            }
            foreach (var value in excelScheduleField.ScheduleFieldValues)
            {
                var cellIndex = ws.Cell(value.IndexRow, value.IndexColQuantity);
                cellIndex.SetValue(quantity);
            }
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
