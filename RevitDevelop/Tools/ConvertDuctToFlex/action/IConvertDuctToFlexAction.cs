using Autodesk.Revit.DB.Mechanical;

namespace RevitDevelop.Tools.ConvertDuctToFlex.action
{
    public interface IConvertDuctToFlexAction
    {
        //elements of option A
        public List<Element> GetElementsByFittingToFitting(
            FamilyInstance fittingStart, 
            FamilyInstance fittingEnd);
        //elements of option B
        public List<Element> GetElementsByFittingToDistance(
            FamilyInstance fittingStart, 
            double lengthMm);
        //elements of option C
        public List<Element> GetElementsByFittingToDistanceExactly(
            FamilyInstance fittingStart, 
            double lengthMm,
            out Duct ductLast,
            out double lengthConvertMm);
        public void ExcuteOptionA();
        public void ExcuteOptionB();
        public void ExcuteOptionC();
        public void Excute();
    }
}
