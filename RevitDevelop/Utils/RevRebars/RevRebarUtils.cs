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
        private static RebarDirectionTrend GetRebarDirectionTrend(XYZ vtRebar)
        {
            var result = RebarDirectionTrend.Up;
            try
            {
                var angleX = vtRebar.AngleTo(XYZ.BasisX) * 180 / Math.PI;
                var angleY = vtRebar.AngleTo(XYZ.BasisY) * 180 / Math.PI;
                var angleZ = vtRebar.AngleTo(XYZ.BasisZ) * 180 / Math.PI;
                result = angleX <= 45 || angleY <= 45 || angleZ <= 45 ? RebarDirectionTrend.Up : RebarDirectionTrend.Down;
            }
            catch (Exception)
            {
            }
            return result;
        }
        public static List<Curve> GetCurveOnPlan(this Rebar rebar, bool mirror = false)
        {
            var result = new List<Curve>();
            try
            {
                var rebVtZ = rebar.GetNormal();
                var rebVtX = rebVtZ.CrossProduct(XYZ.BasisZ);
                var rebVtY = rebVtZ.CrossProduct(rebVtX);
                var ls = rebar.GetCurvesOrgin();
                var angle = !mirror
                    ? rebVtZ.AngleTo(XYZ.BasisZ)
                    : -rebVtZ.AngleTo(XYZ.BasisZ);
                var center = ls.GetPoints().CenterPoint();
                var vtx = rebVtZ.CrossProduct(XYZ.BasisZ);
                if (CompareInstances.IsAlmostEqual(Math.Cos((angle * 180 / Math.PI)), 0)) throw new Exception();
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
        public static List<Curve> GetCurvesGenerateOnPlane(this Rebar rebar, RevRebarDirectionType revRebarDirectionType = RevRebarDirectionType.Up, bool mirror = false)
        {
            var lsOrigin = rebar.GetLinesOrigin();
            var ls = rebar.GetCurveOnPlan(mirror);
            try
            {
                var vtTrend = (lsOrigin.LastOrDefault().GetEndPoint(1) - lsOrigin.FirstOrDefault().GetEndPoint(0)).Normalize();
                var rebarDirectionTrend = GetRebarDirectionTrend(vtTrend);
                var vtCalAngle = XYZ.BasisY;
                var mainCurve = ls.OrderByDescending(x => x.Length).FirstOrDefault();
                var vtx = mainCurve.Direction();
                var vtz = XYZ.BasisZ;
                var vty = vtx.CrossProduct(vtz);
                var center = ls.GetPoints().CenterPoint();
                var vtCheck = (mainCurve.Mid() - center).Normalize();
                vty = vtCheck.DotProduct(vty).IsGreater(0)
                    ? vty
                    : -vty;
                if (CompareInstances.IsAlmostEqual(vtCheck.Distance().FootToMm(), 0))
                    vty = vtx.CrossProduct(vtz);
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
                var vtTrendNew = (lsNew.LastOrDefault().GetEndPoint(1) - lsNew.FirstOrDefault().GetEndPoint(0)).Normalize();
                var dk = vtTrendNew.DotProduct(XYZ.BasisX);
                var isRevert = rebarDirectionTrend == RebarDirectionTrend.Up
                    ? dk > 0 ? true : false
                    : dk > 0 ? false : true;
                if (!isRevert)
                {
                    ls = rebar.GetCurveOnPlan(true);
                    try
                    {
                        lsNew = new List<Curve>();
                        foreach (var curve in ls)
                        {
                            try
                            {
                                if (curve is Line l)
                                {
                                    var p1 = l.GetEndPoint(0).Rotate(center, angle + Math.PI);
                                    var p2 = l.GetEndPoint(1).Rotate(center, angle + Math.PI);
                                    var ln = p1.CreateLine(p2);
                                    lsNew.Add(ln);
                                }
                                if (curve is Arc arc)
                                {
                                    var arcCustom = new ArcCustom(arc);
                                    var start = arcCustom.Start.Rotate(center, angle + Math.PI);
                                    var end = arcCustom.End.Rotate(center, angle + Math.PI);
                                    var mid = arcCustom.Mid.Rotate(center, angle + Math.PI);
                                    var darcn = Arc.Create(start, end, mid);
                                    lsNew.Add(darcn);
                                }
                            }
                            catch (Exception)
                            {
                            }
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
        public static RebarLapPositionType GetRebarLapPositionType(this Rebar rebar, XYZ posCheck)
        {
            var result = RebarLapPositionType.Start;
            try
            {
                var document = rebar.Document;
                var ls = rebar.GetLinesOrigin();
                var sp = ls.FirstOrDefault().GetEndPoint(0);
                var ep = ls.LastOrDefault().GetEndPoint(1);
                var ds = sp.Distance(posCheck).FootToMm();
                var de = ep.Distance(posCheck).FootToMm();
                result = ds > de ? RebarLapPositionType.End : RebarLapPositionType.Start;
            }
            catch (Exception)
            {
            }
            return result;
        }
    }
    public enum RebarDirectionTrend
    {
        Down = 0,
        Up = 1,
    }
    public enum RebarLapPositionType
    {
        Start = 0,
        End = 1,
    }
}
