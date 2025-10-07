using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitDevelop.Utils.RevSketchPlan
{
    public static class RevSketchPlanUtils
    {
        public static void SetSketchPlan(this Document document)
        {
            try
            {
                var plane = Plane.CreateByNormalAndOrigin(document.ActiveView.ViewDirection, document.ActiveView.Origin);
                var sketchPlane = SketchPlane.Create(document, plane);
                document.ActiveView.SketchPlane = sketchPlane;
                if (document.ActiveView.SketchPlane is null)
                {

                }
            }
            catch (Exception)
            {
            }
        }
    }
}
