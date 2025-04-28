namespace RevitDevelop.Utils.RevCategories
{
    public static class RevCategoryUtils
    {
        public static BuiltInCategory ToBuiltinCategory(this Category cat)
        {
            BuiltInCategory result = BuiltInCategory.INVALID;
            if (cat == null)
            {
                return result;
            }

            try
            {
                result = (BuiltInCategory)Enum.Parse(typeof(BuiltInCategory), cat.Id.ToString());
                return result;
            }
            catch
            {
                return result;
            }
        }

    }
}
