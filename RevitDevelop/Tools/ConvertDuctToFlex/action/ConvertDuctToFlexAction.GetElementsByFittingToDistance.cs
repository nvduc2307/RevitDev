using Autodesk.Revit.DB.Mechanical;
using RevitDevelop.Utils.RevDuct;

namespace RevitDevelop.Tools.ConvertDuctToFlex.action
{
    public partial class ConvertDuctToFlexAction : IConvertDuctToFlexAction
    {
        public List<Element> GetElementsByFittingToDistance(
            FamilyInstance fittingStart,
            Duct ductDirection,
            double lengthMm)
        {
            var result = new List<Element>();
            result = ductDirection.GetElementsByConnector(null, fittingStart);
            return result;
        }
    }
}
