using RevitDevelop.Utils.FilterElementsInRevit;
using RevitDevelop.Utils.RevCurves;
using RevitDevelop.Utils.StringUtils;

namespace RevitDevelop.Utils.RevViews
{
    public static class RevViewUtils
    {
        public static bool IsExistedViewName(this string nameView, List<View> views, out string nameViewGenerate)
        {
            nameViewGenerate = nameView;
            try
            {
                var isExistedView = views.Any(x => x.Name == nameView);
                if (isExistedView)
                {
                    var viewContainNames = views.Where(x => x.Name.Contains(nameView));
                    var maxIndex = viewContainNames.Max(x => x.Name.GetInterger(nameView));
                    nameViewGenerate = $"{nameView} ({maxIndex + 1})";
                }
                return isExistedView;
            }
            catch (Exception)
            {
            }
            return false;
        }
        public static void SetViewRange(this ViewPlan view, double topPlane = 0, double cutPlane = 0)
        {
            try
            {
                var level = view.GenLevel;
                var viewRange = view.GetViewRange();
                viewRange.SetLevelId(PlanViewPlane.BottomClipPlane, PlanViewRange.Unlimited);
                viewRange.SetLevelId(PlanViewPlane.ViewDepthPlane, PlanViewRange.Unlimited);
                if (!viewRange.IsValidObject) return;
                viewRange.SetLevelId(PlanViewPlane.TopClipPlane, level.Id);
                viewRange.SetLevelId(PlanViewPlane.CutPlane, level.Id);
                viewRange.SetOffset(PlanViewPlane.TopClipPlane, topPlane);
                viewRange.SetOffset(PlanViewPlane.CutPlane, cutPlane);
                view.SetViewRange(viewRange);
            }
            catch (Exception)
            {
            }
        }
        public static void SetViewRangeBeam(this ViewPlan view, double topPlane = 0, double cutPlane = 0, double bottomPlane = -1000 / 304.8, double viewDepth = -2000 / 304.8)
        {
            try
            {
                var level = view.GenLevel;
                var viewRange = view.GetViewRange();

                // Đặt tất cả các mặt phẳng sử dụng level hiện tại
                viewRange.SetLevelId(PlanViewPlane.TopClipPlane, level.Id);
                viewRange.SetLevelId(PlanViewPlane.CutPlane, level.Id);
                viewRange.SetLevelId(PlanViewPlane.BottomClipPlane, level.Id);
                viewRange.SetLevelId(PlanViewPlane.ViewDepthPlane, level.Id);

                // Đặt offset cho các mặt phẳng
                viewRange.SetOffset(PlanViewPlane.TopClipPlane, topPlane);
                viewRange.SetOffset(PlanViewPlane.CutPlane, cutPlane);
                viewRange.SetOffset(PlanViewPlane.BottomClipPlane, bottomPlane);
                viewRange.SetOffset(PlanViewPlane.ViewDepthPlane, viewDepth);

                if (!viewRange.IsValidObject) return;
                view.SetViewRange(viewRange);
            }
            catch (Exception)
            {
            }
        }
        public static void SetViewRange(this ViewPlan view, Level level, double topPlane, double cutPlane, double bottomPlane, double viewDepth)
        {
            if (view is null) return;
            var viewRange = view.GetViewRange();
            if (!viewRange.IsValidObject) return;
            viewRange.SetLevelId(PlanViewPlane.TopClipPlane, level.Id);
            viewRange.SetLevelId(PlanViewPlane.CutPlane, level.Id);
            viewRange.SetLevelId(PlanViewPlane.BottomClipPlane, level.Id);
            viewRange.SetLevelId(PlanViewPlane.ViewDepthPlane, level.Id);
            viewRange.SetOffset(PlanViewPlane.TopClipPlane, topPlane);
            viewRange.SetOffset(PlanViewPlane.CutPlane, cutPlane);
            viewRange.SetOffset(PlanViewPlane.BottomClipPlane, bottomPlane);
            viewRange.SetOffset(PlanViewPlane.ViewDepthPlane, viewDepth);
            if (!viewRange.IsValidObject) return;
            view.SetViewRange(viewRange);
        }
        public static ViewSection CreateViewSection(
            this Document document,
            Curve lineOnPlan,
            double heightSection,
            double depthSection,
            string nameSection)
        {
            ViewSection result = null;
            try
            {
                var viewSections = document.GetViewsFromClass<ViewSection>(false).Select(x => x as View).ToList();
                IsExistedViewName(nameSection, viewSections, out string nameViewGenerate);
                var sectionType = new FilteredElementCollector(document)
                .OfClass(typeof(ViewFamilyType))
                .Cast<ViewFamilyType>()
                .FirstOrDefault(x => x.ViewFamily == ViewFamily.Section);

                var trs = Transform.Identity;
                trs.Origin = lineOnPlan.Mid();
                trs.BasisX = lineOnPlan.Direction();
                trs.BasisY = XYZ.BasisZ;
                trs.BasisZ = lineOnPlan.Direction().CrossProduct(XYZ.BasisZ);

                var box = new BoundingBoxXYZ();
                box.Transform = trs;
                box.Min = new XYZ(-lineOnPlan.Length / 2, -heightSection / 2, -depthSection / 2);
                box.Max = new XYZ(lineOnPlan.Length / 2, heightSection / 2, depthSection / 2);

                result = ViewSection.CreateSection(document, sectionType.Id, box);
                result.Name = nameViewGenerate;
            }
            catch (Exception)
            {
            }
            return result;
        }
    }
}
