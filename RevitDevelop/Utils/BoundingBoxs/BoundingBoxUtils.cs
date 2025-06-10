using RevitDevelop.Utils.NumberUtils;
using RevitDevelop.Utils.RevCategories;
using RevitDevelop.Utils.RevCurves;
using RevitDevelop.Utils.Solids;

namespace RevitDevelop.Utils.BoundingBoxs
{
    public static class BoundingBoxUtils
    {
        public static List<T> GetElementIntersectBoundingBox<T>(this Solid solid, Document document, BuiltInCategory builtInCategory, double extentMm = 1)
        {
            var solidNew = solid.OffsetSolid(extentMm);
            var boundingBoxXyz = solidNew.GetBoundingBoxXYZ();
            if (boundingBoxXyz == null) return new List<T>();
            var outline = new Outline(new XYZ(boundingBoxXyz.Min.X, boundingBoxXyz.Min.Y, boundingBoxXyz.Min.Z),
                new XYZ(boundingBoxXyz.Max.X, boundingBoxXyz.Max.Y, boundingBoxXyz.Max.Z));
            var bbFilter = new BoundingBoxIntersectsFilter(outline, 1.MmToFoot());
            var list = new List<T>();
            var eleInCurrentDocument = new FilteredElementCollector(document, document.ActiveView.Id)
                .WherePasses(bbFilter)
                .WhereElementIsNotElementType()
                .Where(element =>
                {
                    return element.Category.ToBuiltinCategory() == builtInCategory;
                })
                .Cast<T>()
                .ToList();
            if (eleInCurrentDocument != null)
                list.AddRange(eleInCurrentDocument);
            return list;
        }
        public static IEnumerable<Element> GetElementAroundBox(this Element element)
        {
            var doc = element.Document;
            var bb = element.get_BoundingBox(doc.ActiveView);
            var outLine = new Outline(bb.Min, bb.Max);
            var boxFilter = new BoundingBoxIntersectsFilter(outLine, 10.MmToFoot(), false);
            var eles = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .WherePasses(boxFilter)
                .Where(x => x.Id.ToString() != element.Id.ToString())
                .ToList();
            return eles;
        }
        public static IEnumerable<Element> GetElementAroundBox(this BoundingBoxXYZ boundingBoxXYZ, Document document, BuiltInCategory builtInCategory)
        {
            var outLine = new Outline(boundingBoxXYZ.Min, boundingBoxXYZ.Max);
            var boxFilter = new BoundingBoxIntersectsFilter(outLine, 50.MmToFoot(), false);
            return new FilteredElementCollector(document)
                .WhereElementIsNotElementType()
                .WherePasses(boxFilter)
                .Where(x => x.Category.ToBuiltinCategory() == builtInCategory);
        }
        public static List<Element> GetElementAroundBox(this Document document, Outline outline, List<BuiltInCategory> builtInCategories)
        {
            var boxFilter = new BoundingBoxIntersectsFilter(outline, false);
            var results = new List<Element>();
            foreach (var builtInCategorie in builtInCategories)
            {
                try
                {
                    var elements = new FilteredElementCollector(document)
                     .WhereElementIsNotElementType()
                     .WherePasses(boxFilter)
                     .Where(x => x.Category.ToBuiltinCategory() == builtInCategorie)
                     .ToList();
                    results.AddRange(elements);
                }
                catch (Exception)
                {
                }
            }
            return results;
        }
        public static BoundingBoxXYZ GetSectionBox(this Line line, double heightSection, double extendSection)
        {
            var line_point_mid = line.Mid();
            var line_normal = line.Direction.CrossProduct(XYZ.BasisZ);
            var line_new = Line.CreateBound(line_point_mid + line_normal * 1, line_point_mid - line_normal * 1);
            // Using 0.5 and "true" to specify that the 
            // parameter is normalized places the transform
            // origin at the center of the location curve

            Transform curveTransform = line_new.ComputeDerivatives(0.5, true);

            // The transform contains the location curve
            // mid-point and tangent, and we can obtain
            // its normal in the XY plane:

            XYZ origin = curveTransform.Origin;
            XYZ viewdir = curveTransform.BasisX.Normalize();
            XYZ up = XYZ.BasisZ;
            XYZ right = up.CrossProduct(viewdir);

            // Set up view transform, assuming wall's "up" 
            // is vertical. For a non-vertical situation 
            // such as section through a sloped floor, the 
            // surface normal would be needed

            Transform transform = Transform.Identity;
            transform.Origin = origin;
            transform.BasisX = right;
            transform.BasisY = up;
            transform.BasisZ = viewdir;

            BoundingBoxXYZ sectionBox = new BoundingBoxXYZ();
            sectionBox.Transform = transform;

            // Min & Max X values define the section
            // line length on each side of the wall.
            // Max Y is the height of the section box.
            // Max Z (5) is the far clip offset.

            double d = line.Length / 2 + extendSection;
            double minZ = 0;
            double maxZ = heightSection;
            double h = maxZ - minZ;

            sectionBox.Min = new XYZ(-d, -h - 1, -extendSection / 2);
            sectionBox.Max = new XYZ(d, h + 1, extendSection);

            return sectionBox;
        }
        public static double GetHeight(this BoundingBoxXYZ boundingBoxXYZ)
        {
            return Math.Abs(boundingBoxXYZ.Max.Z - boundingBoxXYZ.Min.Z);
        }
    }
}
