using RevitDevelop.Utils.BoundingBoxs;
using RevitDevelop.Utils.Compares;
using RevitDevelop.Utils.Geometries;
using RevitDevelop.Utils.NumberUtils;
using RevitDevelop.Utils.RevCurves;
using RevitDevelop.Utils.RevFaces;

namespace RevitDevelop.Utils.Solids
{
    public static class SolidUtils
    {
        public static DirectShape CreateDirectShape(this Solid solid, Document document, BuiltInCategory builtInCategory = BuiltInCategory.OST_GenericModel)
        {
            DirectShape result = null;
            try
            {
                result = DirectShape.CreateElement(document, new ElementId(builtInCategory));
                result.SetShape([solid]);
            }
            catch (Exception)
            {
            }
            return result;
        }
        public static Solid CreateSolid(this Line l, double heightMm, double widthMm)
        {
            Solid result = null;
            try
            {
                var vtx = l.Direction();
                var vty = vtx.IsParallel(XYZ.BasisZ)
                    ? vtx.CrossProduct(XYZ.BasisX)
                    : vtx.CrossProduct(XYZ.BasisZ);
                var vtz = vtx.CrossProduct(vty);

                var p1 = l.GetEndPoint(0) - vtz * widthMm.MmToFoot() / 2 - vty * heightMm.MmToFoot() / 2;
                var p2 = p1 + vty * heightMm.MmToFoot();
                var p3 = p2 + vtz * widthMm.MmToFoot();
                var p4 = p3 - vty * heightMm.MmToFoot();

                var ps = new List<XYZ>() { p1, p2, p3, p4 };
                result = ps.CreateSolid(vtx, l.Length.FootToMm());
            }
            catch (Exception)
            {
            }
            return result;
        }
        public static Solid CreateSolid(this XYZ pCenter, XYZ dir, double heightMm, double widthMm)
        {
            Solid result = null;
            try
            {
                var vtx = dir;
                var vty = vtx.IsParallel(XYZ.BasisZ)
                    ? vtx.CrossProduct(XYZ.BasisX)
                    : vtx.CrossProduct(XYZ.BasisZ);
                var vtz = vtx.CrossProduct(vty);

                var p1 = pCenter - vtx * widthMm.MmToFoot() / 2 - vty * heightMm.MmToFoot() / 2 - vtz * heightMm.MmToFoot() / 2;
                var p2 = p1 + vty * heightMm.MmToFoot();
                var p3 = p2 + vtx * widthMm.MmToFoot();
                var p4 = p3 - vty * heightMm.MmToFoot();

                var ps = new List<XYZ>() { p1, p2, p3, p4 };
                result = ps.CreateSolid(vtz, heightMm);
            }
            catch (Exception)
            {
            }
            return result;
        }
        public static Solid CreateSolid(this List<XYZ> polygons, XYZ normal, double thicknessMm)
        {
            Solid result = null;
            var polygonsCount = polygons.Count;
            if (polygonsCount > 2)
            {
                //create list curveloop
                var curveLoop = new CurveLoop();
                for (int i = 0; i < polygonsCount; i++)
                {
                    var j = i == 0 ? polygonsCount - 1 : i - 1;
                    curveLoop.Append(Line.CreateBound(polygons[j], polygons[i]));
                }
                //create solid
                result = GeometryCreationUtilities.CreateExtrusionGeometry(new List<CurveLoop>() { curveLoop }, normal, thicknessMm.MmToFoot());
            }
            return result;
        }
        public static Solid CreateSolidVertical(this List<XYZ> polygons, double heightMm)
        {
            Solid result = null;
            var polygonsCount = polygons.Count;
            if (polygonsCount > 2)
            {
                //create list curveloop
                var curveLoop = new CurveLoop();
                for (int i = 0; i < polygonsCount; i++)
                {
                    if (i != polygonsCount - 1)
                    {
                        var p1 = new XYZ(polygons[i].X, polygons[i].Y, polygons[0].Z);
                        var p2 = new XYZ(polygons[i + 1].X, polygons[i + 1].Y, polygons[0].Z);
                        curveLoop.Append(Line.CreateBound(p1, p2));
                    }
                    else
                    {
                        var p1 = new XYZ(polygons[i].X, polygons[i].Y, polygons[0].Z);
                        var p2 = new XYZ(polygons[0].X, polygons[0].Y, polygons[0].Z);
                        curveLoop.Append(Line.CreateBound(p1, p2));
                    }
                }
                //create solid
                result = GeometryCreationUtilities.CreateExtrusionGeometry(new List<CurveLoop>() { curveLoop }, XYZ.BasisZ, heightMm.MmToFoot());
            }
            return result;
        }
        public static List<XYZ> GetPoints(this Solid solid)
        {
            var result = new List<XYZ>();
            try
            {
                result = solid
                .GetFacesFromSolid()
                .Select(x => x.GetPoints())
                .Aggregate((a, b) => a.Concat(b)
                .Distinct(new ComparePoint())
                .ToList());
            }
            catch (Exception)
            {

            }
            return result;
        }
        public static BoundingBoxXYZ GetBoundingBoxXYZ(this Solid solid)
        {
            var result = new BoundingBoxXYZ();
            try
            {
                var ps = solid.GetPoints();
                var minx = ps.Min(x => x.X);
                var miny = ps.Min(x => x.Y);
                var minz = ps.Min(x => x.Z);
                var maxx = ps.Max(x => x.X);
                var maxy = ps.Max(x => x.Y);
                var maxz = ps.Max(x => x.Z);
                result.Min = new XYZ(minx, miny, minz);
                result.Max = new XYZ(maxx, maxy, maxz);
            }
            catch (Exception)
            {
                result = null;
            }
            return result;
        }
        public static Solid OffsetSolid(this Solid solid, double offsetMm)
        {
            var result = solid;
            try
            {
                var boundingBoxXyz = solid.GetBoundingBoxXYZ();
                if (boundingBoxXyz == null) throw new Exception();
                var outline = new Outline(new XYZ(boundingBoxXyz.Min.X - offsetMm.MmToFoot(), boundingBoxXyz.Min.Y - offsetMm.MmToFoot(), boundingBoxXyz.Min.Z - offsetMm.MmToFoot()),
                    new XYZ(boundingBoxXyz.Max.X + offsetMm.MmToFoot(), boundingBoxXyz.Max.Y + offsetMm.MmToFoot(), boundingBoxXyz.Max.Z + offsetMm.MmToFoot()));

                var slbox = new BoundingBoxXYZ();
                slbox.Min = outline.MinimumPoint;
                slbox.Max = outline.MaximumPoint;
                result = slbox.SolidFromBoundingbox();
            }
            catch (Exception)
            {
                result = solid;
            }
            return result;
        }
        public static Solid SolidFromBoundingbox(this BoundingBoxXYZ bb)
        {
            XYZ min = bb.Min;
            XYZ max = bb.Max;
            XYZ xYZ = min;
            XYZ xYZ2 = new XYZ(min.X, max.Y, min.Z);
            XYZ xYZ3 = new XYZ(max.X, max.Y, min.Z);
            XYZ xYZ4 = new XYZ(max.X, min.Y, min.Z);
            Line curve = Line.CreateBound(xYZ, xYZ2);
            Line curve2 = Line.CreateBound(xYZ2, xYZ3);
            Line curve3 = Line.CreateBound(xYZ3, xYZ4);
            Line curve4 = Line.CreateBound(xYZ4, xYZ);
            CurveLoop curveLoop = new CurveLoop();
            curveLoop.Append(curve);
            curveLoop.Append(curve2);
            curveLoop.Append(curve3);
            curveLoop.Append(curve4);
            return GeometryCreationUtilities.CreateExtrusionGeometry(new List<CurveLoop> { curveLoop }, XYZ.BasisZ, bb.GetHeight());
        }

