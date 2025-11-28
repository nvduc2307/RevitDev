using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Nice3point.Revit.Extensions;
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

        private const double EPS = 1e-9;

        public override void Execute()
        {
            using (var tsg = new TransactionGroup(Document, "new Command"))
            {
                tsg.Start();
                try
                {
                    //_validateView();
                    var isRepeat = true;
                    do
                    {
                        try
                        {
                            _controlPoints = new List<XYZ>();
                            _pipe = UiDocument.Selection
                                .PickElement(Document, BuiltInCategory.OST_PipeCurves) as Pipe;
                            if (_pipe == null) return;
                            _diameter = _pipe.Diameter;
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

                                    if (p.Z - _pointBase.Z > 2)
                                    {
                                        var td = new TaskDialog("Exceeding the length limit");
                                        td.MainInstruction = "The pull point exceeds the allowable limit..";
                                        td.MainContent = $"Do you want to create a fitting to compensate for the gap?";
                                        td.CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No;
                                        td.DefaultButton = TaskDialogResult.No;

                                        var result = td.Show();
                                        if (result == TaskDialogResult.Yes)
                                        {
                                            using (Transaction tr = new Transaction(Document, "Create Flex"))
                                            {
                                                tr.Start();

                                                _pFlexControl = p;

                                                var controlFlex1 = CreateFlexByControlPoint(p, true, flexPipeType);
                                                var controlFlex2 = CreateFlexByControlPoint(p, false, flexPipeType);

                                                var elbow = CreateElbowBetweenFlexPipes(Document, controlFlex1, controlFlex2, p);

                                                Document.Delete(_flexPipeBase.Id);

                                                isdo = false;
                                                em.OnMouseMove -= Em_OnMouseMove;
                                                _curveShost?.UnAllRegister();

                                                tr.Commit();
                                            }
                                        }
                                        else
                                        {
                                            p = new XYZ(p.X, p.Y, _pointBase.Z + 2);

                                            _pFlexControl = p;
                                            if (_pFlexControl != null)
                                                throw new Exception();
                                        }
                                    }
                                    else
                                    {
                                        _pFlexControl = p;
                                        if (_pFlexControl != null)
                                            throw new Exception();
                                    }
                                }
                                catch (Exception)
                                {
                                    isdo = false;
                                    em.OnMouseMove -= Em_OnMouseMove;
                                    _curveShost?.UnAllRegister();
                                    _controlPoints.Insert(_indexInsert,
                                        _pFlexControl.RayPointToFace(_fPipeAlong.Normal, _fPipeAlong));
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

        private FlexPipe CreateFlexByControlPoint(XYZ p, bool isFirstFlex, FlexPipeType flexType)
        {
            List<XYZ> lstControlPoint = new List<XYZ>();
            XYZ dirStartTargent = null;
            XYZ dirEndTargent = null;
            XYZ offsetDir = XYZ.BasisZ;
            double minAngleDeg = 10.0;
            double maxAngleDeg = 170.0;
            double stepFt = 50 / 304.8;

            var pointRayToFace = _pFlexControl.RayPointToFace(_fPipeAlong.Normal, _fPipeAlong);
            if (isFirstFlex)
            {
                lstControlPoint.Add(_controlPoints.FirstOrDefault());

                var startMain = _controlPoints.First();
                var endMain = pointRayToFace;

                lstControlPoint.Add(endMain);

                dirStartTargent = _vtxPipe;
                dirEndTargent = (lstControlPoint.LastOrDefault() - lstControlPoint.FirstOrDefault()).Normalize();
            }
            else
            {
                lstControlPoint.Add(_controlPoints.LastOrDefault());

                var startMain = _controlPoints.LastOrDefault();
                var endMain = pointRayToFace;

                lstControlPoint.Add(endMain);

                dirStartTargent = _vtxPipe.Negate();
                dirEndTargent = (lstControlPoint.FirstOrDefault() - lstControlPoint.LastOrDefault()).Normalize();
            }

            var newFlex = Document.Create.NewFlexPipe(lstControlPoint, flexType);
            newFlex.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM)?.Set(_diameter);
            newFlex.StartTangent = dirStartTargent;
            newFlex.EndTangent = dirEndTargent;

            return newFlex;
        }

        public FamilyInstance CreateElbowBetweenFlexPipes(Document doc, FlexPipe flex1, FlexPipe flex2, XYZ pointPick)
        {
            if (doc == null) throw new ArgumentNullException(nameof(doc));
            if (flex1 == null) throw new ArgumentNullException(nameof(flex1));
            if (flex2 == null) throw new ArgumentNullException(nameof(flex2));
            if (pointPick == null) throw new ArgumentNullException(nameof(pointPick));

            var c1 = GetClosestConnector(flex1, pointPick);
            var c2 = GetClosestConnector(flex2, pointPick);

            if (c1 == null || c2 == null)
                return null;

            var sym = doc.GetElement(new ElementId(133158)) as FamilySymbol;
            var lvl = doc.GetElement(new ElementId(311)) as Level;
            if (sym == null || lvl == null) return null;

            if (!sym.IsActive) sym.Activate();

            FamilyInstance fi = doc.Create.NewFamilyInstance(pointPick, sym, lvl, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

            var p = fi.get_Parameter(BuiltInParameter.INSTANCE_ELEVATION_PARAM);
            if (p != null && !p.IsReadOnly && p.StorageType == StorageType.Double)
            {
                try
                {
                    p.Set(pointPick.Z - (lvl?.Elevation ?? 0));
                }
                catch { }
            }

            foreach (Parameter pa in fi.Parameters)
            {
                if (pa.StorageType == StorageType.Double && pa.Definition?.Name == "Nominal Radius")
                {
                    pa.Set(flex1.Diameter / 2);
                }
            }

            doc.Regenerate();

            var targetHand = new XYZ(0.707106781, 0.0, 0.707106781);
            var targetFacing = new XYZ(0.707106781, 0.0, -0.707106781);

            FittingOrientationUtils.AlignByHandFacing(fi, targetHand, targetFacing);

            doc.Regenerate();

            List<Connector> cons = fi.MEPModel.ConnectorManager.Connectors.Cast<Connector>().OrderBy(x => x.Origin.X).ToList();

            c1.ConnectTo(cons.FirstOrDefault());
            c2.ConnectTo(cons.LastOrDefault());

            _vtxPipe = _vtxPipe.Normalize();
            var delta = _vtxPipe * _diameter * 0.5;
            ElementTransformUtils.MoveElement(doc, fi.Id, delta);
            ElementTransformUtils.MoveElement(doc, fi.Id, delta.Negate());

            return fi;
        }

        private Connector GetClosestConnector(MEPCurve mepCurve, XYZ point)
        {
            var cm = mepCurve?.ConnectorManager;
            if (cm == null) return null;

            Connector closest = null;
            double minDist = double.MaxValue;

            foreach (Connector c in cm.Connectors)
            {
                if (c == null) continue;
                var d = c.Origin.DistanceTo(point);
                if (d < minDist)
                {
                    minDist = d;
                    closest = c;
                }
            }

            return closest;
        }

        private void Em_OnMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!_controlPoints.Any())
                return;

            UiDocument.TryGetRayUnderMouse(out XYZ origin, out XYZ direction);
            var p = origin.RayPointToFace(_fPipeAlong.Normal, _fPipeAlong);

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
            _setSketchPlan(Document, Plane.CreateByNormalAndOrigin(_vtyPipe, mid));
            var p = UiDocument
                .Selection
                .PickPoint()
                .RayPointToFace(_fPipeAlong.Normal, _fPipeAlong);
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

        private void _setSketchPlan(Document document, Plane plane)
        {
            try
            {
                using (var ts = new Transaction(document, "new transaction"))
                {
                    ts.SkipAllWarnings();
                    ts.Start();
                    var sketchPlane = SketchPlane.Create(document, plane);
                    document.ActiveView.SketchPlane = sketchPlane;
                    ts.Commit();
                }
            }
            catch (Exception)
            {
            }
        }
    }
}