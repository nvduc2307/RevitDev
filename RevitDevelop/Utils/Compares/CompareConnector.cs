using Autodesk.Revit.DB.Mechanical;
using RevitDevelop.Utils.Geometries;
using RevitDevelop.Utils.RevDuct;

namespace RevitDevelop.Utils.Compares
{
    public class CompareConnector : IEqualityComparer<Element>
    {
        public bool Equals(Element x, Element y)
        {
            if (x is not Duct && x is not FamilyInstance) return false;
            if (y is not Duct && y is not FamilyInstance) return false;
            var connectorsX = x is Duct
                ? (x as Duct).GetConnectors()
                : (x as FamilyInstance).GetConnectors();
            var connectorsY = y is Duct
                ? (y as Duct).GetConnectors()
                : (y as FamilyInstance).GetConnectors();
            var connect = connectorsX
                .FirstOrDefault(x => connectorsY.Any(y => x.Owner.Id.ToString() == y.Owner.Id.ToString() || y.Origin.IsSame(x.Origin, 10)));
            if (connect == null) return false;
            return true;
        }

        public int GetHashCode(Element obj)
        {
            return 0;
        }
    }
}
