namespace RevitDevelop.Utils.RevArcs
{
    public static class RevArcUtils
    {
        public static XYZ GetCenter(Arc arc)
        {
            return arc.Center;
        }
        public static XYZ GetNormal(Arc arc) { return arc.Normal; }
    }
}
