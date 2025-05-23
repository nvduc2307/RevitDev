using Autodesk.Revit.DB.Structure;
using RevitDevelop.Utils.FilterElementsInRevit;
using RevitDevelop.Utils.Geometries;
using RevitDevelop.Utils.Messages;
using RevitDevelop.Utils.NumberUtils;
using RevitDevelop.Utils.Paths;
using RevitDevelop.Utils.RevCurves;
using RevitDevelop.Utils.RevParameters;
using RevitDevelop.Utils.RevRebars;
using System.IO;

namespace RevitDevelop.Utils.RevFamilies
{
    public static class RevFamilyUtils
    {
        private const string _extFamily = ".rfa";
        public static void LoadFamily(this Document document, List<string> pathFamilies)
        {
            var familySymbols = document.GetElementsFromClass<FamilySymbol>();
            foreach (var pathFamily in pathFamilies)
            {
                try
                {
                    var nameFamily = pathFamily.Split('/').LastOrDefault().Split('.').FirstOrDefault();
                    if (familySymbols.Any(x => x.Name.ToUpper() == nameFamily.ToUpper()))
                        continue;
                    LoadFamily(document, pathFamily);
                }
                catch (Exception)
                {
                }
            }
        }
        public static void LoadFamily(this Document document, string pathFamily)
        {
            try
            {
                if (!File.Exists(pathFamily))
                    return;
                document.LoadFamily(pathFamily, new FamilyLoadOptions(), out Family family);
            }
            catch (Exception)
            {
            }
        }
        public static void CreateNewFamilyDetailItem(
            this Autodesk.Revit.ApplicationServices.Application application,
            string nameFamily,
            List<XYZ> points,
            out string pathSaveFamily)
        {
            pathSaveFamily = string.Empty;
            string templatePath = $"{PathUtils.FamiliesBase}\\Metric Detail Item.rft";
            Document familyDoc = application.NewFamilyDocument(templatePath);
            try
            {
                if (points.Count == 1) return;
                using (Transaction tx = new Transaction(familyDoc, "Draw Detail Lines"))
                {
                    tx.Start();
                    var cs = points.PointsToCurves();
                    var view = familyDoc.GetElementsFromClass<ViewPlan>(false).FirstOrDefault();
                    foreach (var c in cs)
                    {
                        DetailCurve detailCurve = familyDoc.FamilyCreate.NewDetailCurve(view, c);
                    }
                    tx.Commit();
                }
                pathSaveFamily = $"C:\\Temp\\{nameFamily}.rfa";
                familyDoc.SaveAs(pathSaveFamily, new SaveAsOptions() { OverwriteExistingFile = true });
                familyDoc.Close(false);
            }
            catch (Exception ex)
            {
                IO.ShowException(ex);
                familyDoc.Close(false);
            }
        }
        public static void CreateFamilyRebarDetailItemOnPlanView(
            this Autodesk.Revit.ApplicationServices.Application application,
            Rebar rebar,
            out string pathSaveFamily)
        {
            pathSaveFamily = string.Empty;
            try
            {
                var document = rebar.Document;
                string pathFamilyRebarDetailItemBase = $"{PathUtils.FamiliesBase}\\RebarDetailBase.rfa";
                if (!File.Exists(pathFamilyRebarDetailItemBase))
                    return;
                var directionSave = $"C:\\Temp\\{document.ProjectInformation.UniqueId}";
                if (!Directory.Exists(directionSave))
                    Directory.CreateDirectory(directionSave);
                pathSaveFamily = $"{directionSave}\\{rebar.Id}.rfa";
                if (File.Exists(pathSaveFamily))
                    File.Delete(pathSaveFamily);
                File.Copy(pathFamilyRebarDetailItemBase, pathSaveFamily);
                if (!File.Exists(pathSaveFamily))
                    return;
                var familyDoc = application.OpenDocumentFile(pathSaveFamily);
                try
                {
                    var annoteType = new FilteredElementCollector(familyDoc)
                        .OfClass(typeof(FamilyInstance))
                        .FirstOrDefault(x => x.Name == "length text2");
                    if (annoteType == null)
                        throw new Exception();
                    if (annoteType is not AnnotationSymbol annotationSymbol)
                        throw new Exception();
                    var location = (annotationSymbol.Location as LocationPoint).Point;
                    var curvesOnPlan = rebar.GetCurvesGenerateOnPlane();
                    if (!curvesOnPlan.Any())
                        throw new Exception();
                    var rebarShape = document.GetElement(rebar.get_Parameter(BuiltInParameter.REBAR_SHAPE).AsElementId()) as RebarShape;
                    var segments = RebarShapeUtils.GetSegmentParamNames(rebarShape);
                    var hasStartHook = rebar.HasStartHook();
                    var hasEndHook = rebar.HasEndHook();
                    var isWiskClock = curvesOnPlan.GetPoints().IsCounterClockWise();
                    using (Transaction tx = new Transaction(familyDoc, "Draw Detail Lines"))
                    {
                        tx.Start();
                        var view = familyDoc.GetElementsFromClass<ViewPlan>(false).FirstOrDefault();
                        var lineStyle = familyDoc.GetElementsFromClass<GraphicsStyle>(false)
                            .FirstOrDefault(x => x.Name == "HC_Rebar_Detail");
                        var c = 0;
                        var qcurvesOnPlan = curvesOnPlan.Count;
                        foreach (var cur in curvesOnPlan)
                        {
                            try
                            {
                                DetailCurve detailCurve = familyDoc.FamilyCreate.NewDetailCurve(view, cur);
                                detailCurve.LineStyle = lineStyle;
                                if (cur is Line)
                                {
                                    var s = hasStartHook
                                    ? c - 1
                                    : c;
                                    var textVal = "";
                                    if (c == 0)
                                    {
                                        if (!hasStartHook)
                                            textVal = qcurvesOnPlan == 1
                                                ? Math.Round(double.Parse(rebar.GetParameterValue(segments[s])).FootToMm(), 0).RoundUp(10).ToString()
                                                : Math.Round(double.Parse(rebar.GetParameterValue(segments[s])).FootToMm(), 0).ToString();
                                    }
                                    else if (c == qcurvesOnPlan - 1)
                                    {
                                        if (!hasEndHook)
                                            textVal = qcurvesOnPlan == 1
                                                ? Math.Round(double.Parse(rebar.GetParameterValue(segments[s])).FootToMm(), 0).RoundUp(10).ToString()
                                                : Math.Round(double.Parse(rebar.GetParameterValue(segments[s])).FootToMm(), 0).ToString();
                                    }
                                    else
                                    {
                                        if (s >= 0)
                                            textVal = qcurvesOnPlan == 1
                                                ? Math.Round(double.Parse(rebar.GetParameterValue(segments[s])).FootToMm(), 0).RoundUp(10).ToString()
                                                : Math.Round(double.Parse(rebar.GetParameterValue(segments[s])).FootToMm(), 0).ToString();
                                    }

                                    if (!string.IsNullOrEmpty(textVal))
                                    {
                                        var vtz = isWiskClock
                                            ? -XYZ.BasisZ
                                            : XYZ.BasisZ;
                                        var mid = cur.Mid();
                                        var dir = cur.Direction();
                                        var nor = dir.CrossProduct(vtz);
                                        var angle = dir.X >= 0
                                            ? -nor.AngleTo(XYZ.BasisY)
                                            : nor.AngleTo(XYZ.BasisY);
                                        var ids = annotationSymbol.Copy(mid - location);
                                        var note = familyDoc.GetElement(ids.FirstOrDefault());
                                        note.SetParameterValue("Text", textVal);
                                        if (!angle.IsAlmostEqual(0) && !angle.IsAlmostEqual(Math.PI))
                                            note.Rotate(Line.CreateUnbound(mid, XYZ.BasisZ), angle);
                                    }
                                    c++;
                                }
                            }
                            catch (Exception)
                            {
                            }
                        }
                        familyDoc.Delete(annoteType.Id);
                        tx.Commit();
                    }
                    familyDoc.Close(true);
                }
                catch (Exception)
                {
                    familyDoc.Close(false);
                    if (File.Exists(pathSaveFamily))
                        File.Delete(pathSaveFamily);
                }
            }
            catch (Exception)
            {
            }
        }
        public static void EditFamilyRebarDetailItem(
            this Autodesk.Revit.ApplicationServices.Application application,
            Rebar rebar,
            out string pathSaveFamily,
            string nameFamily = "")
        {
            pathSaveFamily = string.Empty;
            try
            {
                var document = rebar.Document;
                string pathFamilyRebarDetailItemBase = $"{PathUtils.FamiliesBase}\\RebarDetailBase.rfa";
                if (!File.Exists(pathFamilyRebarDetailItemBase))
                    return;
                var directionSave = $"C:\\Temp\\{document.ProjectInformation.UniqueId}";
                if (!Directory.Exists(directionSave))
                    Directory.CreateDirectory(directionSave);
                pathSaveFamily = string.IsNullOrEmpty(nameFamily)
                    ? $"{directionSave}\\{rebar.Id}.rfa"
                    : $"{directionSave}\\{nameFamily}.rfa";
                if (File.Exists(pathSaveFamily))
                    File.Delete(pathSaveFamily);
                File.Copy(pathFamilyRebarDetailItemBase, pathSaveFamily);
                if (!File.Exists(pathSaveFamily))
                    return;
                var familyDoc = application.OpenDocumentFile(pathSaveFamily);
                try
                {
                    var annoteType = new FilteredElementCollector(familyDoc)
                        .OfClass(typeof(FamilyInstance))
                        .FirstOrDefault(x => x.Name == "HC_REBAR_TEXT_NOTE");
                    if (annoteType == null)
                        throw new Exception();
                    if (annoteType is not AnnotationSymbol annotationSymbol)
                        throw new Exception();
                    var location = (annotationSymbol.Location as LocationPoint).Point;
                    var curvesOnPlan = rebar.GetCurvesGenerateOnPlane();
                    if (!curvesOnPlan.Any())
                        throw new Exception();
                    var rebarShape = document.GetElement(rebar.get_Parameter(BuiltInParameter.REBAR_SHAPE).AsElementId()) as RebarShape;
                    var segments = RebarShapeUtils.GetSegmentParamNames(rebarShape);
                    var hasStartHook = rebar.HasStartHook();
                    var hasEndHook = rebar.HasEndHook();
                    var vtMove = new XYZ() - curvesOnPlan.FirstOrDefault().GetEndPoint(0);
                    using (Transaction tx = new Transaction(familyDoc, "Draw Detail Lines"))
                    {
                        tx.Start();
                        var view = familyDoc.GetElementsFromClass<ViewPlan>(false).FirstOrDefault();
                        var lineStyle = familyDoc.GetElementsFromClass<GraphicsStyle>(false)
                            .FirstOrDefault(x => x.Name == "HC_Rebar_Detail");
                        var c = 0;
                        var qcurvesOnPlan = curvesOnPlan.Count;
                        var center = curvesOnPlan.GetPoints().CenterPoint();
                        foreach (var cur in curvesOnPlan)
                        {
                            try
                            {
                                DetailCurve detailCurve = familyDoc.FamilyCreate.NewDetailCurve(view, cur);
                                detailCurve.LineStyle = lineStyle;
                                detailCurve.Move(vtMove);
                                if (cur is Line)
                                {
                                    var s = hasStartHook
                                    ? c - 1
                                    : c;
                                    var textVal = "";
                                    if (c == 0)
                                    {
                                        if (!hasStartHook)
                                            textVal = qcurvesOnPlan == 1
                                                ? Math.Round(double.Parse(rebar.GetParameterValue(segments[s])).FootToMm(), 0).RoundUp(10).ToString()
                                                : Math.Round(double.Parse(rebar.GetParameterValue(segments[s])).FootToMm(), 0).ToString();
                                    }
                                    else if (c == qcurvesOnPlan - 1)
                                    {
                                        if (!hasEndHook)
                                            textVal = qcurvesOnPlan == 1
                                                ? Math.Round(double.Parse(rebar.GetParameterValue(segments[s])).FootToMm(), 0).RoundUp(10).ToString()
                                                : Math.Round(double.Parse(rebar.GetParameterValue(segments[s])).FootToMm(), 0).ToString();
                                    }
                                    else
                                    {
                                        if (s >= 0)
                                            textVal = qcurvesOnPlan == 1
                                                ? Math.Round(double.Parse(rebar.GetParameterValue(segments[s])).FootToMm(), 0).RoundUp(10).ToString()
                                                : Math.Round(double.Parse(rebar.GetParameterValue(segments[s])).FootToMm(), 0).ToString();
                                    }
                                    if (!string.IsNullOrEmpty(textVal))
                                    {
                                        var mid = cur.Mid();
                                        var vtCheck = (mid - center);
                                        var dir = cur.Direction();
                                        var nor = dir.CrossProduct(-XYZ.BasisZ);
                                        if (qcurvesOnPlan > 1)
                                        {
                                            nor = nor.DotProduct(vtCheck).IsGreater(0)
                                                ? nor
                                                : -nor;
                                        }
                                        var angle = dir.X >= 0
                                            ? -nor.AngleTo(XYZ.BasisY)
                                            : nor.AngleTo(XYZ.BasisY);
                                        var angleDeg = Math.Abs(Math.Round(angle.ToDegrees(), 0));
                                        var ids = annotationSymbol.Copy(mid - location);
                                        var note = familyDoc.GetElement(ids.FirstOrDefault());
                                        note.SetParameterValue("Text", textVal);
                                        if (!angleDeg.IsAlmostEqual(0, 10) && !angleDeg.IsAlmostEqual(180, 10))
                                            note.Rotate(Line.CreateUnbound(mid, XYZ.BasisZ), angle);
                                        note.Move(vtMove);
                                    }
                                    c++;
                                }
                            }
                            catch (Exception)
                            {
                            }
                        }
                        familyDoc.Delete(annoteType.Id);
                        tx.Commit();
                    }
                    familyDoc.Close(true);
                }
                catch (Exception)
                {
                    familyDoc.Close(false);
                    if (File.Exists(pathSaveFamily))
                        File.Delete(pathSaveFamily);
                }
            }
            catch (Exception)
            {
            }
        }
    }
    public class FamilyLoadOptions : IFamilyLoadOptions
    {
        public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
        {
            overwriteParameterValues = true;
            return true;
        }

        public bool OnSharedFamilyFound(Family sharedFamily, bool familyInUse, out FamilySource source, out bool overwriteParameterValues)
        {
            source = FamilySource.Family;
            overwriteParameterValues = true;
            return true;
        }
    }
}
