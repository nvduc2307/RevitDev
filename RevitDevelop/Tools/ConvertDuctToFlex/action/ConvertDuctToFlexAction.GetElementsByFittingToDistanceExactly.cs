using Autodesk.Revit.DB.Mechanical;

namespace RevitDevelop.Tools.ConvertDuctToFlex.action
{
    public partial class ConvertDuctToFlexAction : IConvertDuctToFlexAction
    {
        public List<Element> GetElementsByFittingToDistanceExactly(
            FamilyInstance fittingStart,
            Duct ductDirection,
            double lengthMm, 
            out Duct ductLast, 
            out double lengthConvertMm)
        {
            ductLast = null;
            lengthConvertMm = 0;
            var result = new List<Element>();
            return result;
        }
    }
}
