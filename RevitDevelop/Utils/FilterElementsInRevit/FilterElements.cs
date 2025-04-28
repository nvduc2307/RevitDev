using RevitDevelop.Utils.Entities;

namespace RevitDevelop.Utils.FilterElementsInRevit
{
    public static class FilterElements
    {
        public static List<T> GetElementsFromCategory<T>(this Document document, View view, BuiltInCategory builtInCategory, bool isType = true)
        {
            var results = new List<T>();
            results = isType
                ? new FilteredElementCollector(document, view.Id)
                .WhereElementIsElementType()
                .OfCategory(builtInCategory)
                .Cast<T>()
                .Where(x => x != null)
                .ToList()
                : new FilteredElementCollector(document, view.Id)
                .WhereElementIsNotElementType()
                .OfCategory(builtInCategory)
                .Cast<T>()
                .Where(x => x != null)
                .ToList();
            return results;
        }
        public static List<T> GetElementsFromCategory<T>(this Document document, BuiltInCategory builtInCategory, bool isType = true)
        {
            var results = new List<T>();
            results = isType
                ? new FilteredElementCollector(document)
                .WhereElementIsElementType()
                .OfCategory(builtInCategory)
                .Cast<T>()
                .Where(x => x != null)
                .ToList()
                : new FilteredElementCollector(document)
                .WhereElementIsNotElementType()
                .OfCategory(builtInCategory)
                .OfClass(typeof(T))
                .Cast<T>()
                .Where(x => x != null)
                .ToList();
            return results;
        }
        public static List<Element> GetElementsFromCategory<T>(this Document document, BuiltInCategory builtInCategory, SchemaInfo schemaInfo)
        {
            var results = new List<Element>();
            results = new FilteredElementCollector(document)
                .WhereElementIsNotElementType()
                .Where(x =>
                {
                    var content = SchemaInfo.ReadAll(schemaInfo.SchemaBase, schemaInfo.SchemaField, x);
                    return content != null;
                })
                .Where(x => x != null)
                .ToList();
            return results;
        }
        public static List<T> GetElementsFromClass<T>(this Document document, bool isType)
        {
            var results = new List<T>();
            results = isType
                ? new FilteredElementCollector(document)
                .WhereElementIsElementType()
                .OfClass(typeof(T))
                .Cast<T>()
                .Where(x => x != null)
                .ToList()
                : new FilteredElementCollector(document)
                .WhereElementIsNotElementType()
                .OfClass(typeof(T))
                .Cast<T>()
                .Where(x => x != null)
                .ToList();
            return results;
        }
        public static List<T> GetElementsFromClass<T>(this Document document)
        {
            var results = new List<T>();
            if (typeof(T) != typeof(View) && typeof(T) != typeof(View3D))
            {
                results = new FilteredElementCollector(document)
                .OfClass(typeof(T))
                .Cast<T>()
                .Where(x => x != null)
                .ToList();
            }
            return results;
        }
        public static List<T> GetViewsFromClass<T>(this Document document, bool isTemplate = false)
        {
            var results = new List<T>();
            if (typeof(T) == typeof(View) || typeof(T) == typeof(View3D))
            {
                if (isTemplate)
                {
                    var views = new FilteredElementCollector(document)
                    .OfClass(typeof(View))
                    .Cast<View>()
                    .Where(x => x != null)
                    .Where(x => x.IsTemplate)
                    .ToList();
                    results = views as List<T>;
                }
                else
                {
                    var views = new FilteredElementCollector(document)
                    .OfClass(typeof(View))
                    .Cast<View>()
                    .Where(x => x != null)
                    .Where(x => !x.IsTemplate)
                    .ToList();
                    results = views as List<T>;
                }
            }
            return results;
        }
    }
}
