using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Nice3point.Revit.Toolkit.External;
using RevitDevelop.Utils.Geometries;
using RevitDevelop.Utils.Messages;
using RevitDevelop.Utils.NumberUtils;
using RevitDevelop.Utils.RevCurves;
using RevitDevelop.Utils.SelectFilters;
using RevitDevelop.Utils.SkipWarning;

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
                    var vtz = _getVector(e);
                    var origin = e.GetTransform().Origin;
                    var l = Line.CreateBound(origin, origin + vtz * 1000.MmToFoot());
                    using (var ts = new Transaction(Document, "new transaction"))
                    {
                        ts.SkipAllWarnings();
                        ts.Start();
                        Document.CreateCurves(new List<Curve>() { l });
                        ts.Commit();
                    }
                    //_checkDirectionDoor(e);
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
            var paramHieght = "Height";
            var distanceCheck = 300;
            var result = Direction.None;
            var trans = fe.GetTransform();
            var vtx = trans.OfVector(XYZ.BasisX);
            var vtz = _getVector(fe);
            var vty = vtx.CrossProduct(vtz).Normalize();
            var facing = fe.FacingOrientation;
            var handVt = -fe.FacingOrientation;
            var h = Math.Round(fe.LookupParameter(paramHieght).AsDouble().FootToMm(), 0);
            //song song voi truc y
            if (handVt.IsParallel(vty))
            {
                if (handVt.IsSameDirection(vty))
                {
                    // cung chieu voi vty
                    result = Direction.Left;
                }
                else
                {
                    // nguoc chieu voi vty
                    result = Direction.Right;
                }
            }
            //song song voi truc z
            if (handVt.IsParallel(vtz))
            {
                if (handVt.IsSameDirection(vtz))
                {
                    // cung chieu voi vtz
                    result = Direction.Top;
                }
                else
                {
                    // nguoc chieu voi vtz
                    result = Direction.Bottom;
                }
            }
            IO.ShowInfo(result.ToString());
            return result;
        }

        private XYZ _getVector(FamilyInstance fe)
        {
            XYZ result = null;
            var trans = fe.GetTransform();
            var origin = trans.Origin;
            var vtfx = trans.OfVector(XYZ.BasisX);
            var facing = fe.FacingOrientation;
            var hand = vtfx.CrossProduct(facing);
            var angle = 0.0;
            if (facing.DotProduct(XYZ.BasisZ) == 1)
                return facing;
            if (facing.DotProduct(XYZ.BasisZ) == -1)
                return -facing;
            if (hand.DotProduct(XYZ.BasisZ) == 1)
                return hand;
            if (hand.DotProduct(XYZ.BasisZ) == -1)
                return -hand;
            var yVector = vtfx.CrossProduct(XYZ.BasisZ);
            var zVector = vtfx.CrossProduct(yVector);
            zVector = zVector.DotProduct(XYZ.BasisZ) <=0 ? -zVector: zVector;
            angle = Math.Round(zVector.AngleTo(facing) * 180 / Math.PI, 0);
            //angle = angle > 90 ? 180 - angle : angle;
            if (angle <= 45)
                result = facing;
            else if (180 - angle <= 45)
                result = -facing;
            else
            {
                var vt = vtfx.CrossProduct(facing);
                angle = Math.Round(zVector.AngleTo(vt) * 180 / Math.PI, 0);
                result = angle <= 45 ? vt : -vt;
            }
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
