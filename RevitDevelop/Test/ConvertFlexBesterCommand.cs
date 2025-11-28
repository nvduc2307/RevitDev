using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using RevitDevelop.Utils.RevPipe;
using RevitDevelop.Utils.SelectFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitDevelop.Test
{
    [Transaction(TransactionMode.Manual)]
    public class ConvertFlexBesterCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            double distance = 300 / 304.8; // feet

            try
            {
                using (Transaction tr = new Transaction(doc, "Create Flex"))
                {
                    tr.Start();

                    //var refs = uidoc.Selection.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Element, new GenericSelectionFilter(new List<BuiltInCategory> { BuiltInCategory.OST_PipeCurves, BuiltInCategory.OST_DuctCurves }), "Select Mep");
                    //List<Pipe> pipes = refs.Select(r => doc.GetElement(r) as Pipe).Where(x => x != null).ToList();
                    //List<Duct> ducts = refs.Select(r => doc.GetElement(r) as Duct).Where(x => x != null).ToList();

                    var elem = uidoc.Selection.PickElement(doc, BuiltInCategory.OST_PipeCurves);

                    if (elem is Pipe pipe)
                    {
                        var pipes = elem.GetElementsByConnector();

                        var lstMepCurve = pipes.Select(x => doc.GetElement(x.Id) as Pipe).Where(x => x != null).Cast<MEPCurve>().ToList();

                        var edgePairs = GetPairOfEdges(lstMepCurve, distance);

                        List<XYZ> ControlPointFlex = new List<XYZ>();
                        foreach (var pair in edgePairs)
                        {
                            ControlPointFlex.AddRange(pair);
                        }

                        var pipeFirst = pipes.FirstOrDefault();
                        var pipeLast = pipes.LastOrDefault();

                        double diameter = pipe.Diameter;

                        XYZ startTagent = pipeFirst.Location is LocationCurve locCurFirst && locCurFirst.Curve is Line lineFirst
                            ? (lineFirst.GetEndPoint(1) - lineFirst.GetEndPoint(0)).Normalize()
                            : null;

                        XYZ endTagent = pipeLast.Location is LocationCurve locCurLast && locCurLast.Curve is Line lineLast
                            ? (lineLast.GetEndPoint(1) - lineLast.GetEndPoint(0)).Normalize()
                            : null;

                        ElementId systemTypeId = null;
                        ElementId levelId = pipeFirst.LevelId;

                        HermiteSpline hermiteSplineFlex = HermiteSpline.Create(ControlPointFlex, false);

                        FilteredElementCollector pipeTypes = new FilteredElementCollector(doc).OfClass(typeof(PipingSystemType));
                        foreach (MEPSystemType MEPSysType in pipeTypes.Cast<MEPSystemType>())
                        {
                            systemTypeId = MEPSysType.Id;
                            break;
                        }

                        doc.Delete(pipes.Select(p => p.Id).ToList());

                        FlexPipe flex = FlexPipe.Create(doc, systemTypeId, new ElementId(142445), levelId, startTagent, endTagent, hermiteSplineFlex.ControlPoints);

                        SetValueParameterBuiltIn(flex, BuiltInParameter.RBS_PIPE_DIAMETER_PARAM, diameter);

                        doc.Regenerate();
                    }

                    var elem2 = uidoc.Selection.PickElement(doc, BuiltInCategory.OST_DuctCurves);

                    if (elem2 is Duct duct)
                    {
                        var ducts = Utils.RevDuct.RevDuctUtils.GetElementsByConnector(elem2);

                        var lstMepCurve = ducts.Select(x => doc.GetElement(x.Id) as Duct).Where(x => x != null).Cast<MEPCurve>().ToList();

                        var edgePairs = GetPairOfEdges(lstMepCurve, distance);
                        List<XYZ> ControlPointFlex = new List<XYZ>();
                        foreach (var pair in edgePairs)
                        {
                            ControlPointFlex.AddRange(pair);
                        }

                        var ductFirst = ducts.FirstOrDefault();
                        var ductLast = ducts.LastOrDefault();

                        double diameter = duct.Diameter;
                        ElementId levelId = duct.LevelId;

                        XYZ startTagent = ductFirst.Location is LocationCurve locCurFirst && locCurFirst.Curve is Line lineFirst
                            ? (lineFirst.GetEndPoint(1) - lineFirst.GetEndPoint(0)).Normalize()
                            : null;

                        XYZ endTagent = ductLast.Location is LocationCurve locCurLast && locCurLast.Curve is Line lineLast
                            ? (lineLast.GetEndPoint(1) - lineLast.GetEndPoint(0)).Normalize()
                            : null;

                        doc.Delete(ducts.Select(d => d.Id).ToList());

                        HermiteSpline hermiteSplineFlex = HermiteSpline.Create(ControlPointFlex, false);
                        FlexDuct flex = FlexDuct.Create(doc, new ElementId(683907), new ElementId(142444), levelId, startTagent, endTagent, hermiteSplineFlex.ControlPoints);

                        SetValueParameterBuiltIn(flex, BuiltInParameter.RBS_CURVE_DIAMETER_PARAM, diameter);

                        doc.Regenerate();
                    }

                    tr.Commit();
                }
            }
            catch (Exception)
            {
                throw;
            }

            return Result.Succeeded;
        }

        public static List<XYZ> CreatePointsOnLine(Line line, double stepFt, double minLength = 1e-9)
        {
            var result = new List<XYZ>();

            if (line == null) return result;
            if (stepFt <= minLength) return result;

            var start = line.GetEndPoint(0);
            var end = line.GetEndPoint(1);

            var length = start.DistanceTo(end);
            if (length <= minLength) return result;

            var dir = (end - start).Normalize();

            result.Add(start);

            for (double d = stepFt; d < length - minLength; d += stepFt)
            {
                var p = start + dir.Multiply(d);
                result.Add(p);
            }

            result.Add(end);

            return result;
        }

        private List<List<XYZ>> GetPairOfEdges(List<MEPCurve> pipes, double distance)
        {
            var pairs = new List<List<XYZ>>();
            foreach (var pipe in pipes)
            {
                if (pipe.Location is LocationCurve locCur)
                {
                    if (locCur.Curve is Line line)
                    {
                        var endpoints = PointsByFixedStep(line.GetEndPoint(0), line.GetEndPoint(1), distance, true);

                        pairs.Add(endpoints);
                    }
                }
            }
            return pairs;
        }

        public static List<XYZ> PointsByFixedStep(XYZ a, XYZ b, double stepFt, bool includeEnds = false)
        {
            var res = new List<XYZ>();

            if (a == null || b == null) return res;

            var len = a.DistanceTo(b);

            if (len <= 1e-9 || stepFt <= 1e-9) return res;

            var dir = (b - a).Normalize();

            if (includeEnds)
            {
                for (int j = 1; j <= 3; j++)
                {
                    var d = 10 / 304.8 * j;
                    if (d < len - 1e-9) res.Add(a + dir.Multiply(d));
                }

                //res.Add(a);
            }

            for (double d = stepFt; d < len - 1e-9; d += stepFt)
                res.Add(a + dir.Multiply(d));

            if (includeEnds)
            {
                for (int j = 3; j >= 1; j--)
                {
                    var d = 10 / 304.8 * j;
                    if (d < len - 1e-9) res.Add(b - dir.Multiply(d));
                }

                //res.Add(b);
            }

            return res;
        }

        public static bool SetValueParameterBuiltIn(Element elem, BuiltInParameter paramId, object value)
        {
            return SetValueParameter(GetParameterElement(elem, paramId), value);
        }

        public static Parameter GetParameterElement(Element elem, BuiltInParameter builtIn, bool findInType = false)
        {
            Parameter result = null;
            try
            {
                string labelFor = LabelUtils.GetLabelFor(builtIn);
                foreach (Parameter parameter in (findInType ? elem.Document.GetElement(elem.GetTypeId()) : elem).GetParameters(labelFor))
                {
                    if (parameter.Id.IntegerValue == (int)builtIn)
                    {
                        result = parameter;
                        break;
                    }
                }
            }
            catch (Exception)
            {
            }

            return result;
        }

        public static bool SetValueParameter(Parameter param, object value)
        {
            if (param != null && !param.IsReadOnly && value != null)
            {
                try
                {
                    switch (param.StorageType)
                    {
                        case StorageType.Integer:
                            param.Set((int)value);
                            break;

                        case StorageType.Double:
                            param.Set((double)value);
                            break;

                        case StorageType.String:
                            param.Set((string)value);
                            break;

                        case StorageType.ElementId:
                            param.Set((ElementId)value);
                            break;
                    }

                    return true;
                }
                catch (Exception)
                {
                }
            }

            return false;
        }
    }

    public class XyzEpsComparer : IEqualityComparer<XYZ>
    {
        private readonly double _eps;

        public XyzEpsComparer(double epsFeet)
        { _eps = epsFeet; }

        public bool Equals(XYZ a, XYZ b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null || b == null) return false;
            return a.IsAlmostEqualTo(b, _eps);
        }

        public int GetHashCode(XYZ p)
        {
            int q(double v) => (int)Math.Round(v / _eps);
            unchecked { return (q(p.X) * 397) ^ (q(p.Y) * 17) ^ q(p.Z); }
        }
    }
}