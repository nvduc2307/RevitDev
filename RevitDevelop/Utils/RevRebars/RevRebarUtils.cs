using Autodesk.Revit.DB.Structure;
using RevitDevelop.Utils.Geometries;
using RevitDevelop.Utils.RevArcs;
using RevitDevelop.Utils.RevCurves;

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
        public static List<Curve> GetCurvesGenerateOnPlane(this Rebar rebar, XYZ pCoordinate = null)
        {
            try
            {
                var result = new List<Curve>();
                var ls = rebar.GetCurvesOrgin();
                var vtX = ls.OrderByDescending(x => x.Length)
                        .FirstOrDefault().Direction();
                var vtZ = rebar.GetNormal();
                var vtY = vtX.CrossProduct(vtZ);
                var vtXOnPlan = vtX.EditZ(0);
                var alphaX =
                    vtX.IsSameDirection(XYZ.BasisZ)
                    ? Math.PI / 2
                    : vtX.IsSameDirection(-XYZ.BasisZ)
                    ? -Math.PI / 2
                    : vtX.AngleTo(vtXOnPlan);
                var alphaZ = vtZ.AngleTo(XYZ.BasisZ);
                var trsrX = Transform.CreateRotation(vtZ, alphaX);
                var trsrZ = Transform.CreateRotation(trsrX.OfVector(vtX), alphaZ);
                var alphaXOnplan = trsrX.OfVector(vtX).AngleTo(XYZ.BasisX);
                var trsrXOnPlan = Transform.CreateRotation(XYZ.BasisZ, alphaXOnplan);
                var pf = trsrXOnPlan.OfPoint(trsrZ.OfPoint(trsrX.OfPoint(ls.FirstOrDefault().GetEndPoint(0))));
                var vtMove = pCoordinate == null
                    ? new XYZ(0, 0, 0) - pf
                    : pCoordinate - pf;
                foreach (var curve in ls)
                {
                    try
                    {
                        if (curve is Line l)
                        {
                            var p1 = trsrXOnPlan.OfPoint(trsrZ.OfPoint(trsrX.OfPoint(l.GetEndPoint(0)))) + vtMove;
                            var p2 = trsrXOnPlan.OfPoint(trsrZ.OfPoint(trsrX.OfPoint(l.GetEndPoint(1)))) + vtMove;
                            var ln = p1.CreateLine(p2);
                            result.Add(ln);
                        }
                        if (curve is Arc arc)
                        {
                            var arcCustom = new ArcCustom(arc);
                            var start = trsrXOnPlan.OfPoint(trsrZ.OfPoint(trsrX.OfPoint(arcCustom.Start))) + vtMove;
                            var end = trsrXOnPlan.OfPoint(trsrZ.OfPoint(trsrX.OfPoint(arcCustom.End))) + vtMove;
                            var mid = trsrXOnPlan.OfPoint(trsrZ.OfPoint(trsrX.OfPoint(arcCustom.Mid))) + vtMove;
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
            return new List<Curve>();
        }
    }
}
