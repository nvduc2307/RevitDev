using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using RevitDevelop.Utils.Compares;
using RevitDevelop.Utils.FilterElementsInRevit;
using RevitDevelop.Utils.Geometries;
using RevitDevelop.Utils.NumberUtils;
using RevitDevelop.Utils.RevParameters;
using RevitDevelop.Utils.SkipWarning;
using RevitDevelop.Utils.Solids;
using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace RevitDevelop.Utils.RevDuct
{
    public static class RevDuctUtils
    {
        public static FlexDuct DuctToFlexDuct(
            this Duct duct,
            List<XYZ> ps,
            MechanicalSystemType systemType,
            List<FlexDuctType> flexDucTypes)
        {
            if (!flexDucTypes.Any()) return null;
            if (systemType == null) return null;
            var extent = 50.MmToFoot();
            var document = duct.Document;
            var location = duct.Location as LocationCurve;
            if (location == null) return null;
            var ductType = duct.DuctType.Shape;
            var flexDucType = flexDucTypes.FirstOrDefault(x => x.Shape == ductType);
            if (flexDucType == null) return null;
            var level = duct.LevelId;
            var qPs = ps.Count;
            var pStart = ps.FirstOrDefault();
            var pEnd = ps.LastOrDefault();
            var vtStart = (ps[1] - ps[0]).Normalize();
            var vtEnd = (ps[qPs - 1] - ps[qPs - 2]).Normalize();
            var fd = FlexDuct.Create(document, systemType.Id, flexDucType.Id, level, ps);
            fd.StartTangent = vtStart * extent;
            fd.EndTangent = vtEnd * extent;
            switch (ductType)
            {
                case ConnectorProfileType.Invalid:
                    break;
                case ConnectorProfileType.Round:
                    fd.SetParameterValue(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM, $"{duct.Diameter}");
                    break;
                case ConnectorProfileType.Rectangular:
                    fd.SetParameterValue(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM, $"{duct.Height}");
                    fd.SetParameterValue(BuiltInParameter.RBS_CURVE_WIDTH_PARAM, $"{duct.Width}");
                    break;
                case ConnectorProfileType.Oval:
                    break;
            }
            return fd;
        }
        public static List<XYZ> ConvertConnectorToPoint(this List<Connector> connectors, int numDot, double spacingMm)
        {
            var result = new List<XYZ>();
            if (connectors.Count % 2 != 0)
                return result;
            if (connectors.GroupBy(x => x.Owner.Id.ToString()).Any(x => x.Count() != 2))
                return result;
            var qConnectors = connectors.Count;
            for (int i = 0; i < qConnectors; i += 2)
            {
                if (connectors[i].Owner.Id.ToString() != connectors[i + 1].Owner.Id.ToString())
                    return result;
                var distance = connectors[i].Origin.DistanceTo(connectors[i + 1].Origin).FootToMm();
                var vt = (connectors[i + 1].Origin - connectors[i].Origin).Normalize();
                result.Add(connectors[i].Origin);
                if (distance >= numDot * 2 * spacingMm)
                {
                    if (connectors[i].Owner is Duct)
                    {
                        for (int j = 0; j < numDot; j++)
                        {
                            result.Add(connectors[i].Origin + (j + 1) * vt * spacingMm.MmToFoot());
                        }
                        for (int j = numDot - 1; j >= 0; j--)
                        {
                            result.Add(connectors[i + 1].Origin - (j + 1) * vt * spacingMm.MmToFoot());
                        }
                    }
                    if (connectors[i].Owner is FamilyInstance)
                    {
                        //result.Add(connectors[i].Origin + vt * spacingMm.MmToFoot());
                        //result.Add(connectors[i + 1].Origin - vt * spacingMm.MmToFoot());
                    }
                }
                result.Add(connectors[i + 1].Origin);
            }
            return result.Distinct(new ComparePoint()).ToList();
        }
        public static List<Connector> SortConnector(this List<Element> elements)
        {
            var connectors =
                elements.Select(x => x.GetConnectors())
                .Aggregate((a, b) => a.Concat(b).ToList())
                .ToList();
            var qConnector = connectors.Count;
            Connector connectorStart = null;
            Connector connectorEnd = null;
            for (int i = 0; i < qConnector; i++)
            {
                var connect = connectors.Where(x => x.Owner.Id.ToString() != connectors[i].Owner.Id.ToString())
                    .FirstOrDefault(x => x.Origin.IsSame(connectors[i].Origin));
                if (connect == null)
                {
                    connectorStart = connectors[i];
                    connectorEnd = connectors.Where(x => x.Owner.Id.ToString() == connectorStart.Owner.Id.ToString())
                        .FirstOrDefault(x => x.Id != connectorStart.Id);
                    break;
                }
            }
            if (connectorStart == null)
                return new List<Connector>();
            if (connectorEnd == null)
                return new List<Connector>();
            var results = new List<Connector>();
            var elementStart = connectorStart.Owner;
            results.Add(connectorStart);
            results.Add(connectorEnd);
            var isDo = true;
            do
            {
                try
                {
                    var connectorsStart = connectors.Where(x => x.Owner.Id.ToString() != connectorStart.Owner.Id.ToString());
                    if (!connectorsStart.Any())
                        throw new Exception();
                    var connectorStartAdd = connectorsStart
                        .FirstOrDefault(x => x.Origin.IsSame(connectorEnd.Origin));
                    if (connectorStartAdd == null)
                        throw new Exception();
                    var connectorsEnd = connectors.Where(x => x.Owner.Id.ToString() == connectorStartAdd.Owner.Id.ToString());
                    if (!connectorsEnd.Any())
                        throw new Exception();
                    var connectorEndAdd = connectorsEnd
                        .FirstOrDefault(x => x.Id != connectorStartAdd.Id);
                    results.Add(connectorStartAdd);
                    results.Add(connectorEndAdd);
                    connectorStart = connectorStartAdd;
                    connectorEnd = connectorEndAdd;
                }
                catch (Exception)
                {
                    isDo = false;
                }
            } while (isDo);
            return results;
        }
        public static List<Element> SortElement(this List<Element> elements)
        {
            var connectors =
                elements.Select(x => x.GetConnectors())
                .Aggregate((a, b) => a.Concat(b).ToList())
                .ToList();
            var qConnector = connectors.Count;
            Connector connectorStart = null;
            for (int i = 0; i < qConnector; i++)
            {
                var connect = connectors.Where(x => x.Owner.Id.ToString() != connectors[i].Owner.Id.ToString())
                    .FirstOrDefault(x => x.Origin.IsSame(connectors[i].Origin));
                if (connect == null)
                {
                    connectorStart = connectors[i];
                    break;
                }
            }
            if (connectorStart == null)
                return new List<Element>();
            var results = new List<Element>();
            var elementStart = connectorStart.Owner;
            results.Add(elementStart);
            var isDo = true;
            do
            {
                try
                {
                    var connectorStarts = elementStart.GetConnectors();
                    var elementNext = elements
                        .Where(x => !results.Any(y => y.Id.ToString() == x.Id.ToString()))
                        .FirstOrDefault(x => x.GetConnectors().Any(y => connectorStarts.Any(z => z.Origin.IsSame(y.Origin))));
                    if (elementNext == null)
                        throw new Exception();
                    results.Add(elementNext);
                    elementStart = elementNext;
                }
                catch (Exception)
                {
                    isDo = false;
                }
            } while (isDo);
            if (results.FirstOrDefault() is not Duct)
                results.RemoveAt(0);
            if (results.LastOrDefault() is not Duct)
                results.RemoveAt(results.Count - 1);
            return results;
        }
        public static List<Element> GetGroupDuct(this Element ele)
        {
            var result = new List<Element>() { ele };
            var qelementsTarget = result.Count;
            for (int i = 0; i < qelementsTarget; i++)
            {
                if (result[i].GetType() != typeof(Duct) && result[i].GetType() != typeof(FamilyInstance))
                    continue;
                var document = result[i].Document;
                var connectors = result[i].GetType() == typeof(Duct)
                    ? (result[i] as Duct).GetConnectors()
                    : (result[i] as FamilyInstance).GetConnectors();
                if (!connectors.Any()) continue;
                if (connectors.Count != 2) continue;
                var elesAdd = new List<Element>();
                foreach (var connector in connectors)
                {
                    var els = GetGroupDuct(document, connector)
                        .Where(x => !result.Any(y => y.Id.ToString() == x.Id.ToString()));
                    if (els.Any())
                        elesAdd.AddRange(els);
                }
                if (elesAdd.Any())
                {
                    result.AddRange(elesAdd);
                    qelementsTarget = result.Count;
                }
            }
            result = result.Where(x =>
            {
                if (x is not Duct duct) return true;
                var connects = duct.GetConnectors();
                var distance = connects.FirstOrDefault().Origin
                .DistanceTo(connects.LastOrDefault().Origin)
                .FootToMm();
                return distance > 5;
            }).ToList();
            return SortElement(result);
        }
        public static List<Element> GetGroupDuct(this Document document, Connector connector)
        {
            var result = new List<Element>();
            var extent = 40.MmToFoot();
            var pMin = connector.Origin
                    - XYZ.BasisX * extent / 2
                    - XYZ.BasisY * extent / 2
                    - XYZ.BasisZ * extent / 2;
            var pMax = connector.Origin
                + XYZ.BasisX * extent / 2
                + XYZ.BasisY * extent / 2
                + XYZ.BasisZ * extent / 2;
            var outLine = new Outline(pMin, pMax);
            var bbFilter = new BoundingBoxIntersectsFilter(outLine);
            result = new FilteredElementCollector(document)
            .WhereElementIsNotElementType()
            .WherePasses(bbFilter)
            .Where(x => x.Category.BuiltInCategory == BuiltInCategory.OST_DuctCurves || x.Category.BuiltInCategory == BuiltInCategory.OST_DuctFitting)
            .Where(x => x.GetConnectors().Any(y => y.Origin.IsSame(connector.Origin)))
            .ToList();
            return result;
        }
        public static List<Connector> GetConnectors(this Element element)
        {
            var result = new List<Connector>();
            var dk1 = element.Category.BuiltInCategory == BuiltInCategory.OST_DuctCurves;
            var dk2 = element.Category.BuiltInCategory == BuiltInCategory.OST_DuctFitting;
            var dk3 = element.Category.BuiltInCategory == BuiltInCategory.OST_DuctAccessory;
            if (!dk1 && !dk2 && !dk3)
                return result;
            result = element is Duct
                ? (element as Duct).GetConnectors()
                : (element as FamilyInstance).GetConnectors();
            return result;
        }
        public static List<Connector> GetConnectors(this Duct duct)
        {
            var result = new List<Connector>();
            var connectorManager = duct.ConnectorManager.Connectors;
            foreach (var item in duct.ConnectorManager.Connectors)
            {
                if (item is Connector connector)
                    result.Add(connector);
            }
            return result;
        }
        public static List<Connector> GetConnectors(this FamilyInstance ductAccessories)
        {
            var result = new List<Connector>();
            var buildCate = ductAccessories.Category.BuiltInCategory;
            if (buildCate == BuiltInCategory.OST_DuctAccessory || buildCate == BuiltInCategory.OST_DuctFitting)
            {
                var mepModel = ductAccessories.MEPModel;
                foreach (var item in mepModel.ConnectorManager.Connectors)
                {
                    if (item is Connector connector)
                        result.Add(connector);
                }
                return result;
            }
            return result;
        }
    }
}
