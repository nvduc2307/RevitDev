using RevitDevelop.Utils.Assemblies;
using RevitDevelop.Utils.BoundingBoxs;
using RevitDevelop.Utils.RevCategories;
using RevitDevelop.Utils.RevCurves;
using RevitDevelop.Utils.RevitElements;

namespace RevitDevelop.Utils.RevitElements
{
    public class RevElement
    {
        public ElementId Id { get; set; }
        public Element Element { get; set; }
        public BoxElement BoxElement { get; set; }
        public RevElementType RevElementType { get; set; }
        public RevAssemblyType RevAssemblyType { get; set; }
        public List<BoxElement> ElementSubs { get; set; }
        public RevElement(Element element)
        {
            Id = element.Id;
            Element = element;
            BoxElement = new BoxElement(element);
            RevElementType = GetRevElementType();
            ElementSubs = GetElementSubs();
            RevAssemblyType = GetRevAssemblyType();
        }
        private RevElementType GetRevElementType()
        {
            if (Element is AssemblyInstance) return RevElementType.Assembly;
            var result = RevElementType.Assembly;
            var cate = Element.Category.ToBuiltinCategory();
            switch (cate)
            {
                case BuiltInCategory.OST_StructuralFraming:
                    result = RevElementType.Beam;
                    break;
                case BuiltInCategory.OST_StructuralColumns:
                    result = RevElementType.Column;
                    break;
                case BuiltInCategory.OST_StructuralFoundation:
                    result = RevElementType.Foundation;
                    break;
                case BuiltInCategory.OST_Walls:
                    result = RevElementType.Wall;
                    break;
                case BuiltInCategory.OST_Floors:
                    result = RevElementType.Floor;
                    break;
            }
            return result;
        }
        private RevAssemblyType GetRevAssemblyType()
        {
            if (RevElementType != RevElementType.Assembly) return RevAssemblyType.InValid;
            var cateQuantity = ElementSubs.GroupBy(x => x.Element.Category.ToBuiltinCategory()).Count();
            if (cateQuantity > 1) return RevAssemblyType.InValid;
            var cate = ElementSubs.FirstOrDefault().Element.Category.ToBuiltinCategory();
            var result = RevAssemblyType.InValid;
            switch (cate)
            {
                case BuiltInCategory.OST_StructuralFraming:
                    result = RevAssemblyType.Beam;
                    break;
                case BuiltInCategory.OST_StructuralColumns:
                    result = RevAssemblyType.Column;
                    break;
                case BuiltInCategory.OST_StructuralFoundation:
                    result = RevAssemblyType.Foundation;
                    break;
                case BuiltInCategory.OST_Walls:
                    result = RevAssemblyType.Wall;
                    break;
                case BuiltInCategory.OST_Floors:
                    result = RevAssemblyType.Floor;
                    break;
            }
            return result;
        }
        private List<BoxElement> GetElementSubs()
        {
            var elements = new List<BoxElement>();
            try
            {
                if (Element is AssemblyInstance ass)
                    elements = ass
                        .GetMemberIds()
                        .Select(x => new BoxElement(Element.Document.GetElement(x)))
                        .OrderBy(x => x.LineBox.Mid().DotProduct(BoxElement.VTX))
                        .ToList();
                else elements.Add(new BoxElement(Element));
            }
            catch (Exception)
            {
                elements = new List<BoxElement>();
            }
            return elements;
        }
    }
}
