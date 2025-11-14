using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Plumbing;
using Nice3point.Revit.Toolkit.External;
using RevitDevelop.Utils.Geometries;
using RevitDevelop.Utils.NumberUtils;
using RevitDevelop.Utils.SelectFilters;
using RevitDevelop.Utils.SkipWarning;
using RevitDevelop.Utils.Virture;

namespace RevitDevelop.Test
{
    [Transaction(TransactionMode.Manual)]
    public class Test2Command : ExternalCommand
    {
        private CurveVisualizationServer _curveShost;
        private XYZ _sp;
        private XYZ _ep;
        private HermiteSpline _hermiteSpline;
        public override void Execute()
        {

            using (var tsg = new TransactionGroup(Document, "new Command"))
            {
                tsg.Start();
                try
                {
                    var ele = UiDocument.Selection.PickElement(Document, BuiltInCategory.OST_FlexPipeCurves);
                    if (ele is not FlexPipe flexPipe)
                        return;
                    var location = flexPipe.Location as LocationCurve;
                    _sp = location.Curve.GetEndPoint(0);
                    _ep = location.Curve.GetEndPoint(1);
                    _hermiteSpline = location.Curve as HermiteSpline;
                    var view = Document.ActiveView as ViewPlan;
                    if (view == null) return;
                    var p = UiDocument.Selection.PickPoint().EditZ(view.GenLevel.Elevation);
                    var controls = _hermiteSpline.ControlPoints.Select(x => x.EditZ(view.GenLevel.Elevation)).ToList();
                    TestCmd.GetindexControl(Document, _hermiteSpline, p, out int controlIndex);
                    ///=========
                    var pControl = controls[controlIndex];
                    var isdistanceMm = controls.Any(x=>x.DistanceTo(p).FootToMm() <= 50);
                    if (isdistanceMm)
                    {

                    }
                    else
                    {
                        var extent = 50.MmToFoot();
                        var pControlPre = controls[controlIndex - 1];
                        var vt = (pControl - pControlPre).Normalize();
                        var pInsert1 = p - vt * extent * 3;
                        var pInsert2 = p - vt * extent * 2;
                        var pInsert3 = p - vt * extent;
                        var pInsert4 = p;
                        var pInsert5 = p + vt * extent;
                        var pInsert6 = p + vt * extent * 2;
                        var pInsert7 = p + vt * extent * 3;
                        controls.Insert(controlIndex, pInsert7);
                        controls.Insert(controlIndex, pInsert6);
                        controls.Insert(controlIndex, pInsert5);
                        controls.Insert(controlIndex, pInsert4);
                        controls.Insert(controlIndex, pInsert3);
                        controls.Insert(controlIndex, pInsert2);
                        controls.Insert(controlIndex, pInsert1);
                    }
                    controls = controls.Select(x => x.EditZ((Document.ActiveView as ViewPlan).GenLevel.Elevation)).ToList();
                    using (var ts = new Transaction(Document, "new transaction"))
                    {
                        ts.SkipAllWarnings();
                        ts.Start();
                        Document.Create.NewFlexPipe(controls, flexPipe.FlexPipeType);
                        Document.Delete(ele.Id);
                        ts.Commit();
                    }
                    tsg.Assimilate();
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException) { }
                catch (Exception ex)
                {
                    tsg.RollBack();
                }
            }

        }
    }
}
