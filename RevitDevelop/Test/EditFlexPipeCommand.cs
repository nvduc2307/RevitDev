using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Plumbing;
using Nice3point.Revit.Toolkit.External;
using RevitDevelop.Utils.Compares;
using RevitDevelop.Utils.Geometries;
using RevitDevelop.Utils.Messages;
using RevitDevelop.Utils.NumberUtils;
using RevitDevelop.Utils.RevFaces;
using RevitDevelop.Utils.SelectFilters;
using RevitDevelop.Utils.SkipWarning;
using RevitDevelop.Utils.Virture;
using RevitDevelop.Utils.WindowEvent.EventMouses;

namespace RevitDevelop.Test
{
    [Transaction(TransactionMode.Manual)]
    public class EditFlexPipeCommand : ExternalCommand
    {
        private EventMouseHook _mev;
        private List<XYZ> _ps;
        private CurveVisualizationServer _curveShost;
        private XYZ _sp;
        private XYZ _ep;
        private List<XYZ> _flexPs;
        private List<XYZ> _controlPoints;
        private int _indexControl;
        private HermiteSpline _hermiteSpline;
        private double _flexDiameter;
        public override void Execute()
        {
            using (var tsg = new TransactionGroup(Document, "name transaction group"))
            {
                tsg.Start();
                try
                {
                    _ps = new List<XYZ>();
                    _flexPs = new List<XYZ>();
                    _controlPoints = new List<XYZ>();
                    _indexControl = -1;
                    _mev = new EventMouseHook();
                    _mev.OnMouseMove += Mev_OnMouseMove;
                    _mev.Start(System.Windows.Forms.MouseButtons.None);
                    var ele = UiDocument.Selection.PickElement(Document, BuiltInCategory.OST_FlexPipeCurves);
                    if (ele is not FlexPipe flexPipe)
                        return;
                    _flexDiameter = flexPipe.Diameter;
                    var location = flexPipe.Location as LocationCurve;
                    _sp = location.Curve.GetEndPoint(0);
                    _ep = location.Curve.GetEndPoint(1);
                    _hermiteSpline = location.Curve as HermiteSpline;
                    _controlPoints = _hermiteSpline.ControlPoints
                        .Select(x=>x.EditZ(Document.ActiveView.GenLevel.Elevation))
                        .ToList();
                    _flexPs = _hermiteSpline.Tessellate().ToList();
                    var isDo = true;
                    do
                    {
                        try
                        {
                            var p = UiDocument.Selection.PickPoint();
                            _ps.Add(p);
                            if (_ps.Count == 2)
                                throw new Exception();
                        }
                        catch (Exception)
                        {
                            isDo = false;
                            _mev.OnMouseMove -= Mev_OnMouseMove;
                            _curveShost?.UnAllRegister();
                            _controlPoints.Insert(_indexControl, _ps.LastOrDefault());
                            _controlPoints = _controlPoints.Select(x=>x.EditZ((Document.ActiveView as ViewPlan).GenLevel.Elevation)).ToList();
                            using (var ts = new Transaction(Document, "new transaction"))
                            {
                                ts.SkipAllWarnings();
                                ts.Start();
                                Document.Create.NewFlexPipe(_controlPoints, flexPipe.FlexPipeType);
                                Document.Delete(ele.Id);
                                ts.Commit();
                            }

                        }
                    } while (isDo);
                    //--------
                    tsg.Assimilate();
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException) { }
                catch (Exception ex)
                {
                    IO.ShowException(ex);
                    tsg.RollBack();
                }
            }
        }
        private void Mev_OnMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (_ps.Count == 0)
                return;
            if (_sp == null)
                return;
            if (_ep == null)
                return;
            if (_indexControl == -1)
            {
                GetindexControl(Document, _hermiteSpline, _ps.First(), out int indexControl);
                _indexControl = indexControl;
                var pControl = _controlPoints[_indexControl];
                var isdistanceMm = _controlPoints.Any(x => x.DistanceTo(_ps.First()).FootToMm() <= 50);
                if (isdistanceMm)
                {
                    _indexControl = _controlPoints
                        .IndexOf(_controlPoints.FirstOrDefault(x => x.DistanceTo(_ps.First()).FootToMm() <= 50));
                    _controlPoints.RemoveAt(_indexControl);
                }
                else
                {
                    var extent = 100.MmToFoot();
                    var pControlPre = _controlPoints[_indexControl - 1];
                    var vt = (pControl - pControlPre).Normalize();
                    var pInsert1 = _ps.First() - vt * extent * 3 - vt * _flexDiameter * 3;
                    var pInsert2 = _ps.First() - vt * extent * 2 - vt * _flexDiameter * 3;
                    var pInsert3 = _ps.First() - vt * extent - vt * _flexDiameter * 3;
                    var pInsert5 = _ps.First() + vt * extent + vt * _flexDiameter * 3;
                    var pInsert6 = _ps.First() + vt * extent * 2 + vt * _flexDiameter * 3;
                    var pInsert7 = _ps.First() + vt * extent * 3 + vt * _flexDiameter * 3;
                    _controlPoints.Insert(_indexControl, FindPointOnSpLine(Document, _hermiteSpline, pInsert7, out int _));
                    _controlPoints.Insert(_indexControl, FindPointOnSpLine(Document, _hermiteSpline, pInsert6, out int _));
                    _controlPoints.Insert(_indexControl, FindPointOnSpLine(Document, _hermiteSpline, pInsert5, out int _));
                    _controlPoints.Insert(_indexControl, FindPointOnSpLine(Document, _hermiteSpline, pInsert3, out int _));
                    _controlPoints.Insert(_indexControl, FindPointOnSpLine(Document, _hermiteSpline, pInsert2, out int _));
                    _controlPoints.Insert(_indexControl, FindPointOnSpLine(Document, _hermiteSpline, pInsert1, out int _));
                    _indexControl = _indexControl + 3;
                }
                return;
            }
            var p = UiDocument.GetModelCoordinatesAtCursor();
            var controlPoints = _controlPoints.ToList();
            controlPoints.Insert(_indexControl, p);
            controlPoints = controlPoints.Select(x => x.EditZ((Document.ActiveView as ViewPlan).GenLevel.Elevation))
                .Distinct(new ComparePoint())
                .ToList();
            var her = new HermiteSplineTangents();
            her.StartTangent = _hermiteSpline.Tangents.FirstOrDefault();
            her.EndTangent = _hermiteSpline.Tangents.LastOrDefault();
            var hs = HermiteSpline.Create(controlPoints, false, her);
            var pss = hs.Tessellate().ToList();
            var ls = new List<Line>();
            if (pss.Any())
                ls = pss.PointsToCurves().Cast<Line>().ToList();
            if (_curveShost != null)
                _curveShost.UnRegister();
            _curveShost = new CurveVisualizationServer(UiDocument, ls);
            _curveShost.UpdateEdgeColor(new Color(255, 0, 0));
            _curveShost.UpdateScale(10.MmToFoot());
            _curveShost.Register();
        }
        public static XYZ FindPointOnSpLine(Document document, HermiteSpline spLine, XYZ pInput, out int index)
        {
            index = 0;
            var view = document.ActiveView as ViewPlan;
            if (view == null) return null;
            var ps = spLine.Tessellate();
            var pNear = ps.OrderBy(x => x.DistanceTo(pInput)).FirstOrDefault();
            var pNearOnArr = ps.FirstOrDefault(x => x.IsSame(pNear));
            if (pNearOnArr == null) return null;
            var pNearIndex = ps.IndexOf(pNearOnArr);
            if (pNearIndex == 0) return null;
            if (pNearIndex == ps.Count - 1)
            {
                index = pNearIndex;
                return null;
            }
            var vt = (ps[pNearIndex + 1] - ps[pNearIndex]).Normalize();
            var nor = vt.CrossProduct(view.ViewDirection);
            var f = new FaceCustom(nor, ps[pNearIndex]);
            var pResult = pInput.RayPointToFace(f.Normal, f);
            index = pNearIndex;
            return pResult;
        }
        public static void GetindexControl(Document document, HermiteSpline spLine, XYZ pOnSpl, out int indexControl)
        {
            indexControl = 0;
            var pControls = spLine.ControlPoints.ToList();
            var _ = FindPointOnSpLine(document, spLine, pOnSpl, out int index1);
            foreach (var item in pControls)
            {
                FindPointOnSpLine(document, spLine, item, out int index);
                if (index > index1)
                {
                    indexControl = pControls.IndexOf(item);
                    return;
                }
            }
            indexControl = indexControl == 0 
                ? 1 
                : indexControl == pControls.Count - 1
                ? pControls.Count - 2
                : indexControl;
        }
    }
}
