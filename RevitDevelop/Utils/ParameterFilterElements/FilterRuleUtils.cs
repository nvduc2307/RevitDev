using Autodesk.Revit.DB;

namespace RevitDevelop.Utils.ParameterFilterElements
{
    public static class FilterRuleUtils
    {
        public static FilterRule CreateFilterRule(BuiltInParameter builtInParameter, FilterRuleEnum filterRuleEnum, int value)
        {
            FilterRule result = null;
            try
            {
                ElementId builtInParameterId = new ElementId(builtInParameter);
                switch (filterRuleEnum)
                {
                    case FilterRuleEnum.CreateBeginsWithRule:
                        result = ParameterFilterRuleFactory.CreateEqualsRule(builtInParameterId, value);
                        break;
                    case FilterRuleEnum.CreateContainsRule:
                        result = ParameterFilterRuleFactory.CreateEqualsRule(builtInParameterId, value);
                        break;
                    case FilterRuleEnum.CreateEndsWithRule:
                        result = ParameterFilterRuleFactory.CreateEqualsRule(builtInParameterId, value);
                        break;
                    case FilterRuleEnum.CreateEqualsRule:
                        result = ParameterFilterRuleFactory.CreateEqualsRule(builtInParameterId, value);
                        break;
                    case FilterRuleEnum.CreateGreaterOrEqualRule:
                        result = ParameterFilterRuleFactory.CreateEqualsRule(builtInParameterId, value);
                        break;
                    case FilterRuleEnum.CreateGreaterRule:
                        result = ParameterFilterRuleFactory.CreateEqualsRule(builtInParameterId, value);
                        break;
                    case FilterRuleEnum.CreateIsAssociatedWithGlobalParameterRule:
                        result = ParameterFilterRuleFactory.CreateEqualsRule(builtInParameterId, value);
                        break;
                    case FilterRuleEnum.CreateIsNotAssociatedWithGlobalParameterRule:
                        result = ParameterFilterRuleFactory.CreateEqualsRule(builtInParameterId, value);
                        break;
                    case FilterRuleEnum.CreateLessOrEqualRule:
                        result = ParameterFilterRuleFactory.CreateEqualsRule(builtInParameterId, value);
                        break;
                    case FilterRuleEnum.CreateLessRule:
                        result = ParameterFilterRuleFactory.CreateEqualsRule(builtInParameterId, value);
                        break;
                    case FilterRuleEnum.CreateNotBeginsWithRule:
                        result = ParameterFilterRuleFactory.CreateEqualsRule(builtInParameterId, value);
                        break;
                    case FilterRuleEnum.CreateNotEqualsRule:
                        result = ParameterFilterRuleFactory.CreateNotEqualsRule(builtInParameterId, value);
                        break;
                    case FilterRuleEnum.CreateSharedParameterApplicableRule:
                        result = ParameterFilterRuleFactory.CreateEqualsRule(builtInParameterId, value);
                        break;
                }
            }
            catch (Exception)
            {
            }
            return result;
        }
    }
    public enum FilterRuleEnum
    {
        CreateBeginsWithRule,
        CreateContainsRule,
        CreateEndsWithRule,
        CreateEqualsRule,
        CreateGreaterOrEqualRule,
        CreateGreaterRule,
        CreateIsAssociatedWithGlobalParameterRule,
        CreateIsNotAssociatedWithGlobalParameterRule,
        CreateLessOrEqualRule,
        CreateLessRule,
        CreateNotBeginsWithRule,
        CreateNotEqualsRule,
        CreateSharedParameterApplicableRule
    }
}
