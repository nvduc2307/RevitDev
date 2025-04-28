using RevitDevelop.Utils.FilterElementsInRevit;

namespace RevitDevelop.Utils.ParameterFilterElements
{
    public static class ParameterFilterElementUtils
    {
        public static ParameterFilterElement CreateParameterFilterElement(
            this Document document,
            string filterName,
            IEnumerable<BuiltInCategory> builtInCategories,
            FilterRule filterRule)
        {
            var elementParaFilter = new ElementParameterFilter(filterRule, false);
            var parameterFilterElements = document.GetElementsFromClass<ParameterFilterElement>(false);
            var filterExisted = parameterFilterElements.FirstOrDefault(x => x.Name == filterName);
            if (filterExisted != null)
            {
                filterExisted.ClearRules();
                filterExisted.SetElementFilter(elementParaFilter);
                return filterExisted;
            }
            ;
            var categoryIds = builtInCategories.Select(x => Category.GetCategory(document, x)).Select(x => x.Id).ToList();
            var result = ParameterFilterElement.Create(document, filterName, categoryIds, elementParaFilter);
            return result;
        }

        public static void ApplyFilter(this View viewApplyFilter, ParameterFilterElement parameterFilterElement, bool visibility)
        {
            try
            {
                var document = viewApplyFilter.Document;
                document.ActiveView.AddFilter(parameterFilterElement.Id);
                document.ActiveView.SetFilterVisibility(viewApplyFilter.Id, visibility);
            }
            catch (Exception)
            {
            }
        }
    }
}
