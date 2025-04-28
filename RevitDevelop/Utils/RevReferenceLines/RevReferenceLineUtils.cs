namespace RevitDevelop.Utils.RevReferenceLines
{
    public static class RevReferenceLineUtils
    {
        public static void CreateLineStyle(this Document doc, string name, Color color)
        {
            Categories categories = doc.Settings.Categories;
            Category lineCat = categories.get_Item(BuiltInCategory.OST_Lines);
            Category newLineStyleCat = categories.NewSubcategory(lineCat, name);
            doc.Regenerate();
            newLineStyleCat.SetLineWeight(1, GraphicsStyleType.Projection);
            newLineStyleCat.LineColor = color;
        }

        public static void CreateLineStyle(this Document doc, string name, Color color, out Category newLineStyle)
        {
            Categories categories = doc.Settings.Categories;
            Category lineCat = categories.get_Item(BuiltInCategory.OST_Lines);
            Category newLineStyleCat = categories.NewSubcategory(lineCat, name);
            doc.Regenerate();
            newLineStyleCat.SetLineWeight(1, GraphicsStyleType.Projection);
            newLineStyleCat.LineColor = color;
            newLineStyle = newLineStyleCat;
        }

        public static IEnumerable<Category> GetAllLineStyle(this Document doc)
        {
            Category c = doc.Settings.Categories.get_Item(BuiltInCategory.OST_Lines);
            CategoryNameMap subcats = c.SubCategories;
            foreach (Category lineStyle in subcats)
            {
                yield return lineStyle;
            }
        }
    }
}
