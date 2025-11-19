using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Plumbing;
using Nice3point.Revit.Toolkit.External;
using RevitDevelop.Utils.Compares;
using RevitDevelop.Utils.FilterElementsInRevit;
using RevitDevelop.Utils.Geometries;
using RevitDevelop.Utils.Messages;
using RevitDevelop.Utils.NumberUtils;
using RevitDevelop.Utils.RevCurves;
using RevitDevelop.Utils.RevFaces;
using RevitDevelop.Utils.RevPipes;
using RevitDevelop.Utils.SelectFilters;
using RevitDevelop.Utils.SkipWarning;
using RevitDevelop.Utils.Virture;
using RevitDevelop.Utils.WindowEvent.EventMouses;

namespace RevitDevelop.Test
{
    [Transaction(TransactionMode.Manual)]
    public class EditPipeCommand : ExternalCommand
    {
        private double _extendSpacing = 50.MmToFoot();
        private Pipe _pipe;
        private XYZ _pointBase;
        private List<XYZ> _controlPoints;
        private XYZ _vtxPipe;
        private XYZ _vtyPipe;
        private XYZ _vtzPipe;
        private XYZ _pipeStart;
        private XYZ _pipeEnd;
        private FaceCustom _fPipePlan;
        private FaceCustom _fPipeAlong;
        private XYZ _pFlexControl;
        private CurveVisualizationServer _curveShost;
        private int _indexInsert = 3;
        private FlexPipe _flexPipeBase;
        private HermiteSpline _hermiteSpline;
        private double _diameter;
        public override void Execute()
        {
            using (var tsg = new TransactionGroup(Document, "new Command"))
            {
                tsg.Start();
                try
                {
                    _validateView();
                    var isRepeat = true;
                    do
                    {
                        try
                        {

                            _controlPoints = new List<XYZ>();
                            _pipe = UiDocument.Selection
                                .PickElement(Document, BuiltInCategory.OST_PipeCurves) as Pipe;
                            if (_pipe == null) return;
                            _diameter = _pipe.get_Parameter(BuiltInParameter.RBS_PIPE_OUTER_DIAMETER).AsDouble();
                            _pointBase = _pickPoint(_pipe);
                            var flexPipeType = Document.GetElementsFromClass<FlexPipeType>(false)
                                .Where(x => x.Shape == ConnectorProfileType.Round)
                                .FirstOrDefault();
                            using (var ts = new Transaction(Document, "new transaction"))
                            {
                                ts.SkipAllWarnings();
                                ts.Start();
                                _configPipe(
                                    _pipe,
                                    _pointBase,
                                    out XYZ basePointStart,
                                    out XYZ basePointEnd);
                                _controlPoints.Add(basePointStart);
                                _controlPoints.Add(basePointStart + _vtxPipe * _extendSpacing);
                                _controlPoints.Add(basePointStart + _vtxPipe * _extendSpacing * 2);
                                _controlPoints.Add(basePointStart.MidPoint(basePointEnd));
                                _controlPoints.Add(basePointEnd - _vtxPipe * _extendSpacing * 2);
                                _controlPoints.Add(basePointEnd - _vtxPipe * _extendSpacing);
                                _controlPoints.Add(basePointEnd);
                                _flexPipeBase = Document.Create.NewFlexPipe(_controlPoints, flexPipeType);
                                _flexPipeBase.StartTangent = _vtxPipe;
                                _flexPipeBase.EndTangent = _vtxPipe;
                                _flexPipeBase.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM)
                                            .Set(_diameter);
                                _hermiteSpline = (_flexPipeBase.Location as LocationCurve).Curve as HermiteSpline;
                                _controlPoints.RemoveAt(_indexInsert);
                                ts.Commit();
                            }
                            var em = new EventMouseHook();
                            em.OnMouseMove += Em_OnMouseMove;
                            em.Start(System.Windows.Forms.MouseButtons.None);
                            var isdo = true;
                            do
                            {
                                try
                                {
                                    var p = UiDocument.Selection.PickPoint();
                                    _pFlexControl = p;
                                    if (_pFlexControl != null)
                                        throw new Exception();
                                }
                                catch (Exception)
                                {
                                    isdo = false;
                                    em.OnMouseMove -= Em_OnMouseMove;
                                    _curveShost?.UnAllRegister();
                                    _controlPoints.Insert(_indexInsert,
                                        _pFlexControl.RayPointToFace(_fPipePlan.Normal, _fPipePlan));
                                    using (var ts = new Transaction(Document, "new transaction"))
                                    {
                                        ts.SkipAllWarnings();
                                        ts.Start();
                                        var newFlex = Document.Create.NewFlexPipe(_controlPoints, flexPipeType);
                                        newFlex.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM)
                                            .Set(_diameter);
                                        newFlex.StartTangent = _vtxPipe;
                                        newFlex.EndTangent = _vtxPipe;
                                        Document.Delete(_flexPipeBase.Id);
                                        ts.Commit();
                                    }
                                }
                            } while (isdo);
                        }
                        catch (Exception)
                        {
                            isRepeat = false;
                        }
                    } while (isRepeat);
                    tsg.Assimilate();
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException) { }
                catch (Exception ex)
                {
                    IO.ShowWarning(ex.Message);
                    tsg.RollBack();
                }
            }
        }

        private void Em_OnMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!_controlPoints.Any())
                return;
            var p = UiDocument.GetModelCoordinatesAtCursor()
                .RayPointToFace(_fPipePlan.Normal, _fPipePlan);
            var controlPoints = _controlPoints.ToList();
            controlPoints.Insert(_indexInsert, p);
            controlPoints = controlPoints
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

        private void _configPipe(Pipe pipe, XYZ basePoint, out XYZ basePointStart, out XYZ basePointEnd)
        {
            var pipeLoca = pipe.Location as LocationCurve;
            var extendDiameter = pipe.Diameter * 4;
            var extend = extendDiameter + _extendSpacing * 2;
            basePointStart = basePoint - _vtxPipe * extend;
            basePointEnd = basePoint + _vtxPipe * extend;
            pipe.CreateNew(basePointEnd, _pipeEnd);
            pipeLoca.Curve = Line.CreateBound(_pipeStart, basePointStart);
        }
        private void _validateView()
        {
            var view = Document.ActiveView as ViewPlan;
            if (view == null) throw new Exception("view is not a Viewplan");
        }
        private XYZ _pickPoint(Pipe pipe)
        {
            var locationCurve = pipe.Location as LocationCurve;
            var c = locationCurve.Curve;
            _pipeStart = c.GetEndPoint(0);
            _pipeEnd = c.GetEndPoint(1);
            var mid = c.Mid();
            _vtxPipe = (_pipeEnd - _pipeStart).Normalize();
            _vtyPipe = _vtxPipe.CrossProduct(XYZ.BasisZ);
            _vtzPipe = _vtxPipe.CrossProduct(_vtyPipe);
            _fPipePlan = new FaceCustom(_vtzPipe, mid);
            _fPipeAlong = new FaceCustom(_vtyPipe, mid);
            var p = UiDocument
                .Selection
                .PickPoint()
                .RayPointToFace(XYZ.BasisZ, _fPipePlan);
            var pOnPipe = p.RayPointToFace(_vtyPipe, _fPipeAlong);
            var d1Mm = Math.Round(pOnPipe.DistanceTo(_pipeStart).FootToMm(), 0);
            if (d1Mm == 0) return null;
            var d2Mm = Math.Round(pOnPipe.DistanceTo(_pipeEnd).FootToMm(), 0);
            if (d2Mm == 0) return null;
            var vt1 = (pOnPipe - _pipeStart).Normalize();
            var vt2 = (pOnPipe - _pipeEnd).Normalize();
            if (vt1.DotProduct(vt2) >= 0)
                return null;
            if (pOnPipe.DistanceTo(p).FootToMm() > 500)
                return null;
            return pOnPipe;
        }
    }
}
