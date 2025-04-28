using RevitDevelop.Utils.Geometries;
using RevitDevelop.Utils.RevCurves;
using RevitDevelop.Utils.RevFaces;

namespace RevitDevelop.Utils.Compares
{
    public class CompareGrid : IEqualityComparer<Grid>
    {
        private ViewPlan _viewPlan;
        public CompareGrid(ViewPlan viewPlan)
        {
            _viewPlan = viewPlan;
        }
        public bool Equals(Grid x, Grid y)
        {
            try
            {
                var c1 = (x as DatumPlane).GetCurvesInView(DatumExtentType.ViewSpecific, _viewPlan).ElementAt(0).Direction();
                var c2 = (y as DatumPlane).GetCurvesInView(DatumExtentType.ViewSpecific, _viewPlan).ElementAt(0).Direction();
                return c1.IsParallel(c2);
            }
            catch (Exception)
            {
            }
            return false;
        }

        public int GetHashCode(Grid obj)
        {
            return 0;
        }
    }
    public class CompareGridOverLap : IEqualityComparer<Grid>
    {
        private ViewPlan _viewPlan;
        public CompareGridOverLap(ViewPlan viewPlan)
        {
            _viewPlan = viewPlan;
        }
        public bool Equals(Grid x, Grid y)
        {
            try
            {
                var c1 = (x as DatumPlane).GetCurvesInView(DatumExtentType.ViewSpecific, _viewPlan).ElementAt(0);
                var c2 = (y as DatumPlane).GetCurvesInView(DatumExtentType.ViewSpecific, _viewPlan).ElementAt(0);
                var isPll = c1.Direction().IsParallel(c2.Direction());
                if (!isPll) return false;

                var face = new FaceCustom(c1.Direction(), c1.Mid());
                var p1 = c1.Mid().RayPointToFace(c1.Direction(), face);
                var p2 = c2.Mid().RayPointToFace(c1.Direction(), face);
                return p1.IsSame(p2);
            }
            catch (Exception)
            {
            }
            return false;
        }

        public int GetHashCode(Grid obj)
        {
            return 0;
        }
    }
}
