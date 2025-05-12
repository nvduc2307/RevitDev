using Autodesk.Revit.DB.Structure;
using RevitDevelop.Utils.Compares;
using RevitDevelop.Utils.Geometries;
using RevitDevelop.Utils.NumberUtils;
using RevitDevelop.Utils.RevArcs;
using RevitDevelop.Utils.RevCurves;
using RevitDevelop.Utils.RevFaces;

namespace RevitDevelop.Utils.RevRebars
{
    public static class RevRebarUtils
    {
        public static bool HasStartHook(this Rebar r)
        {
            try
            {
                try
                {
                    var hookId = r.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
                    return hookId.ToString() != "-1";
                }
                catch (Exception)
                {
                }
                return false;
            }
            catch (Exception)
            {
            }
            return false;
        }
        public static bool HasEndHook(this Rebar r)
        {
            try
            {
                try
                {
                    var hookId = r.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
                    return hookId.ToString() != "-1";
                }
                catch (Exception)
                {
                }
                return false;
            }
            catch (Exception)
            {
            }
            return false;
        }
        public static XYZ GetNormal(this Rebar rebar)
        {
            XYZ result = null;
            try
            {
                result = rebar.GetShapeDrivenAccessor().Normal;
            }
            catch (Exception)
            {
            }
            return result;
        }
        public static List<Curve> GetLinesOrigin(this Rebar rb)
        {
            var results = new List<Curve>();
            try
            {
                var vty = rb.GetNormal();
                var cs = rb
                    .GetCenterlineCurves(false, false, false, MultiplanarOption.IncludeAllMultiplanarCurves, 0)
                    .Where(x => x is Line)
                    .ToList();
                var cc = cs.Count;
                if (cc == 0) return results;
                if (cc == 1) return results.Concat(cs).ToList();
                var p1 = cs[0].GetEndPoint(0);
                var p2Last = cs[cc - 1].GetEndPoint(1);
                for (var i = 0; i < cc; i++)
                {
                    if (i == cc - 1)
                    {
                        var l = Line.CreateBound(p1, p2Last);
                        results.Add(l);
                    }
                    else
                    {
                        var j = i + 1;
                        var vtx = cs[i].Direction();
                        var vty1 = vtx.CrossProduct(cs[j].Direction());
                        var vtz = vtx.CrossProduct(vty1);
                        var f = new FaceCustom(vtz, cs[i].Mid());
                        var p2 = cs[j].Mid().RayPointToFace(cs[j].Direction(), f);
                        var l = Line.CreateBound(p1, p2);
                        results.Add(l);
                        p1 = p2;
                    }
                }
            }
            catch (Exception)
            {
            }
            return results;
        }
        public static List<Curve> GetCurvesOrgin(this Rebar rb)
        {
            var results = new List<Curve>();
            try
            {
                results = rb.GetCenterlineCurves(false, false, false, MultiplanarOption.IncludeAllMultiplanarCurves, 0).ToList();
            }
            catch (Exception)
            {
            }
            return results;
        }
        public static List<Curve> GetCurveOnPlan(this Rebar rebar)
        {
            var result = new List<Curve>();
            try
            {
                var rebVtZ = rebar.GetNormal();
                var rebVtX = rebVtZ.CrossProduct(XYZ.BasisZ);
                var rebVtY = rebVtZ.CrossProduct(rebVtX);
                var ls = rebar.GetCurvesOrgin();
                var angle = rebVtZ.AngleTo(XYZ.BasisZ);
                var center = ls.GetPoints().CenterPoint();
                var vtx = rebVtZ.CrossProduct(XYZ.BasisZ);
                if (Math.Cos((angle * 180 / Math.PI)).IsEqual(0)) throw new Exception();
                var trsPlan = Transform.CreateRotation(rebVtX, angle);
                var centerPlan = trsPlan.OfPoint(center);
                var vtMovePlan = center - centerPlan;
                var rebVtXPlan = trsPlan.OfVector(rebVtX);
                var rebVtYPlan = trsPlan.OfVector(rebVtY);
                var rebVtZPlan = trsPlan.OfVector(rebVtZ);
                foreach (var curve in ls)
                {
                    try
                    {
                        if (curve is Line l)
                        {
                            var p1 = trsPlan.OfPoint(l.GetEndPoint(0)) + vtMovePlan;
                            var p2 = trsPlan.OfPoint(l.GetEndPoint(1)) + vtMovePlan;
                            var ln = p1.CreateLine(p2);
                            result.Add(ln);
                        }
                        if (curve is Arc arc)
                        {
                            var arcCustom = new ArcCustom(arc);
                            var start = trsPlan.OfPoint(arcCustom.Start) + vtMovePlan;
                            var end = trsPlan.OfPoint(arcCustom.End) + vtMovePlan;
                            var mid = trsPlan.OfPoint(arcCustom.Mid) + vtMovePlan;
                            var darcn = Arc.Create(start, end, mid);
                            result.Add(darcn);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                return result;
            }
            catch (Exception)
            {
            }
            return rebar.GetCurvesOrgin();
        }
        public static List<Curve> GetCurvesGenerateOnPlane(this Rebar rebar, RevRebarDirectionType revRebarDirectionType = RevRebarDirectionType.Up)
        {
            var ls = rebar.GetCurveOnPlan();
            try
            {
                var vtCalAngle = XYZ.BasisY;
                //if (ls.Count == 1) return ls;
                var mainCurve = ls.OrderByDescending(x => x.Length).FirstOrDefault();
                var vtx = mainCurve.Direction();
                var vtz = XYZ.BasisZ;
                var vty = vtx.CrossProduct(vtz);
                var center = ls.GetPoints().CenterPoint();
                var vtCheck = (mainCurve.Mid() - center).Normalize();
                vty = vtCheck.DotProduct(vty).IsGreater(0)
                    ? vty
                    : -vty;
                var angle = 0.0;
                switch (revRebarDirectionType)
                {
                    case RevRebarDirectionType.Up:
                        vtCalAngle = XYZ.BasisY;
                        angle = vty.X > 0
                            ? vty.AngleTo(vtCalAngle)
                            : -vty.AngleTo(vtCalAngle);
                        break;
                    case RevRebarDirectionType.Down:
                        vtCalAngle = -XYZ.BasisY;
                        angle = vty.X > 0
                            ? -vty.AngleTo(vtCalAngle)
                            : vty.AngleTo(vtCalAngle);
                        break;
                    case RevRebarDirectionType.Left:
                        vtCalAngle = -XYZ.BasisX;
                        angle = vty.Y > 0
                            ? vty.AngleTo(vtCalAngle)
                            : -vty.AngleTo(vtCalAngle);
                        break;
                    case RevRebarDirectionType.Right:
                        vtCalAngle = XYZ.BasisX;
                        angle = vty.Y > 0
                            ? -vty.AngleTo(vtCalAngle)
                            : vty.AngleTo(vtCalAngle);
                        break;
                }
                var lsNew = new List<Curve>();
                foreach (var curve in ls)
                {
                    try
                    {
                        if (curve is Line l)
                        {
                            var p1 = l.GetEndPoint(0).Rotate(center, angle);
                            var p2 = l.GetEndPoint(1).Rotate(center, angle);
                            var ln = p1.CreateLine(p2);
                            lsNew.Add(ln);
                        }
                        if (curve is Arc arc)
                        {
                            var arcCustom = new ArcCustom(arc);
                            var start = arcCustom.Start.Rotate(center, angle);
                            var end = arcCustom.End.Rotate(center, angle);
                            var mid = arcCustom.Mid.Rotate(center, angle);
                            var darcn = Arc.Create(start, end, mid);
                            lsNew.Add(darcn);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                return lsNew;
            }
            catch (Exception)
            {
            }
            return ls;
        }
        public static Outline GetRebarDetailOutLine(this View view, FamilyInstance rebarDetailInView)
        {
            try
            {
                var option = new Options();
                var geoSymbol = rebarDetailInView.Symbol.get_Geometry(option);
                var ls = new List<Curve>();
                foreach (var ele in geoSymbol)
                {
                    if (ele is Line line)
                        ls.Add(line);
                    if (ele is Line arc)
                        ls.Add(arc);
                }
                var trs = rebarDetailInView.GetTransform();
                var p1 = trs.OfPoint(new XYZ());
                var fc = new FaceCustom(view.ViewDirection, p1);
                var ps = ls
                    .GetPoints()
                    .Select(x => trs.OfPoint(x))
                    .Select(x => x.RayPointToFace(view.ViewDirection, fc))
                    .ToList();
                var psX = ps.OrderBy(x => x.DotProduct(view.RightDirection)).Distinct(new ComparePoint()).ToList();
                var psY = ps.OrderBy(x => x.DotProduct(view.UpDirection)).Distinct(new ComparePoint()).ToList();
                var minx = ps.Min(x => x.X);
                var miny = ps.Min(x => x.Y);
                var minz = ps.Min(x => x.Z);
                var maxx = ps.Max(x => x.X);
                var maxy = ps.Max(x => x.Y);
                var maxz = ps.Max(x => x.Z);
                var pMin = new XYZ(minx, miny, minz);
                var pMax = new XYZ(maxx, maxy, maxz);
                var outLine = new Outline(pMin, pMax);
                return outLine;
            }
            catch (Exception)
            {
            }
            return null;
        }
    }
}
