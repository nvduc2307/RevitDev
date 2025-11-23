using Autodesk.AutoCAD.Geometry;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using RevitDevelop.Utils.SelectFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using Plane = Autodesk.Revit.DB.Plane;

namespace RevitDevelop.Test
{
    [Transaction(TransactionMode.Manual)]
    public class OverrideCenterLineInViewCommand : IExternalCommand
    {
        public const double TOLERANCE = 1.0e-5;
        public const double MIN_LENGTH = 1 / 304.8;
        public const double ANGLE_TOLERANCE = 5.0e-3;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            View view = doc.ActiveView;

            try
            {
                var refs = uidoc.Selection.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Element, new GenericSelectionFilter(BuiltInCategory.OST_FlexPipeCurves), "Select object").ToList();

                var elems = new List<FlexPipe>();
                foreach (var item in refs)
                {
                    var ele = doc.GetElement(item);

                    if (ele != null && ele is FlexPipe flexPipe)
                    {
                        elems.Add(flexPipe);
                    }
                }

                using (Transaction trans = new Transaction(doc, "Override Center Line"))
                {
                    trans.Start();

                    foreach (var flexPipe in elems)
                    {
                        var start = flexPipe.Points.FirstOrDefault();
                        var end = flexPipe.Points.LastOrDefault();

                        DrawLine2d(doc, start, end);
                    }

                    var flexPipeCat = doc.Settings.Categories.get_Item(BuiltInCategory.OST_FlexPipeCurves);
                    if (flexPipeCat != null)
                        view.SetCategoryHidden(flexPipeCat.Id, true);

                    if (view.DetailLevel != ViewDetailLevel.Coarse)
                        view.DetailLevel = ViewDetailLevel.Coarse;

                    trans.Commit();
                }
            }
            catch (Exception)
            {
                throw;
            }

            return Result.Succeeded;
        }

        public static void DrawLine2d(Document doc, XYZ start, XYZ end)
        {
            if (doc.ActiveView.ViewType == ViewType.ThreeD || doc.ActiveView is ViewSheet) return;

            if (start != null && end != null && !start.IsAlmostEqualTo(end))
            {
                var dist = start.DistanceTo(end);
                if (dist > MIN_LENGTH)
                {
                    var pl = Plane.CreateByNormalAndOrigin(doc.ActiveView.ViewDirection, doc.ActiveView.Origin);
                    XYZ a = ProjectOnto(pl, start);
                    XYZ b = ProjectOnto(pl, end);

                    var line = Line.CreateBound(start, end);

                    var dc = doc.Create.NewDetailCurve(doc.ActiveView, line);

                    var ogs = new OverrideGraphicSettings();
                    ogs.SetProjectionLineColor(new Color(255, 0, 0));
                    doc.ActiveView.SetElementOverrides(dc.Id, ogs);
                }
            }
        }

        public static void DrawLine(Document doc, XYZ start, XYZ end)
        {
            if (start != null && end != null && !start.IsAlmostEqualTo(end))
            {
                var dist = start.DistanceTo(end);
                if (dist > MIN_LENGTH)
                {
                    var line = Line.CreateBound(start, end);

                    var vec = IsParallel(line.Direction, XYZ.BasisX) ? XYZ.BasisY : XYZ.BasisX;
                    var normal = line.Direction.CrossProduct(vec).Normalize();
                    var pl = Plane.CreateByNormalAndOrigin(normal, start);
                    var sp = SketchPlane.Create(doc, pl);

                    doc.Create.NewModelCurve(line, sp);
                }
            }
        }

        public static double GetSignedDistance(Plane plane, XYZ point)
        {
            XYZ source = point - plane.Origin;
            return plane.Normal.DotProduct(source);
        }

        public static XYZ ProjectOnto(Plane plane, XYZ point)
        {
            double signedDistance = GetSignedDistance(plane, point);
            return point - plane.Normal * signedDistance;
        }

        public static bool IsEqual(double first, double second, double tolerance = TOLERANCE)
        {
            double result = Math.Abs(first - second);
            return result <= tolerance;
        }

        public static bool IsParallel(XYZ first, XYZ second, double tolerance = ANGLE_TOLERANCE)
        {
            XYZ product = first.CrossProduct(second);
            double length = product.GetLength();
            return IsEqual(length, 0, tolerance);
        }
    }
}