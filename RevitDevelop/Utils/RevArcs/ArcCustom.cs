using RevitDevelop.Utils.Geometries;

namespace RevitDevelop.Utils.RevArcs
{
    public class ArcCustom
    {
        public Arc Arc { get; }
        public XYZ Center { get; }
        public XYZ Normal { get; }
        public XYZ Start { get; }
        public XYZ End { get; }
        public XYZ Mid { get; }
        public List<Curve> Curves { get; }

        public ArcCustom(Arc arc)
        {
            Arc = arc;
            Center = arc.Center;
            Normal = arc.Normal;
            var ps = Arc.Tessellate();
            Start = ps.FirstOrDefault();
            End = ps.LastOrDefault();
            Mid = ps.Count() > 2 ? ps[ps.Count() / 2] : null;
            Curves = arc.Tessellate().ToList().PointsToCurves();
        }
    }
}