        public static List<Solid> GetSolids(this Element element)
        {
            var result = new List<Solid>();
            try
            {
                var document = element.Document;
                if (element is AssemblyInstance ass)
                {
                    var solids = ass
                        .GetMemberIds()
                        .Select(x =>
                        {
                            var sls = new List<Solid>();
                            try
                            {
                                sls = document.GetElement(x)._getSolidOrigins();
                            }
                            catch (Exception)
                            {
                            }
                            return sls;
                        })
                        .Aggregate((a, b) => a.Concat(b).ToList());
                    if (solids.Any()) result.AddRange(solids);
                }
                else
                {
                    var solids = element._getSolidOrigins();
                    if (solids.Any()) result.AddRange(solids);
                }
            }
            catch (Exception)
            {
            }
            return result;
        }
        private static List<Solid> _getSolidOrigins(this Element element)
        {
            List<Solid> list = new List<Solid>();
            foreach (GeometryObject item in element.get_Geometry(new Options
            {
                IncludeNonVisibleObjects = true,
                ComputeReferences = true
            }))
            {
                Solid solid = item as Solid;
                GeometryInstance geometryInstance = item as GeometryInstance;
                if (solid != null && solid.Volume > 1E-06)
                {
                    list.Add(solid);
                }

                if (!(geometryInstance != null))
                {
                    continue;
                }

                foreach (GeometryObject item2 in geometryInstance.GetInstanceGeometry())
                {
                    solid = item2 as Solid;
                    if (solid != null && solid.Volume > 1E-06)
                    {
                        list.Add(solid);
                    }
                }
            }

            return list;
        }
        public static List<Face> GetFacesFromSolid(this Solid solid)
        {
            List<Face> list = new List<Face>();
            if (solid == null)
            {
                return list;
            }

            list.AddRange(solid.Faces.Cast<Face>());
            return list;
        }

    }
}
