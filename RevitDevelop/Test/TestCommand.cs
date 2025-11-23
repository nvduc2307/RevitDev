using Autodesk.Revit.Attributes;
using Nice3point.Revit.Toolkit.External;
using RevitDevelop.Utils.Geometries;
using RevitDevelop.Utils.Messages;
using RevitDevelop.Utils.NumberUtils;
using RevitDevelop.Utils.SelectFilters;

namespace RevitDevelop.Test
{
    [Transaction(TransactionMode.Manual)]
    public class TestCommand : ExternalCommand
    {
        public override void Execute()
        {

            using (var tsg = new TransactionGroup(Document, "new Command"))
            {
                tsg.Start();
                try
                {
                    var e = UiDocument.Selection.PickElement(Document) as FamilyInstance;
                    if (e == null) return;
                    _checkDirectionDoor(e);
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
        private Direction _checkDirectionDoor(FamilyInstance fe)
        {
            var result = Direction.None;
            var trans = fe.GetTransform();
            var vtx = trans.OfVector(XYZ.BasisX);
            var vty = vtx.CrossProduct(XYZ.BasisZ);
            var vtz = vtx.CrossProduct(vty);
            vty = vtx.CrossProduct(vtz).Normalize();
            var facing = fe.FacingOrientation;
            var h = Math.Round(fe.LookupParameter("Height").AsDouble().FootToMm(), 0);
            var w = Math.Round(fe.LookupParameter("Width").AsDouble().FootToMm(), 0);
            //song song voi truc y
            if (facing.IsParallel(vty))
            {
                if (facing.IsSameDirection(vty))
                {
                    // cung chieu voi vty
                    result = h <= 300 ? Direction.Bottom : Direction.Left;
                }
                else
                {
                    // nguoc chieu voi vty
                    result = h <= 300 ? Direction.Top : Direction.Right;
                }
            }
            //song song voi truc z
            if (facing.IsParallel(vtz))
            {
                if (facing.IsSameDirection(vtz))
                {
                    // cung chieu voi vtz
                    result = h <= 300 ? Direction.Left : Direction.Top;
                }
                else
                {
                    // nguoc chieu voi vtz
                    result = h <= 300 ? Direction.Right : Direction.Bottom;
                }
            }
            IO.ShowInfo(result.ToString());
            return result;
        }
    }
    public enum Direction
    {
        None = -1,
        Right = 0,
        Top = 1,
        Left = 2,
        Bottom = 3,
    }
}
