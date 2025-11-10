using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Nice3point.Revit.Toolkit.External;
using RevitDevelop.Test.views;
using RevitDevelop.Utils.Geometries;
using RevitDevelop.Utils.Messages;
using RevitDevelop.Utils.NumberUtils;
using RevitDevelop.Utils.RevFaces;
using RevitDevelop.Utils.RevSketchPlan;
using RevitDevelop.Utils.SelectFilters;
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
        private FamilyInstance _wall;
        public override void Execute()
        {
            using (var tsg = new TransactionGroup(Document, "name transaction group"))
            {
                tsg.Start();
                try
                {
                    var ele = UiDocument.Selection.PickElement(Document, BuiltInCategory.OST_Walls);
                    if (ele is Wall wall)
                    {
                        var editBy = wall.get_Parameter(BuiltInParameter.EDITED_BY).AsString();
                        IO.ShowWarning(editBy);
                    }
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
    }
}
