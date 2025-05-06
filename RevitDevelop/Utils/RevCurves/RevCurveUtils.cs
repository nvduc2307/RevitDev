using RevitDevelop.Utils.Geometries;

namespace RevitDevelop.Utils.RevCurves
{
    public static class RevCurveUtils
    {
        public static XYZ Direction(this Curve c)
        {
            return (c.GetEndPoint(0) - c.GetEndPoint(1)).Normalize();
        }
        public static XYZ Direction(this Line l)
        {
            return (l.GetEndPoint(0) - l.GetEndPoint(1)).Normalize();
        }

        public static XYZ Mid(this Curve c)
        {
            return (c.GetEndPoint(0) + c.GetEndPoint(1)) * 0.5;
        }
        public static XYZ Mid(this Line l)
        {
            return (l.GetEndPoint(0) + l.GetEndPoint(1)) * 0.5;
        }

        public static List<Curve> Copy(this List<Curve> curves, XYZ dirCopy)
        {
            var results = new List<Curve>();
            try
            {
                if (dirCopy.IsSame(XYZ.Zero)) return curves;
                foreach (var item in curves)
                {
                    var p1 = item.GetEndPoint(0) + dirCopy;
                    var p2 = item.GetEndPoint(1) + dirCopy;
                    results.Add(Line.CreateBound(p1, p2));
                }
            }
            catch (Exception)
            {
                return null;
            }
            return results;
        }
        public static CurveLoop ToCurveLoop(this List<Curve> curves)
        {
            var result = new CurveLoop();
            try
            {
                foreach (var item in curves)
                {
                    result.Append(item);
                }
            }
            catch (Exception)
            {
                return null;
            }
            return result;
        }
        public static void CreateCurves(this Document document, List<Curve> curves)
        {
            foreach (var l in curves)
            {
                try
                {
                    var nor = l.Direction().IsParallel(XYZ.BasisZ) ? l.Direction().CrossProduct(XYZ.BasisX) : l.Direction().CrossProduct(XYZ.BasisZ);
                    var plane = Plane.CreateByNormalAndOrigin(nor, l.Mid());
                    var sket = SketchPlane.Create(document, plane);

                    document.Create.NewModelCurve(l, sket);
                }
                catch (Exception)
                {
                }
            }
        }
        public static void CreateCurves(this Document document, Curve curve)
        {
            try
            {
                var nor = curve.Direction().IsParallel(XYZ.BasisZ) ? curve.Direction().CrossProduct(XYZ.BasisX) : curve.Direction().CrossProduct(XYZ.BasisZ);
                var plane = Plane.CreateByNormalAndOrigin(nor, curve.Mid());
                var sket = SketchPlane.Create(document, plane);

                document.Create.NewModelCurve(curve, sket);
            }
            catch (Exception)
            {
            }
        }
        public static List<XYZ> GetPoints(this List<Curve> curves)
        {
            var ps = curves.Select(x => x.GetEndPoint(0)).ToList();
            ps.Add(curves.Last().GetEndPoint(1));
            return ps;
        }
    }
}
