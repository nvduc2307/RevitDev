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
        //elements of option C
        public List<Element> GetElementsByFittingToDistance(
            FamilyInstance fittingStart,
            List<Element> elementAlreadySort,
            double lengthMm,
            out Duct ductLast,
            out XYZ ductLastPointStart,
            out XYZ ductLastPointIntersect,
            out XYZ ductLastPointEnd);
        public void ExcuteOptionA();
        public void ExcuteOptionB();
        public void ExcuteOptionC();
        public void Excute();
    }
}
