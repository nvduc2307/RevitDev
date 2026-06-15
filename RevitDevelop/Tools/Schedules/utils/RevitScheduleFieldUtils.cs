using RevitDevelop.Tools.Schedules.model;
using RevitDevelop.Utils.Numbers;

namespace RevitDevelop.Tools.Schedules.utils
{
    public class RevitScheduleFieldUtils
    {
        public static List<RevitScheduleFieldValue> GetDataSchedule(Document document, List<string> scheduleNames)
        {
            var result = new List<RevitScheduleFieldValue>();
            var scheduleAlls = new FilteredElementCollector(document)
                .WhereElementIsNotElementType()
                .OfClass(typeof(ViewSchedule))
                .Cast<ViewSchedule>()
                .Where(x => !x.IsTemplate)
                .ToList();
            if (!scheduleAlls.Any()) return result;
            var schedules = scheduleAlls
                .Where(x => scheduleNames.Any(y => y == x.Name))
                .ToList();
            if (!schedules.Any()) return result;
            foreach (var schedule in schedules)
            {
                var data = GetDataSchedule(schedule);
                if (!data.Any()) continue;
                result.AddRange(data);
            }
            return result;
        }
        private static List<RevitScheduleFieldValue> GetDataSchedule(ViewSchedule schedule)
        {
            var result = new List<RevitScheduleFieldValue>();
            if (schedule == null) return result;
            TableData tableData = schedule.GetTableData();
            TableSectionData body = tableData.GetSectionData(SectionType.Body);
            int nRows = body.NumberOfRows;
            int nCols = body.NumberOfColumns;
            var datas = new List<string>();

            for (int r = 0; r < nRows; r++)
            {
                var item = new RevitScheduleFieldValue();
                item.documentName = schedule.Document.Title;
                item.ScheduleName = schedule.Name;
                for (int c = 0; c < nCols; c++)
                {
                    try
                    {
                        string text = schedule.GetCellText(SectionType.Body, r, c);
                        if (string.IsNullOrEmpty(text)) break;
                        var colType = GetScheduleDeviceFieldType(schedule, c);
                        switch (colType)
                        {
                            case RevitScheduleFieldValueType.Unknown:
                                break;
                            case RevitScheduleFieldValueType.Family:
                                item.FamilyName = text;
                                break;
                            case RevitScheduleFieldValueType.Type:
                                item.TypeName = text;
                                break;
                            case RevitScheduleFieldValueType.SystemType:
                                item.SystemName = text;
                                break;
                            case RevitScheduleFieldValueType.Count:
                                item.Quantity = text.GetIntegerFromText();
                                if (item.Quantity == 0) break;
                                break;
                            case RevitScheduleFieldValueType.Length:
                                item.Length = text.GetDoubleFromText();
                                if (item.Length == 0) break;
                                break;
                            case RevitScheduleFieldValueType.Size:
                                item.Size = text;
                                break;
                            case RevitScheduleFieldValueType.KyusuiCheck:
                                item.IsKyusuiChecked = text.ToUpper() == "YES" || text.ToUpper() == "はい" ? true : false;
                                if (!item.IsKyusuiChecked) break;
                                break;
                            case RevitScheduleFieldValueType.KyutouCheck:
                                item.IsDrainageSystemChecked = text.ToUpper() == "YES" ? true : false;
                                break;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                if (string.IsNullOrEmpty(item.FamilyName)) continue;
                if (string.IsNullOrEmpty(item.TypeName)) continue;
                if (string.IsNullOrEmpty(item.SystemName)) continue;
                if (item.Quantity == 0) continue;
                if (item.Length == 0) continue;
                if (!item.IsKyusuiChecked) continue;
                if (string.IsNullOrEmpty(item.Size)) continue;
                result.Add(item);
            }
            return result;
        }
        public static RevitScheduleFieldValueType GetScheduleDeviceFieldType(
            ViewSchedule schedule,
            int bodyColCount)
        {
            var result = RevitScheduleFieldValueType.Unknown;
            var definition = schedule.Definition;
            var fieldCount = definition.GetFieldCount();

            // Known BuiltInParameter IDs (language-independent)
            var familyParamIds = new HashSet<ElementId>
            {
                new ElementId(BuiltInParameter.ELEM_FAMILY_PARAM),
                new ElementId(BuiltInParameter.ALL_MODEL_FAMILY_NAME),
                new ElementId(BuiltInParameter.SYMBOL_FAMILY_NAME_PARAM)
            };
            var typeParamIds = new HashSet<ElementId>
            {
                new ElementId(BuiltInParameter.ELEM_TYPE_PARAM),
                new ElementId(BuiltInParameter.ALL_MODEL_TYPE_NAME),
                new ElementId(BuiltInParameter.SYMBOL_NAME_PARAM)
            };
            var lengthParamIds = new HashSet<ElementId>
            {
                new ElementId(BuiltInParameter.CURVE_ELEM_LENGTH),
                new ElementId(BuiltInParameter.FABRICATION_PART_LENGTH)
            };
            var sizeParamIds = new HashSet<ElementId>
            {
                new ElementId(BuiltInParameter.RBS_CALCULATED_SIZE),
                new ElementId(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM)
            };
            var systemTypeParamIds = new HashSet<ElementId>
            {
                new ElementId(BuiltInParameter.RBS_PIPING_SYSTEM_TYPE_PARAM),
                new ElementId(BuiltInParameter.RBS_DUCT_SYSTEM_TYPE_PARAM),
                new ElementId(BuiltInParameter.RBS_SYSTEM_CLASSIFICATION_PARAM)
            };

            int visibleIndex = 0;
            for (int i = 0; i < fieldCount; i++)
            {
                var field = definition.GetField(i);
                if (field.IsHidden)
                    continue;
                if (visibleIndex == bodyColCount)
                {
                    result = GetScheduleDeviceFieldType(field, familyParamIds, typeParamIds,
                    lengthParamIds, sizeParamIds, systemTypeParamIds);
                }
                visibleIndex++;
                if (result != RevitScheduleFieldValueType.Unknown)
                    break;
            }
            return result;
        }
        private static RevitScheduleFieldValueType GetScheduleDeviceFieldType(
            ScheduleField field,
            HashSet<ElementId> familyIds,
            HashSet<ElementId> typeIds,
            HashSet<ElementId> lengthIds,
            HashSet<ElementId> sizeIds,
            HashSet<ElementId> systemTypeIds)
        {
            // FieldType.Count → Count column (language-independent)
            if (field.FieldType == ScheduleFieldType.Count)
                return RevitScheduleFieldValueType.Count;

            var paramId = field.ParameterId;

            // BuiltInParameter matching (language-independent)
            if (familyIds.Contains(paramId)) return RevitScheduleFieldValueType.Family;
            if (typeIds.Contains(paramId)) return RevitScheduleFieldValueType.Type;
            if (lengthIds.Contains(paramId)) return RevitScheduleFieldValueType.Length;
            if (sizeIds.Contains(paramId)) return RevitScheduleFieldValueType.Size;
            if (systemTypeIds.Contains(paramId)) return RevitScheduleFieldValueType.SystemType;

            // Custom/Shared parameters → match by name
            // (project-defined names are consistent across languages)
            var fieldName = field.GetName()?.Trim() ?? string.Empty;

            // Checkbox: 設備積算_給水設備工事
            if (fieldName.Contains("設備積算") && fieldName.Contains("給水"))
                return RevitScheduleFieldValueType.KyusuiCheck;

            // Checkbox: 設備積算_給湯設備工事
            if (fieldName.Contains("設備積算") && fieldName.Contains("給湯"))
                return RevitScheduleFieldValueType.KyutouCheck;

            // Fitting length: 積算_配管継手長さ (custom param for fitting equivalent length)
            if (fieldName.Contains("積算") && fieldName.Contains("長さ"))
                return RevitScheduleFieldValueType.Length;

            // Diameter: 直径 (custom param sometimes used instead of サイズ)
            if (fieldName == "直径")
                return RevitScheduleFieldValueType.Size;

            return RevitScheduleFieldValueType.Unknown;
        }
    }
}
