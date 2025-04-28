using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI.Selection;
using RevitDevelop.Utils.RevCategories;

namespace RevitDevelop.Utils.SelectFilters
{
    public class GenericSelectionFilter : ISelectionFilter
    {
        private List<BuiltInCategory> _categories = new List<BuiltInCategory>();
        public GenericSelectionFilter(BuiltInCategory category)
        {
            _categories.Add(category);
        }
        public GenericSelectionFilter(List<BuiltInCategory> categories)
        {
            _categories = categories;
        }
        public bool AllowElement(Element elem)
        {
            return elem.Category == null ? false : _categories.Any(x => x == elem.Category.ToBuiltinCategory());
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
    public class GenericSelectionFilterFromCategory : ISelectionFilter
    {
        private BuiltInCategory _category;
        public GenericSelectionFilterFromCategory(BuiltInCategory category)
        {
            _category = category;
        }
        public bool AllowElement(Element elem)
        {
            if (elem.Category == null) return false;
            if (elem is AssemblyInstance ass)
            {
                var els = ass.GetMemberIds().Select(x => elem.Document.GetElement(x));
                return !els.Any(x => x.Category.ToBuiltinCategory() != _category);
            }
            else return elem.Category.ToBuiltinCategory() == _category;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }

    public class GenericSelectionFilterAssemblyFromCategory : ISelectionFilter
    {
        private BuiltInCategory _category;
        public GenericSelectionFilterAssemblyFromCategory(BuiltInCategory category)
        {
            _category = category;
        }
        public bool AllowElement(Element elem)
        {
            if (elem.Category == null) return false;
            if (elem is not AssemblyInstance) return false;
            var ass = elem as AssemblyInstance;
            var els = ass.GetMemberIds().Select(x => elem.Document.GetElement(x));
            return els.Any(x => x.Category.ToBuiltinCategory() == _category);
        }
        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
    public class GenericSelectionFilterRebarAssemblyFromHostCategory : ISelectionFilter
    {
        private RebarHostCategory _rebarHostCate;
        public GenericSelectionFilterRebarAssemblyFromHostCategory(RebarHostCategory rebarHostCate)
        {
            _rebarHostCate = rebarHostCate;
        }
        public bool AllowElement(Element elem)
        {
            if (elem.Category == null) return false;
            if (elem is not AssemblyInstance) return false;
            var ass = elem as AssemblyInstance;
            var els = ass
                .GetMemberIds()
                .Select(x => elem.Document.GetElement(x))
                .Where(x => x is Rebar)
                .Cast<Rebar>();
            return !els.Any(x => (RebarHostCategory)x.get_Parameter(BuiltInParameter.REBAR_HOST_CATEGORY).AsInteger() != _rebarHostCate);
        }
        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
