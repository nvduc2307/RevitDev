using Autodesk.Revit.UI;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using RevitDevelop.Tools.Schedules.model;
using RevitDevelop.Utils;
using System.IO;

namespace RevitDevelop.Tools.Schedules.action
{
    public class WriteScheduleWaterAndHotWateSupplyAction
    {
        private List<MappingRecord> _mappingRecords;
        private List<ExcelScheduleField> _excelScheduleWaterAndHotWateSupplies;
        private List<RevitScheduleFieldValue> _revitScheduleFieldValue;
        public WriteScheduleWaterAndHotWateSupplyAction(
            List<MappingRecord> mappingRecords,
            List<ExcelScheduleField> excelScheduleWaterAndHotWateSupplies)
        {
            _mappingRecords = mappingRecords;
            _excelScheduleWaterAndHotWateSupplies = excelScheduleWaterAndHotWateSupplies;
        }
        public void Execute(string pathExcelOut,string sheetName, List<ScheduleDocument> documents, List<string> scheduleNames)
        {
            try
            {
                if (!File.Exists(pathExcelOut)) throw new Exception($"File:{pathExcelOut} is not existed");
                if (_mappingRecords == null) throw new Exception($"MappingRecords is null");
                if (!_mappingRecords.Any()) throw new Exception($"MappingRecords is empty");
                if (!_excelScheduleWaterAndHotWateSupplies.Any()) throw new Exception($"ScheduleWaterAndHotWateSupply is empty");
                if(!documents.Any()) throw new Exception($"documents is empty");
                if (!scheduleNames.Any()) throw new Exception($"scheduleNames is empty");
                _revitScheduleFieldValue = GetRevitScheduleFieldValues(documents, scheduleNames);
                if (!_revitScheduleFieldValue.Any()) throw new Exception("RevitScheduleFieldValue is empty");
                using (var wb = new XLWorkbook(pathExcelOut))
                {
                    var ws = wb.Worksheets.FirstOrDefault(s =>
                        string.Equals(s.Name, sheetName, StringComparison.OrdinalIgnoreCase));
                    if (ws == null) throw new Exception("sheetName is null");
                    foreach (var excelScheduleWaterAndHotWateSupply in _excelScheduleWaterAndHotWateSupplies)
                    {
                        var mappings = _mappingRecords
                            .Where(x => excelScheduleWaterAndHotWateSupply.FamilyName.Contains(x.WorkItem)
                            && excelScheduleWaterAndHotWateSupply.TypeName == x.MappingTypeName)
                            .ToList();
                        if (!mappings.Any()) continue;
                        ActionInvokeDeviceLength(ws, excelScheduleWaterAndHotWateSupply, mappings);
                        ActionInvokeTeeQuantity(ws, excelScheduleWaterAndHotWateSupply, mappings);
                        ActionInvokeElbowQuantity(ws, excelScheduleWaterAndHotWateSupply, mappings);
                        ActionInvokeAdapterQuantity(ws, excelScheduleWaterAndHotWateSupply, mappings);
                    }
                    wb.Save();
                }
            }
            catch (Exception ex)
            {
                IO.ShowWarning(ex.Message);
            }
        }
        private List<RevitScheduleFieldValue> GetRevitScheduleFieldValues(
            List<ScheduleDocument> documents, List<string> scheduleNames)
        {
            var result = new List<RevitScheduleFieldValue>();
            return result;
        }
        private void ActionInvokeDeviceLength(IXLWorksheet ws, ExcelScheduleField excelScheduleField, List<MappingRecord> mappings)
        {
        }
        private void ActionInvokeTeeQuantity(IXLWorksheet ws, ExcelScheduleField excelScheduleField, List<MappingRecord> mappings)
        {
        }
        private void ActionInvokeElbowQuantity(IXLWorksheet ws, ExcelScheduleField excelScheduleField, List<MappingRecord> mappings)
        {
        }
        private void ActionInvokeAdapterQuantity(IXLWorksheet ws, ExcelScheduleField excelScheduleField, List<MappingRecord> mappings)
        {
        }
    }
}
