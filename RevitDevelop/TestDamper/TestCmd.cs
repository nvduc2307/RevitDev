using Autodesk.Revit.Attributes;
using Nice3point.Revit.Toolkit.External;
using RevitDevelop.Utils.Geometries;
using RevitDevelop.Utils.Messages;
using RevitDevelop.Utils.SelectFilters;
using RevitDevelop.Utils.Virture;
using RevitDevelop.Utils.WindowEvent.EventMouses;

namespace RevitDevelop.TestDamper
{
    [Transaction(TransactionMode.Manual)]
    public class TestCmd : ExternalCommand
    {
        private EventMouseHook _mev;
        private List<XYZ> _ps;
        private DetailCurve _l;
        private CurveVisualizationServer _curveShost;
        public override void Execute()
        {
            using (var tsg = new TransactionGroup(Document, "new Command"))
            {
                tsg.Start();
                try
                {
                    var familyInstance = UiDocument.Selection.PickElement(Document) as FamilyInstance;
                    if (familyInstance == null) return;
                    var dir = GetHandlePositionForRoundDamper(familyInstance, out double tw);
                    IO.ShowInfo(dir);
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
        private string GetHandlePositionForRoundDamper(FamilyInstance damper, out double twistAngle)
        {
            twistAngle = 0;
            var angleCheck = 45;
            var result = string.Empty;
            if (damper == null) return result;
            var trans = damper.GetTransform();
            var vtx = trans.BasisX;
            var vty = trans.BasisY;
            var z = !trans.BasisX.IsParallel(XYZ.BasisZ) ? XYZ.BasisZ : XYZ.BasisY;
            if (vtx.IsParallel(z)) return result;
            var vtHandPos = damper.FacingFlipped ? vty : -vty;
            var vtyGlobal = -vtx.CrossProduct(z);
            var vtzGlobal = vtx.CrossProduct(vtyGlobal);

            int roundDigits = 3;
            var angle = Math.Round(vtHandPos.AngleTo(vtyGlobal) * 180 / Math.PI, roundDigits);
            twistAngle = angle;
            if (angle <= angleCheck)
            {
                twistAngle = angle;
                return "StringDefinition.Left";
            }
            angle = Math.Round(vtHandPos.AngleTo(-vtyGlobal) * 180 / Math.PI, roundDigits);
            if (angle <= angleCheck)
            {
                twistAngle = angle;
                return "StringDefinition.Right";
            }
            angle = Math.Round(vtHandPos.AngleTo(vtzGlobal) * 180 / Math.PI, roundDigits);
            if (angle <= angleCheck)
            {
                twistAngle = angle;
                return "StringDefinition.Top";
            }
            angle = Math.Round(vtHandPos.AngleTo(-vtzGlobal) * 180 / Math.PI, roundDigits);
            if (angle <= angleCheck)
            {
                twistAngle = angle;
                return "StringDefinition.Bottom";
            }
            return result;
        }
    }
}
