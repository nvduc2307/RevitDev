using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.VisualStudio.PlatformUI;
using Newtonsoft.Json;
using RevitDevelop.Tools.Schedules.model;
using RevitDevelop.Utils;
using System.IO;

namespace RevitDevelop.Tools.Schedules.action
{
    public class ScheduleExcelAction
    {
        private const int _rowHeaderIndex = 7;
        private string _pathOutput;
        private ScheduleExcelHeader _scheduleExcelHeader;
        public ScheduleExcelAction()
        {
            _scheduleExcelHeader = GetScheduleExcelHeader();
        }
        public List<ScheduleSheetInExcelToFillModel> GetSheets(string pathOutput)
        {
            var result = new List<ScheduleSheetInExcelToFillModel>();
            _pathOutput = pathOutput;
            if (string.IsNullOrEmpty(_pathOutput)) return result;
            if (!File.Exists(_pathOutput)) return result;
            try
            {
                using (var wb = new XLWorkbook(_pathOutput))
                {
                    foreach (var ws in wb.Worksheets)
                    {
                        var data = GetData(ws);
                        if (!data.Any()) continue;
                        var item = new ScheduleSheetInExcelToFillModel
                        {
                            SheetName = ws.Name,
                            ExcelScheduleFields = data
                        };
                        result.Add(item);
                    }
                }
            }
            catch (Exception)
            {
            }
            return result;
        }
        private List<ExcelScheduleField> GetData(IXLWorksheet sheet)
        {
            var result = new List<ExcelScheduleField>();
            if (!HasData(sheet)) return result;
            var colMax = sheet.LastColumnUsed().ColumnNumber();
            var rowMax = sheet.LastRowUsed().RowNumber();
            var row = _rowHeaderIndex + 1;
            var isdo = true;
            do
            {
                try
                {
                    var excelScheduleField = new ExcelScheduleField()
                    {
                        SheetName = sheet.Name
                    };
                    for (var i = 1; i < colMax; i++)
                    {
                        var cell = sheet.Cell(row, i);
                        var colCheck = sheet.Cell(_rowHeaderIndex, i);
                        if (colCheck.Value.ToString() == _scheduleExcelHeader.HeaderFamilyName)
                            if (cell.Value.ToString() == _scheduleExcelHeader.EndData1) throw new Exception();
                        if (colCheck.Value.ToString() == _scheduleExcelHeader.HeaderFamilyName)
                            excelScheduleField.FamilyName = cell.Value.ToString();
                        if (colCheck.Value.ToString() == _scheduleExcelHeader.HeaderTypeName)
                            excelScheduleField.TypeName = cell.Value.ToString();
                        if (colCheck.Value.ToString() == _scheduleExcelHeader.HeaderUnit)
                            excelScheduleField.UnitScheduleType = GetUnit(cell.Value.ToString());
                        excelScheduleField.ScheduleFieldValues = GetExcelScheduleFieldValues(sheet, colMax);
                    }
                    if (!string.IsNullOrEmpty(excelScheduleField.FamilyName))
                        result.Add(excelScheduleField);
                    if(row > rowMax) throw new Exception();
                    row++;
                }
                catch (Exception)
                {
                    isdo = false;
                }

            } while (isdo);
            return result;
        }
        private bool HasData(IXLWorksheet sheet)
        {
            if (_scheduleExcelHeader == null) return false;
            var colMax = sheet.LastColumnUsed().ColumnNumber();
            var cells = new List<IXLCell>();
            for (var i = 1; i < colMax; i++)
            {
                try
                {
                    var cell = sheet.Cell(_rowHeaderIndex, i);
                    cells.Add(cell);
                }
                catch (Exception)
                {
                }
            }
            if (!cells.Any()) return false;
            if (!cells.Any(x => x.Value.ToString() == _scheduleExcelHeader.HeaderFamilyName)) return false;
            if (!cells.Any(x => x.Value.ToString() == _scheduleExcelHeader.HeaderTypeName)) return false;
            if (!cells.Any(x => x.Value.ToString() == _scheduleExcelHeader.HeaderUnit)) return false;
            if (!cells.Any(x => x.Value.ToString() == _scheduleExcelHeader.HeaderQuantityValue)) return false;
            if (!cells.Any(x => x.Value.ToString() == _scheduleExcelHeader.HeaderTotalValue)) return false;
            return true;
        }
        private List<ExcelScheduleFieldValue> GetExcelScheduleFieldValues(IXLWorksheet sheet, int colMax)
        {
            var results = new List<ExcelScheduleFieldValue>();
            for (int i = 1; i < colMax; i++)
            {
                var cell = sheet.Cell(_rowHeaderIndex, i);
                var cellProject = sheet.Cell(_rowHeaderIndex - 2, i);
                var cellLevel = sheet.Cell(_rowHeaderIndex - 2, i);
                var cellNext = sheet.Cell(_rowHeaderIndex, i + 1);
                if (cell.Value.ToString() != _scheduleExcelHeader.HeaderQuantityValue) continue;
                var cellField = new ExcelScheduleFieldValue
                {
                    ProjectName = cellProject.Value.ToString(),
                    LevelName = cellLevel.Value.ToString(),
                    IndexColQuantity = cell.Address.ColumnLetter,
                    QuantityValue = 0,
                    IndexColTotal = cellNext.Address.ColumnLetter
                };
                results.Add(cellField);
            }
            return results;
        }
        private int GetUnit(string unitStringValue)
        {
            return 1;
        }
        private ScheduleExcelHeader GetScheduleExcelHeader()
        {
            ScheduleExcelHeader result = null;
            var pathData = $"{PathUtils.FolderTemplate}\\dato_schedule_in_excel_header.json";
            if (!File.Exists(pathData)) return result;
            result = JsonConvert.DeserializeObject<ScheduleExcelHeader>(File.ReadAllText(pathData));
            return result;
        }
    }
}
