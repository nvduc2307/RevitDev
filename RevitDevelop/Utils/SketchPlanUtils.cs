namespace RevitDevelop.Utils
{
    public static class SketchPlanUtils
    {
        public static void SetSketchPlan(this Document document)
        {
            var plane = Plane.CreateByNormalAndOrigin(document.ActiveView.ViewDirection, document.ActiveView.Origin);
            var sketchPlane = SketchPlane.Create(document, plane);
            document.ActiveView.SketchPlane = sketchPlane;
        }
    }
}
