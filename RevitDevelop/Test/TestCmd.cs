using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Nice3point.Revit.Toolkit.External;
using RevitDevelop.Test.views;
using RevitDevelop.Utils.Geometries;
using RevitDevelop.Utils.NumberUtils;
using RevitDevelop.Utils.RevFaces;
using RevitDevelop.Utils.RevSketchPlan;
using RevitDevelop.Utils.SkipWarning;
using RevitDevelop.Utils.Virture;
using RevitDevelop.Utils.WindowEvent.EventMouses;

namespace RevitDevelop.Test
{
    [Transaction(TransactionMode.Manual)]
    public class TestCmd : ExternalCommand
    {
        private TestView _view;
        private CurveVisualizationServer _curveShost;
        private XYZ _sP;
        private List<XYZ> _ps;
        private List<DetailCurve> _detailCurves;
        private List<Dimension> _dims;
        public override void Execute()
        {
            using (var tsg = new TransactionGroup(Document, "name transaction group"))
            {
                tsg.Start();
                try
                {
                    _curveShost = null;
                    _ps = new List<XYZ>();
                    _detailCurves = new List<DetailCurve>();
                    _dims = new List<Dimension>();
                    var view = Document.ActiveView;
                    if (!(view is ViewPlan || view is ViewSection))
                        return;
                    var eventMouse1 = new EventMouseHook();
                    eventMouse1.OnMouseMove += EventMouse1_OnMouseMove;
                    eventMouse1.Start(System.Windows.Forms.MouseButtons.None);
                    using (var ts = new Transaction(Document, "new transaction"))
                    {
                        ts.SkipAllWarnings();
                        ts.Start();
                        Document.SetSketchPlan();
                        ts.Commit();
                    }
                    var isDo = true;
                    do
                    {
                        try
                        {
                            _sP = UiDocument.Selection.PickPoint("Pick first point");
                            _ps.Add(_sP);
                            if (_ps.Count == 5)
                                throw new Exception();
                        }
                        catch (Exception)
                        {
                            isDo = false;
                            eventMouse1.OnMouseMove -= EventMouse1_OnMouseMove;
                            _curveShost.UnAllRegister();
                        }
                    } while (isDo);
                    //--------
                    tsg.Assimilate();
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException) { }
                catch (Exception)
                {
                    tsg.RollBack();
                }
            }
        }
        private void EventMouse1_OnMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (_sP == null) return;
            var view = Document.ActiveView;
            var level = view.GenLevel;
            var vtx = view.RightDirection;
            var vty = view.UpDirection;
            var vtz = view.ViewDirection;
            var orgin = view.Origin;
            var f = new FaceCustom(vtz, orgin);
            var p = UiDocument.GetModelCoordinatesAtCursor()
            .RayPointToFace(f.Normal, f);
            var p0 = _sP.RayPointToFace(f.Normal, f);
            if (level != null)
            {
                p0 = new XYZ(p0.X, p0.Y, level.Elevation);
                p = new XYZ(p.X, p.Y, level.Elevation);
            }
            if (p0.DistanceTo(p).FootToMm() < 5)
                return;
            var ls = _ps.PointsToCurves().Select(x => x as Line).ToList();
            var l = Line.CreateBound(p0, p);
            ls.Add(l);
            XYZ dir = null;
            XYZ nor = null;
            if (_curveShost != null)
                _curveShost.UnRegister();
            _curveShost = new CurveVisualizationServer(UiDocument, ls);
            _curveShost.Register();
            if (_ps.Count < 2)
                return;
            dir = (_ps[1] - _ps[0]).Normalize();
            nor = dir.CrossProduct(vtz);
            var vtCheck = (p - _sP).Normalize();
            using (var ts = new Transaction(Document, "new transaction"))
            {
                ts.SkipAllWarnings();
                ts.Start();
                //if (_detailCurves.Any())
                //{
                //    Document.Delete(_detailCurves.Select(x => x.Id).ToList());
                //    _detailCurves = new List<DetailCurve>();
                //}
                //var lss = _ps.PointsToCurves();
                //foreach (var ld in lss)
                //{
                //    var dl = Document.Create.NewDetailCurve(view, ld);
                //    _detailCurves.Add(dl);
                //}
                if (_dims.Any())
                {
                    Document.Delete(_dims.Select(x => x.Id).ToList());
                    _dims = new List<Dimension>();
                }
                var arr = new ReferenceArray();
                foreach (var item in _ps)
                {
                    var ld = Line.CreateBound(item, item + nor * 1.MmToFoot());
                    var dl = Document.Create.NewDetailCurve(view, ld);
                    arr.Append(new Reference(dl));
                }
                var lDim = Line.CreateUnbound(p, dir);
                _dims.Add(Document.Create.NewDimension(view, lDim, arr));
                ts.Commit();
            }

        }
    }
}
