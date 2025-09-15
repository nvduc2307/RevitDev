using Autodesk.Revit.DB.Mechanical;

namespace RevitDevelop.Tools.PolylineToFlex.IActions
{
    public interface IPolylineToFlexAction
    {
        public void Excute(FlexDuctType flexDuctType, MechanicalSystemType systemType, double flexDuctDiamterMm, Level level);
    }
}
