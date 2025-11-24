using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Nice3point.Revit.Extensions;
using RevitDevelop.Utils.SelectFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitDevelop.Test
{
    [Transaction(TransactionMode.Manual)]
    public class CheckIntersectPipeCommand : IExternalCommand
    {
        private const double OFFSET_FT = 500 / 304.8;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                var referenceMain = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, new GenericSelectionFilter(BuiltInCategory.OST_PipeCurves), "Pick pipe main");
                var mainPipe = doc.GetElement(referenceMain) as Pipe;

                var referenceBranch = uidoc.Selection.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Element, new GenericSelectionFilter(BuiltInCategory.OST_PipeCurves), "Pick pipe branch");
                var branchPipes = referenceBranch.Select(r => doc.GetElement(r) as Pipe).ToList();

                if (mainPipe.Location is LocationCurve locCur)
                {
                    Line mainCurve = locCur.Curve as Line;
                    XYZ dir = mainCurve.Direction;

                    var intersectionPoints = FindIntersectionPoints(mainCurve, branchPipes);

                    var pointsWithParam = intersectionPoints
                                          .Select(p => new { P = p, T = mainCurve.Project(p)?.Parameter ?? 0.0 })
                                          .OrderBy(x => x.T)
                                          .ToList();

                    using (var t = new Transaction(doc, "Break Pipe Main at Intersections"))
                    {
                        t.Start();

                        for (var i = 0; i < pointsWithParam.Count; i++)
                        {
                            try
                            {
                                var pStart = pointsWithParam[i].P - dir.Multiply(OFFSET_FT);
                                var pEnd = pointsWithParam[i].P + dir.Multiply(OFFSET_FT);

                                // NOTE: BreakCurve trả về Id của đoạn MỚI tạo ra
                                // Gọi với main.Id theo thứ tự từ cuối về đầu thường ổn:
                                // sau mỗi break, đoạn "bên đầu 1" giữ nguyên Id cũ.
                                var newId = PlumbingUtils.BreakCurve(doc, mainPipe.Id, pStart);
                                var middleId = PlumbingUtils.BreakCurve(doc, mainPipe.Id, pEnd);

                                if (middleId != null)
                                    doc.Delete(middleId);

                                var flex = CreateFlexByPoint(doc, pStart, pEnd, pointsWithParam[i].P, mainPipe.Diameter, mainPipe.LevelId);

                                doc.Regenerate();
                            }
                            catch (Exception)
                            {
                            }
                        }

                        t.Commit();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return Result.Succeeded;
        }

        private FlexPipe CreateFlexByPoint(Document doc, XYZ startP, XYZ endP, XYZ intersectP, double diameterFt, ElementId levelId)
        {
            XYZ startTargent = (intersectP - startP).Normalize();
            XYZ endTargent = (endP - intersectP).Normalize();

            ElementId systemTypeId = null;

            FilteredElementCollector pipeTypes = new FilteredElementCollector(doc).OfClass(typeof(PipingSystemType));
            foreach (MEPSystemType MEPSysType in pipeTypes.Cast<MEPSystemType>())
            {
                systemTypeId = MEPSysType.Id;
                break;
            }

            var controlPoint = new List<XYZ> { startP, intersectP + XYZ.BasisZ * 1, endP };

            FlexPipe flex = FlexPipe.Create(doc, systemTypeId, new ElementId(142445), levelId, startTargent, endTargent, controlPoint);

            if (flex != null)
            {
                var pd = flex.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM);

                if (pd != null && !pd.IsReadOnly)
                    pd.Set(diameterFt);
            }

            return flex;
        }

        private List<XYZ> FindIntersectionPoints(Curve mainCurve, IList<Pipe> others)
        {
            var hits = new List<XYZ>();

            foreach (var p in others)
            {
                var lc = p.Location as LocationCurve;
                var c = lc?.Curve;
                if (c == null) continue;

                var res = mainCurve.Intersect(c, out IntersectionResultArray ira);

                if (res == SetComparisonResult.Overlap && ira != null && ira.Size > 0)
                {
                    if (ira != null)
                    {
                        for (int i = 0; i < ira.Size; i++)
                        {
                            var it = ira.get_Item(i);
                            if (it != null && it.XYZPoint != null)
                                hits.Add(it.XYZPoint);
                        }
                    }

                    continue;
                }

                if ((res == SetComparisonResult.Subset || res == SetComparisonResult.Superset || res == SetComparisonResult.Disjoint))
                    continue;

                if (res == SetComparisonResult.Equal)
                    continue;
            }

            return hits;
        }
    }
}