using Autodesk.Revit.DB.Mechanical;

namespace RevitDevelop.DaitoTest.Utils
{
    public static class FittingUtils
    {
        public static bool IsElbow(this FamilyInstance fitting, List<BuiltInCategory> cateFittings = null)
        {
            if (cateFittings == null)
            {
                cateFittings = new List<BuiltInCategory>() {
                    BuiltInCategory.OST_DuctFitting,
                    BuiltInCategory.OST_PipeFitting,
                };
            }
            if (fitting == null) return false;
            if (!cateFittings.Any(x=>x == fitting.Category.BuiltInCategory))
                return false;
            var mep = fitting.MEPModel;
            if(mep == null) return false;
            if (mep is not MechanicalFitting mechan) return false;
            if(mechan.PartType != PartType.Elbow) return false;
            return true;
        }
        public static int GetTypeFitting(this FamilyInstance fitting, List<BuiltInCategory> cateFittings = null)
        {
            if (cateFittings == null)
            {
                cateFittings = new List<BuiltInCategory>() {
                    BuiltInCategory.OST_DuctFitting,
                    BuiltInCategory.OST_PipeFitting,
                };
            }
            var result = -1;
            if (fitting == null) return result;
            if (!cateFittings.Any(x => x == fitting.Category.BuiltInCategory))
                return result;
            var mep = fitting.MEPModel;
            if (mep == null) return result;
            if (mep is not MechanicalFitting mechan) return result;
            return (int)mechan.PartType;
        }
    }
}
