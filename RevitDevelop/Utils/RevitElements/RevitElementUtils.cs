using RevitDevelop.Utils.NumberUtils;
using RevitDevelop.Utils.RevCategories;

namespace RevitDevelop.Utils.RevitElements
{
    public static class RevitElementUtils
    {
        public static XYZ GetPosition(this Element ele)
        {
            try
            {
                XYZ location = null;
                LocationPoint locationPoint = ele.Location as LocationPoint;

                if (locationPoint != null)
                    location = locationPoint.Point;
                else
                {
                    // Thử lấy từ bounding box
                    BoundingBoxXYZ boundingBox = ele.get_BoundingBox(null);
                    if (boundingBox != null)
                    {
                        location = new XYZ(
                            (boundingBox.Min.X + boundingBox.Max.X) / 2,
                            (boundingBox.Min.Y + boundingBox.Max.Y) / 2,
                            0
                        );
                    }
                }
                return location;
            }
            catch (Exception)
            {
            }
            return null;
        }
        public static Element CreateHost(this Document document, BuiltInCategory builtInCategory)
        {
            return DirectShape.CreateElement(document, new ElementId(builtInCategory));
        }
        public static void DeleteElement(this Document document, Element element)
        {

            using var ts = new Transaction(document, "Delete Element");
            ts.Start();
            //--------
            document.Delete(element.Id);
            //--------
            ts.Commit();
        }
        public static void DeleteElements(this Document document, List<Element> elements)
        {

            using var ts = new Transaction(document, "Delete Element");
            ts.Start();
            //--------
            document.Delete(elements.Select(x => x.Id).ToList());
            //--------
            ts.Commit();
        }
        public static List<Element> GetElementIntersectBoundingBox(Element ele, double extentFilterMm = 100)
        {
            try
            {
                var document = ele.Document;
                var offset = 100;
                var boundingBoxXyz = ele.get_BoundingBox(null);
                if (boundingBoxXyz == null) return new List<Element>();
                var outline = new Outline(new XYZ(boundingBoxXyz.Min.X, boundingBoxXyz.Min.Y, boundingBoxXyz.Min.Z - offset),
                    new XYZ(boundingBoxXyz.Max.X, boundingBoxXyz.Max.Y, boundingBoxXyz.Max.Z + offset));
                var bbFilter = new BoundingBoxIntersectsFilter(outline, extentFilterMm.MmToFoot());
                var list = new List<Element>();
                var eleInCurrentDocument = new FilteredElementCollector(document, document.ActiveView.Id)
                    .WherePasses(bbFilter)
                    .WhereElementIsNotElementType()
                    .Where(element =>
                    {
                        var builtInCategory = element.Category.ToBuiltinCategory();
                        return builtInCategory == BuiltInCategory.OST_Rebar;
                    })
                    .ToList();
                list.AddRange(eleInCurrentDocument);
            }
            catch (Exception)
            {
            }
            return new List<Element>();
        }
    }
}
