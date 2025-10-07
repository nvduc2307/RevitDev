using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using CommunityToolkit.Mvvm.DependencyInjection;
using Nice3point.Revit.Toolkit.External;
using RevitDevelop.Test.viewModels;
using RevitDevelop.Test.views;
using RevitDevelop.Utils.Compares;
using RevitDevelop.Utils.Geometries;
using RevitDevelop.Utils.HookHelper;
using RevitDevelop.Utils.Idlings;
using RevitDevelop.Utils.Messages;
using RevitDevelop.Utils.PressHelpers;
using RevitDevelop.Utils.RevFaces;
using RevitDevelop.Utils.RevSketchPlan;
using RevitDevelop.Utils.SkipWarning;
using RevitDevelop.Utils.Virture;
using RevitDevelop.Utils.WindowElements;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RevitDevelop.Test
{
    [Transaction(TransactionMode.Manual)]
    public class TestCmd : ExternalCommand
    {
        private TestView _view;
        public override void Execute()
        {

            using (var tsg = new TransactionGroup(Document, "name transaction group"))
            {
                tsg.Start();
                try
                {
                    var view = Document.ActiveView;
                    if (!(view is ViewPlan || view is ViewSection))
                        return;
                    var d = new UserActivityHook(true, false);
                    d.OnMouseActivity += D_OnMouseActivity1;
                    //using (var ts = new Transaction(Document, "new transaction"))
                    //{
                    //    ts.SkipAllWarnings();
                    //    ts.Start();
                    //    Document.SetSketchPlan();
                    //    ts.Commit();
                    //}

                    //XYZ A = UiDocument.Selection.PickPoint("Pick first point");
                    //CurveVisualizationServer curveShost = null;
                    //IdlingLoop.ExternalAction += () =>
                    //{
                    //    view = Document.ActiveView;
                    //    var level = view.GenLevel;
                    //    var vtx = view.RightDirection;
                    //    var vty = view.UpDirection;
                    //    var vtz = view.ViewDirection;
                    //    var orgin = view.Origin;
                    //    var f = new FaceCustom(vtz, orgin);
                    //    var p = UiDocument.GetModelCoordinatesAtCursor()
                    //    .RayPointToFace(f.Normal, f);
                    //    var p0 = A.RayPointToFace(f.Normal, f);
                    //    if (level != null)
                    //    {
                    //        p0 = new XYZ(p0.X, p0.Y, level.Elevation);
                    //        p = new XYZ(p.X, p0.Y, level.Elevation);
                    //    }
                    //    var l = Line.CreateBound(p0, p);
                    //    if (curveShost != null)
                    //        curveShost.UnAllRegister();
                    //    curveShost = new CurveVisualizationServer(UiDocument, new List<Line>() { l });
                    //    curveShost.Register();
                    //};
                    //IdlingLoop.Start(UiApplication);
                    //if (PressHelper.IsEnterKeyPressed())
                    //    IdlingLoop.Stop(UiApplication);
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

        private void D_OnMouseActivity1(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            var _lastMousePosition = e.Location;
        }
    }
}
