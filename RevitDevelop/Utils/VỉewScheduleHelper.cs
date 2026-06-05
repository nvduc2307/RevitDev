namespace RevitDevelop.Utils
{
    public class VỉewScheduleHelper
    {
        public static ScheduleField FindField(ViewSchedule schedule, BuiltInParameter paramEnum)
        {
            ScheduleDefinition definition = schedule.Definition;
            ScheduleField foundField = null;
            ElementId paramId = new ElementId(paramEnum);

            foreach (ScheduleFieldId fieldId in definition.GetFieldOrder())
            {
                foundField = definition.GetField(fieldId);
                if (foundField.ParameterId == paramId)
                {
                    return foundField;
                }
            }

            return null;
        }
        public static ScheduleField FindField(ViewSchedule schedule, string paramEnumName)
        {
            ScheduleDefinition definition = schedule.Definition;
            ScheduleField foundField = null;
            foreach (ScheduleFieldId fieldId in definition.GetFieldOrder())
            {
                foundField = definition.GetField(fieldId);
                if (foundField.GetName() == paramEnumName) return foundField;
            }
            return null;
        }
        public static void AddFilter<T>(
            ViewSchedule viewSchedule, 
            BuiltInParameter paramFilter, 
            T valueFilter, 
            ScheduleFilterType scheduleFilterType = ScheduleFilterType.Equal)
        {
            var definition = viewSchedule.Definition;
            var scheduleField = FindField(viewSchedule, paramFilter);
            // Set field to hidden
            scheduleField.IsHidden = false;
            ScheduleFilter filter = valueFilter is double valueDouble
                ? new ScheduleFilter(scheduleField.FieldId, scheduleFilterType, valueDouble)
                : valueFilter is ElementId valueElementId
                    ? new ScheduleFilter(scheduleField.FieldId, scheduleFilterType, valueElementId)
                    : valueFilter is int valueInt
                                    ? new ScheduleFilter(scheduleField.FieldId, scheduleFilterType, valueInt)
                                    : valueFilter is string valueString
                                                    ? new ScheduleFilter(scheduleField.FieldId, scheduleFilterType, valueString)
                                                    : new ScheduleFilter(scheduleField.FieldId, scheduleFilterType);
            definition.AddFilter(filter);
        }
        public static void AddFilter<T>(ViewSchedule viewSchedule, ScheduleField scheduleField, T valueFilter, ScheduleFilterType scheduleFilterType = ScheduleFilterType.Equal)
        {
            var definition = viewSchedule.Definition;
            // Set field to hidden
            scheduleField.IsHidden = false;
            ScheduleFilter filter = valueFilter is double valueDouble
                ? new ScheduleFilter(scheduleField.FieldId, scheduleFilterType, valueDouble)
                : valueFilter is ElementId valueElementId
                    ? new ScheduleFilter(scheduleField.FieldId, scheduleFilterType, valueElementId)
                    : valueFilter is int valueInt
                                    ? new ScheduleFilter(scheduleField.FieldId, scheduleFilterType, valueInt)
                                    : valueFilter is string valueString
                                                    ? new ScheduleFilter(scheduleField.FieldId, scheduleFilterType, valueString)
                                                    : new ScheduleFilter(scheduleField.FieldId, scheduleFilterType);
            definition.AddFilter(filter);
        }
        public static void ChangeValueFilter<T>(
            ViewSchedule viewSchedule,
            BuiltInParameter paramFilter,
            T valueFilter,
            ScheduleFilterType scheduleFilterType = ScheduleFilterType.Equal)
        {
            var definition = viewSchedule.Definition;
            var scheduleFilters = definition.GetFilters().ToList();
            var scheduleField = FindField(viewSchedule, paramFilter);
            if (scheduleField != null)
            {
                var filter = scheduleFilters.Find(scheduleFilter => scheduleFilter.FieldId == scheduleField.FieldId);
                if (filter != null)
                {
                    // Set field to hidden
                    scheduleField.IsHidden = false;
                    if (valueFilter is double valueDouble)
                    {
                        filter.SetValue(valueDouble);
                    }
                    else if (valueFilter is ElementId valueElementId)
                    {
                        filter.SetValue(valueElementId);
                    }
                    else if (valueFilter is int valueInt)
                    {
                        filter.SetValue(valueInt);
                    }
                    else if (valueFilter is string valueString)
                    {
                        filter.SetValue(valueString);
                    }
                }
                else
                {
                    AddFilter<T>(viewSchedule, paramFilter, valueFilter, scheduleFilterType);
                }
            }
        }
        public static void ChangeValueFilter<T>(ViewSchedule viewSchedule, ScheduleField scheduleField, T valueFilter, ScheduleFilterType scheduleFilterType = ScheduleFilterType.Equal)
        {
            var definition = viewSchedule.Definition;
            var scheduleFilters = definition.GetFilters().ToList();
            if (scheduleField != null)
            {
                var filter = scheduleFilters.Find(scheduleFilter => scheduleFilter.FieldId == scheduleField.FieldId);
                if (filter != null)
                {
                    // Set field to hidden
                    scheduleField.IsHidden = false;
                    if (valueFilter is double valueDouble)
                    {
                        filter.SetValue(valueDouble);
                    }
                    else if (valueFilter is ElementId valueElementId)
                    {
                        filter.SetValue(valueElementId);
                    }
                    else if (valueFilter is int valueInt)
                    {
                        filter.SetValue(valueInt);
                    }
                    else if (valueFilter is string valueString)
                    {
                        filter.SetValue(valueString);
                    }
                }
                else
                {
                    AddFilter<T>(viewSchedule, scheduleField, valueFilter, scheduleFilterType);
                }
            }
        }
        public static void AddSortGroupField(ViewSchedule viewSchedule, BuiltInParameter paramFilter, ScheduleSortOrder scheduleSortOrder)
        {
            var document = viewSchedule.Document;
            var definition = viewSchedule.Definition;
            var scheduleField = FindField(viewSchedule, paramFilter);
            using (Transaction t = new Transaction(document, "Add filter"))
            {
                t.Start();

                // If field not present, add it
                if (scheduleField != null)
                {
                    // Set field to hidden
                    ScheduleSortGroupField sortGroup = null;
                    sortGroup = new ScheduleSortGroupField(scheduleField.FieldId, scheduleSortOrder);
                    sortGroup.ShowHeader = true;
                    sortGroup.ShowBlankLine = true;
                    definition.IsItemized = false;
                    definition.AddSortGroupField(sortGroup);
                }
                t.Commit();
            }
        }
    }
}
