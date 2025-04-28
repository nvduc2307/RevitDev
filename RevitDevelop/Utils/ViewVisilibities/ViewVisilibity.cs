namespace RevitDevelop.Utils.ViewVisilibities
{
    public static class ViewVisilibity
    {
        public static void SetVisilibity(this View view, BuiltInCategory builtInCategory, bool visible)
        {
            var doc = view.Document;
            var category = Category.GetCategory(doc, builtInCategory);
            try
            {
                if (category != null && category.get_AllowsVisibilityControl(view)) view.SetCategoryHidden(category.Id, !visible);
            }
            catch (Exception)
            {
            }
        }
        public static void SetVisilibity(this View view, IEnumerable<BuiltInCategory> builtInCategoryies, bool visible)
        {
            var doc = view.Document;
            var categories = builtInCategoryies.Select(x => Category.GetCategory(doc, x));

            foreach (var category in categories)
            {
                try
                {
                    if (category != null && category.get_AllowsVisibilityControl(view))
                    {
                        view.SetCategoryHidden(category.Id, !visible);
                    }
                }
                catch (Exception)
                {
                }
            }
        }
        public static void SetHideElements(this View view, IEnumerable<BuiltInCategory> builtInCategoryiesVisible)
        {
            var doc = view.Document;
            var categoriesVisible = builtInCategoryiesVisible.Select(x => Category.GetCategory(doc, x));
            var allCates = doc.Settings.Categories;
            foreach (Category category in allCates)
            {
                try
                {
                    if (categoriesVisible.Any(x => x.Id.ToString().Equals(category.Id.ToString())))
                    {
                        if (category != null && category.get_AllowsVisibilityControl(view))
                            view.SetCategoryHidden(category.Id, false);
                    }
                    else
                    {
                        if (category != null && category.get_AllowsVisibilityControl(view))
                            view.SetCategoryHidden(category.Id, true);
                    }
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
