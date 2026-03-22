namespace RevitDevelop.Utils
{
    public static class ElementUtils
    {
        public static Element CreateHost(this Document document, BuiltInCategory builtInCategory)
        {
            return DirectShape.CreateElement(document, new ElementId(builtInCategory));
        }
    }
}
