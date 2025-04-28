using RevitDevelop.Utils.Geometries;

namespace RevitDevelop.Utils.Compares
{
    public class CompareElement
    {
    }
    public class CompareLine : IEqualityComparer<Line>
    {
        public bool Equals(Line x, Line y)
        {
            var p1 = x.GetEndPoint(0);
            var p2 = x.GetEndPoint(0);
            var p11 = y.GetEndPoint(0);
            var p22 = y.GetEndPoint(0);

            var dk1 = p1.IsSame(p11) || p1.IsSame(p22);
            var dk2 = p2.IsSame(p11) || p2.IsSame(p22);

            return dk1 && dk2;
        }

        public int GetHashCode(Line obj)
        {
            return 0;
        }
    }
}
