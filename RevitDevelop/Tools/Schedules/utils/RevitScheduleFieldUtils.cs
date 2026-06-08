using RevitDevelop.Tools.Schedules.model;

namespace RevitDevelop.Tools.Schedules.utils
{
    public class RevitScheduleFieldUtils
    {
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
        public static RevitScheduleFieldValueType GetScheduleDeviceFieldType(
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
