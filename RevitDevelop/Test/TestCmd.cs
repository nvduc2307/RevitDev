using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Architecture;
using Nice3point.Revit.Toolkit.External;
using RevitDevelop.Utils.RevCurves;
using RevitDevelop.Utils.SelectFilters;

namespace RevitDevelop.Test
{
    [Transaction(TransactionMode.Manual)]
    public class TestCmd : ExternalCommand
    {
        public override void Execute()
        {
            using (var tsg = new TransactionGroup(Document, "name transaction group"))
            {
                tsg.Start();
                try
                {
                    //--------
                    SpatialElementBoundaryOptions options = new SpatialElementBoundaryOptions();
                    var room = Document.GetElement(UiDocument.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, new GenericSelectionFilter(BuiltInCategory.OST_Rooms))) as Room;
                    var curves = room.GetBoundarySegments(options)
                        .FirstOrDefault()
                        .Select(x => x.GetCurve())
                        .ToList();

                    using (var ts = new Transaction(Document, "name transaction"))
                    {
                        ts.Start();
                        //--------
                        Document.CreateCurves(curves);
                        //--------
                        ts.Commit();
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
